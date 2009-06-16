// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// version 3.0 as published by the Free Software Foundation.
// 
// re-motion is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-motion; if not, see http://www.gnu.org/licenses.
// 
using System;
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Parsing.Structure;
using Remotion.Data.Linq.Parsing.Structure.IntermediateModel;
using Remotion.Utilities;

namespace Remotion.Data.UnitTests.Linq.Parsing.Structure.IntermediateModel
{
  [TestFixture]
  public class ConstantExpressionNodeTest : ExpressionNodeTestBase
  {
    [Test]
    public void Initialization_QuerySourceElementType ()
    {
      var node = new ConstantExpressionNode ("x", typeof (int[]), new[] {1, 2, 3});
      Assert.That (node.QuerySourceElementType, Is.EqualTo (typeof (int)));
    }

    [Test]
    [ExpectedException (typeof (ArgumentTypeException),
        ExpectedMessage =
            "Argument expression has type System.Int32 when type System.Collections.Generic.IEnumerable`1[T] was expected.\r\nParameter name: expression"
        )]
    public void Initialization_TypeNotEnumerable ()
    { 
      new ConstantExpressionNode ("x", typeof (int), 5);
    }


    [Test]
    public void Resolve_ReplacesParameter_WithQuerySourceReference ()
    {
      var node = ExpressionNodeObjectMother.CreateConstant ();
      var expression = ExpressionHelper.CreateLambdaExpression<int, bool> (i => i > 5);
      var clause = ExpressionHelper.CreateMainFromClause ();
      QuerySourceClauseMapping.AddMapping (node, clause);

      var result = node.Resolve (expression.Parameters[0], expression.Body, QuerySourceClauseMapping);

      var expectedResult = Expression.MakeBinary (ExpressionType.GreaterThan, new QuerySourceReferenceExpression (clause), Expression.Constant (5));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "Cannot resolve with a ConstantExpressionNode for which no clause was "
        + "created. Be sure to call CreateClause before calling Resolve, and pass in the same QuerySourceClauseMapping to both methods.")]
    public void Resolve_WithoutClause ()
    {
      var node = ExpressionNodeObjectMother.CreateConstant ();
      var expression = ExpressionHelper.CreateLambdaExpression<int, bool> (i => i > 5);
      node.Resolve (expression.Parameters[0], expression.Body, QuerySourceClauseMapping);
    }

    [Test]
    public void CreateParameterForOutput ()
    {
      var node = new ConstantExpressionNode ("x", typeof (int[]), new[] { 1, 2, 3, 4, 5 });
      var parameter = node.CreateParameterForOutput ();

      Assert.That (parameter.Name, Is.EqualTo ("x"));
      Assert.That (parameter.Type, Is.SameAs (typeof (int)));
    }

    [Test]
    public void CreateClause ()
    {
      var node = new ConstantExpressionNode ("x", typeof (int[]), new[] { 1, 2, 3, 4, 5 });
      var constantClause = (MainFromClause) node.CreateClause(null, QuerySourceClauseMapping);

      Assert.That (constantClause.Identifier.Name, Is.EqualTo ("x"));
      Assert.That (constantClause.Identifier.Type, Is.SameAs(typeof(int)));    
      Assert.That (constantClause.QuerySource, Is.InstanceOfType (typeof (ConstantExpression)));
      Assert.That (((ConstantExpression) constantClause.QuerySource).Value, Is.SameAs (node.Value));
      Assert.That (constantClause.QuerySource.Type, Is.SameAs (node.QuerySourceType));
    }

    [Test]
    public void CreateClause_AddsMapping ()
    {
      var node = new ConstantExpressionNode ("x", typeof (int[]), new[] { 1, 2, 3, 4, 5 });
      var querySourceClauseMapping = new QuerySourceClauseMapping();
      var clause = node.CreateClause (null, querySourceClauseMapping);

      Assert.That (querySourceClauseMapping.Count, Is.EqualTo (1));
      Assert.That (querySourceClauseMapping.GetClause (node), Is.SameAs (clause));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException))]
    public void CreateClause_WithNonNullPreviousClause ()
    {
      var node = new ConstantExpressionNode ("x", typeof (int[]), new[] { 1, 2, 3, 4, 5 });
      var querySourceClauseMapping = new QuerySourceClauseMapping ();
      node.CreateClause (ExpressionHelper.CreateClause(), querySourceClauseMapping);
    }

    [Test]
    public void CreateClause_WithNullPreviousClause ()
    {
      var node = new ConstantExpressionNode ("x", typeof (int[]), new[] { 1, 2, 3, 4, 5 });
      var querySourceClauseMapping = new QuerySourceClauseMapping ();
      var constantClause = (MainFromClause) node.CreateClause (null, querySourceClauseMapping);

      Assert.That (constantClause.Identifier.Name, Is.EqualTo ("x"));
      Assert.That (constantClause.Identifier.Type, Is.SameAs (typeof (int)));
      Assert.That (constantClause.QuerySource, Is.InstanceOfType (typeof (ConstantExpression)));
      Assert.That (((ConstantExpression) constantClause.QuerySource).Value, Is.SameAs (node.Value));
      Assert.That (constantClause.QuerySource.Type, Is.SameAs (node.QuerySourceType));
    }
  }
}
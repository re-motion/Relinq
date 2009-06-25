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
    private ClauseGenerationContext _emptyContext;
    private ConstantExpressionNode _node;

    public override void SetUp ()
    {
      base.SetUp ();
      _emptyContext = new ClauseGenerationContext (
          new QuerySourceClauseMapping(), new MethodCallExpressionNodeTypeRegistry (),new ResultModificationExpressionNodeRegistry());
      _node = new ConstantExpressionNode ("x", typeof (int[]), new[] { 1, 2, 3 });
    }

    [Test]
    public void Initialization_QuerySourceElementType ()
    {
      Assert.That (_node.QuerySourceElementType, Is.EqualTo (typeof (int)));
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
      var expression = ExpressionHelper.CreateLambdaExpression<int, bool> (i => i > 5);
      var clause = ExpressionHelper.CreateMainFromClause ();
      QuerySourceClauseMapping.AddMapping (_node, clause);

      var result = _node.Resolve (expression.Parameters[0], expression.Body, ClauseGenerationContext);

      var expectedResult = Expression.MakeBinary (ExpressionType.GreaterThan, new QuerySourceReferenceExpression (clause), Expression.Constant (5));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "Cannot resolve with a ConstantExpressionNode for which no clause was "
        + "created. Be sure to call CreateClause before calling Resolve, and pass in the same QuerySourceClauseMapping to both methods.")]
    public void Resolve_WithoutClause ()
    {
      var expression = ExpressionHelper.CreateLambdaExpression<int, bool> (i => i > 5);
      _node.Resolve (expression.Parameters[0], expression.Body, ClauseGenerationContext);
    }

    [Test]
    public void CreateParameterForOutput ()
    {
      var parameter = _node.CreateParameterForOutput ();

      Assert.That (parameter.Name, Is.EqualTo ("x"));
      Assert.That (parameter.Type, Is.SameAs (typeof (int)));
    }

    [Test]
    public void CreateClause ()
    {
      var constantClause = (MainFromClause) _node.CreateClause(null, ClauseGenerationContext);

      Assert.That (constantClause.ItemName, Is.EqualTo ("x"));
      Assert.That (constantClause.ItemType, Is.SameAs(typeof(int)));    
      Assert.That (constantClause.FromExpression, Is.InstanceOfType (typeof (ConstantExpression)));
      Assert.That (((ConstantExpression) constantClause.FromExpression).Value, Is.SameAs (_node.Value));
      Assert.That (constantClause.FromExpression.Type, Is.SameAs (_node.QuerySourceType));
    }

    [Test]
    public void CreateClause_AddsMapping ()
    {
      var clause = _node.CreateClause (null, _emptyContext);

      Assert.That (_emptyContext.ClauseMapping.Count, Is.EqualTo (1));
      Assert.That (_emptyContext.ClauseMapping.GetClause (_node), Is.SameAs (clause));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException))]
    public void CreateClause_WithNonNullPreviousClause ()
    {
      _node.CreateClause (ExpressionHelper.CreateClause(), _emptyContext);
    }

    [Test]
    public void CreateClause_WithNullPreviousClause ()
    {
      var constantClause = (MainFromClause) _node.CreateClause (null, _emptyContext);

      Assert.That (constantClause.ItemName, Is.EqualTo ("x"));
      Assert.That (constantClause.ItemType, Is.SameAs (typeof (int)));
      Assert.That (constantClause.FromExpression, Is.InstanceOfType (typeof (ConstantExpression)));
      Assert.That (((ConstantExpression) constantClause.FromExpression).Value, Is.SameAs (_node.Value));
      Assert.That (constantClause.FromExpression.Type, Is.SameAs (_node.QuerySourceType));
    }

    [Test]
    public void CreateSelectClause ()
    {
      var previousClause = ExpressionHelper.CreateClause();
      var mainFromClause = ExpressionHelper.CreateMainFromClause();

      ClauseGenerationContext.ClauseMapping.AddMapping (_node, mainFromClause);

      var selectClause = _node.CreateSelectClause (previousClause, ClauseGenerationContext);
      Assert.That (((QuerySourceReferenceExpression) selectClause.Selector).ReferencedClause, Is.SameAs (mainFromClause));
    }
  }
}
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
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Parsing.Structure.IntermediateModel;
using Remotion.Utilities;

namespace Remotion.Data.UnitTests.Linq.Parsing.Structure.IntermediateModel
{
  [TestFixture]
  public class MainSouceExpressionNodeTest : ExpressionNodeTestBase
  {
    private MainSourceExpressionNode _node;

    public override void SetUp ()
    {
      base.SetUp ();
      _node = ExpressionNodeObjectMother.CreateMainSource ();
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
      new MainSourceExpressionNode ("x", Expression.Constant(5));
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
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "Cannot resolve with a MainSourceExpressionNode for which no clause was "
        + "created. Be sure to call CreateClause before calling Resolve, and pass in the same QuerySourceClauseMapping to both methods.")]
    public void Resolve_WithoutClause ()
    {
      var expression = ExpressionHelper.CreateLambdaExpression<int, bool> (i => i > 5);
      _node.Resolve (expression.Parameters[0], expression.Body, ClauseGenerationContext);
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException))]
    public void Apply_Throws ()
    {
      _node.Apply (QueryModel, ClauseGenerationContext);
    }
  }
}
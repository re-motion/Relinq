// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// re-linq is free software; you can redistribute it and/or modify it under 
// the terms of the GNU Lesser General Public License as published by the 
// Free Software Foundation; either version 2.1 of the License, 
// or (at your option) any later version.
// 
// re-linq is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-linq; if not, see http://www.gnu.org/licenses.
// 
using System;
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Parsing.Structure.IntermediateModel;
using Remotion.Linq.Utilities;

namespace Remotion.Linq.UnitTests.Linq.Core.Parsing.Structure.IntermediateModel
{
  [TestFixture]
  public class MainSourceExpressionNodeTest : ExpressionNodeTestBase
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
        ExpectedMessage = "Expected a type implementing IEnumerable<T>, but found 'System.Int32'.\r\nParameter name: expression")]
    public void Initialization_TypeNotEnumerable ()
    { 
      new MainSourceExpressionNode ("x", Expression.Constant(5));
    }
    
    [Test]
    public void Resolve_ReplacesParameter_WithQuerySourceReference ()
    {
      var expression = ExpressionHelper.CreateLambdaExpression<int, bool> (i => i > 5);
      var clause = ExpressionHelper.CreateMainFromClause_Int ();
      ClauseGenerationContext.AddContextInfo (_node, clause);

      var result = _node.Resolve (expression.Parameters[0], expression.Body, ClauseGenerationContext);

      var expectedResult = Expression.MakeBinary (ExpressionType.GreaterThan, new QuerySourceReferenceExpression (clause), Expression.Constant (5));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "Cannot retrieve an IQuerySource for the given MainSourceExpressionNode. "
        + "Be sure to call Apply before calling methods that require IQuerySources, and pass in the same QuerySourceClauseMapping to both.")]
    public void Resolve_WithoutClause ()
    {
      var expression = ExpressionHelper.CreateLambdaExpression<int, bool> (i => i > 5);
      _node.Resolve (expression.Parameters[0], expression.Body, ClauseGenerationContext);
    }

    [Test]
    public void Apply ()
    {
      var queryModel = _node.Apply (null, ClauseGenerationContext);
      
      Assert.That (queryModel.MainFromClause, Is.Not.Null);
      Assert.That (queryModel.MainFromClause.ItemType, Is.EqualTo (typeof (int)));
      Assert.That (queryModel.MainFromClause.ItemName, Is.EqualTo ("x"));
      Assert.That (queryModel.MainFromClause.FromExpression, Is.SameAs (_node.ParsedExpression));
      Assert.That (queryModel.SelectClause, Is.Not.Null);
      Assert.That (((QuerySourceReferenceExpression) queryModel.SelectClause.Selector).ReferencedQuerySource, Is.SameAs (queryModel.MainFromClause));
      Assert.That (queryModel.ResultTypeOverride, Is.SameAs (typeof (int[])));
    }

    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void Apply_Throws ()
    {
      _node.Apply (QueryModel, ClauseGenerationContext);
    }
  }
}

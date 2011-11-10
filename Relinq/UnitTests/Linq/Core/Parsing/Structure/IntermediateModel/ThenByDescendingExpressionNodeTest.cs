// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (C) rubicon IT GmbH, www.rubicon.eu
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
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Linq.Clauses;
using Remotion.Linq.Parsing;
using Remotion.Linq.Parsing.Structure.IntermediateModel;
using Rhino.Mocks;

namespace Remotion.Linq.UnitTests.Linq.Core.Parsing.Structure.IntermediateModel
{
  [TestFixture]
  public class ThenByDescendingExpressionNodeTest : ExpressionNodeTestBase
  {
    private ThenByDescendingExpressionNode _node;

    public override void SetUp ()
    {
      base.SetUp ();

      var selector = ExpressionHelper.CreateLambdaExpression<int, bool> (i => i > 5);
      _node = new ThenByDescendingExpressionNode (CreateParseInfo (), selector);
    }

    [Test]
    public void SupportedMethod_WithoutComparer ()
    {
      AssertSupportedMethod_Generic (
          ThenByDescendingExpressionNode.SupportedMethods, 
          q => ((IOrderedQueryable<object>) q).ThenByDescending(i => i),
          e => ((IOrderedEnumerable<object>) e).ThenByDescending (i => i));
    }

    [Test]
    public void Resolve_PassesExpressionToSource ()
    {
      var sourceMock = MockRepository.GenerateMock<IExpressionNode>();
      var selector = ExpressionHelper.CreateLambdaExpression<int, int> (i => i);
      var node = new ThenByDescendingExpressionNode (CreateParseInfo (sourceMock), selector);
      var expression = ExpressionHelper.CreateLambdaExpression();
      var parameter = ExpressionHelper.CreateParameterExpression();
      var expectedResult = ExpressionHelper.CreateExpression();
      sourceMock.Expect (mock => mock.Resolve (parameter, expression, ClauseGenerationContext)).Return (expectedResult);

      var result = node.Resolve (parameter, expression, ClauseGenerationContext);

      sourceMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (expectedResult));
    }

    [Test]
    public void GetResolvedSelector ()
    {
      var expectedResult = Expression.MakeBinary (ExpressionType.GreaterThan, SourceReference, Expression.Constant (5));

      var result = _node.GetResolvedKeySelector (ClauseGenerationContext);

      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void Apply ()
    {
      var clause = new OrderByClause ();
      QueryModel.BodyClauses.Add (clause);

      var result = _node.Apply (QueryModel, ClauseGenerationContext);
      Assert.That (result, Is.SameAs (QueryModel));

      Assert.That (clause.Orderings.Count, Is.EqualTo (1));
      Assert.That (clause.Orderings[0].OrderingDirection, Is.EqualTo (OrderingDirection.Desc));
      Assert.That (clause.Orderings[0].Expression, Is.SameAs (_node.GetResolvedKeySelector (ClauseGenerationContext)));
    }

    [Test]
    public void Apply_UsesLastClause ()
    {
      var clause = new OrderByClause ();
      QueryModel.BodyClauses.Add (new OrderByClause());
      QueryModel.BodyClauses.Add (clause);

      _node.Apply (QueryModel, ClauseGenerationContext);

      Assert.That (clause.Orderings.Count, Is.EqualTo (1));
    }

    [Test]
    [ExpectedException (typeof (ParserException))]
    public void Apply_NoPreviousClause ()
    {
      _node.Apply (QueryModel, ClauseGenerationContext);
    }

    [Test]
    [ExpectedException (typeof (ParserException))]
    public void Apply_InvalidPreviousClause ()
    {
      QueryModel.BodyClauses.Add (new WhereClause (ExpressionHelper.CreateExpression ()));
      _node.Apply (QueryModel, ClauseGenerationContext);
    }
  }
}

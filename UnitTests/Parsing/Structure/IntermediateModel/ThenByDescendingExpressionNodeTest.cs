// Copyright (c) rubicon IT GmbH, www.rubicon.eu
//
// See the NOTICE file distributed with this work for additional information
// regarding copyright ownership.  rubicon licenses this file to you under 
// the Apache License, Version 2.0 (the "License"); you may not use this 
// file except in compliance with the License.  You may obtain a copy of the 
// License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, WITHOUT 
// WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.  See the 
// License for the specific language governing permissions and limitations
// under the License.
// 
using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Linq.Clauses;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.Parsing.Structure.IntermediateModel;
using Rhino.Mocks;

namespace Remotion.Linq.UnitTests.Parsing.Structure.IntermediateModel
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
    public void GetSupportedMethods ()
    {
      Assert.That (
          ThenByDescendingExpressionNode.GetSupportedMethods(),
          Is.EquivalentTo (
              new[]
              {
                  GetGenericMethodDefinition (() => Queryable.ThenByDescending<object, object> (null, null)),
                  GetGenericMethodDefinition (() => Enumerable.ThenByDescending<object, object> (null, null))
              }));
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
    public void Apply_NoPreviousClause ()
    {
      Assert.That (
          () => _node.Apply (QueryModel, ClauseGenerationContext),
          Throws.InstanceOf<NotSupportedException>()
              .With.Message.EqualTo ("ThenByDescending expressions must follow OrderBy, OrderByDescending, ThenBy, or ThenByDescending expressions."));
    }

    [Test]
    public void Apply_InvalidPreviousClause ()
    {
      QueryModel.BodyClauses.Add (new WhereClause (ExpressionHelper.CreateExpression ()));
      Assert.That (
          () => _node.Apply (QueryModel, ClauseGenerationContext),
          Throws.InstanceOf<NotSupportedException>()
              .With.Message.EqualTo ("ThenByDescending expressions must follow OrderBy, OrderByDescending, ThenBy, or ThenByDescending expressions."));
    }
  }
}

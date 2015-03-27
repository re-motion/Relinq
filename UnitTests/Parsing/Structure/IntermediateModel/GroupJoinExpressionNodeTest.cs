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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Linq.Clauses;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.Parsing.Structure.IntermediateModel;
using Remotion.Linq.UnitTests.TestDomain;

namespace Remotion.Linq.UnitTests.Parsing.Structure.IntermediateModel
{
  [TestFixture]
  public class GroupJoinExpressionNodeTest : ExpressionNodeTestBase
  {
    private Expression _innerSequence;
    private Expression<Func<string, string>> _outerKeySelector;
    private Expression<Func<string, string>> _innerKeySelector;
    private Expression<Func<string,IEnumerable<string>,string>>  _resultSelector;
    private GroupJoinExpressionNode _node;
    private GroupJoinClause _groupJoinClause;

    public override void SetUp ()
    {
      base.SetUp ();
      _innerSequence = ExpressionHelper.CreateExpression ();
      _outerKeySelector = ExpressionHelper.CreateLambdaExpression<string, string> (o => o.ToString ());
      _innerKeySelector = ExpressionHelper.CreateLambdaExpression<string, string> (i => i.ToString ());
      _resultSelector = ExpressionHelper.CreateLambdaExpression<string, IEnumerable<string>, string> ((o, into) => o.ToString() + into.ToString());

      _node = new GroupJoinExpressionNode (
          CreateParseInfo (SourceNode, "groupJoin"), _innerSequence, _outerKeySelector, _innerKeySelector, _resultSelector);
      _groupJoinClause = ExpressionHelper.CreateGroupJoinClause<Cook>();
    }

    [Test]
    public void GetSupportedMethods ()
    {
      Assert.That (
          GroupJoinExpressionNode.GetSupportedMethods(),
          Is.EquivalentTo (
              new[]
              {
                  GetGenericMethodDefinition (
                      () => Queryable.GroupJoin<object, object, object, object> (null, null, o => null, o => null, (o, i) => null)),
                  GetGenericMethodDefinition (
                      () => Enumerable.GroupJoin<object, object, object, object> (null, null, o => null, o => null, (o, i) => null))
              }));
    }

    [Test]
    public void GetResolvedResultSelector ()
    {
      ClauseGenerationContext.AddContextInfo (_node, _groupJoinClause);

      var resolvedExpression = _node.GetResolvedResultSelector (ClauseGenerationContext);

      var expectedExpression = ExpressionHelper.Resolve<string, IEnumerable<string>, string> (
          SourceClause, 
          _groupJoinClause, 
          (o, into) => o.ToString () + into.ToString());
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, resolvedExpression);
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "Cannot retrieve an IQuerySource for the given GroupJoinExpressionNode. "
        + "Be sure to call Apply before calling methods that require IQuerySources, and pass in the same QuerySourceClauseMapping to both.")]
    public void GetResolvedResultSelector_WithoutClause ()
    {
      _node.GetResolvedResultSelector (ClauseGenerationContext);
    }

    [Test]
    public void Resolve ()
    {
      ClauseGenerationContext.AddContextInfo (_node, _groupJoinClause);

      var parameter = Expression.Parameter (typeof (string), "s");
      var result = _node.Resolve (parameter, parameter, ClauseGenerationContext);

      var expectedResult = ExpressionHelper.Resolve<string, IEnumerable<string>, string> (
          SourceClause, 
          _groupJoinClause, 
          (o, into) => o.ToString () + into.ToString());
      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void Apply ()
    {
      var result = _node.Apply (QueryModel, ClauseGenerationContext);
      Assert.That (result, Is.SameAs (QueryModel));

      var clause = (GroupJoinClause) QueryModel.BodyClauses[0];

      Assert.That (clause.ItemName, Is.EqualTo ("into"));
      Assert.That (clause.ItemType, Is.SameAs (typeof (IEnumerable<string>)));
      Assert.That (clause.JoinClause.ItemName, Is.EqualTo ("i"));
      Assert.That (clause.JoinClause.ItemType, Is.SameAs (typeof (string)));
      Assert.That (clause.JoinClause.InnerSequence, Is.SameAs (_innerSequence));
      Assert.That (clause.JoinClause.OuterKeySelector, Is.SameAs (_node.JoinExpressionNode.GetResolvedOuterKeySelector (ClauseGenerationContext)));
      Assert.That (clause.JoinClause.InnerKeySelector, Is.SameAs (_node.JoinExpressionNode.GetResolvedInnerKeySelector (ClauseGenerationContext)));
    }

    [Test]
    public void Apply_AddsMapping ()
    {
      _node.Apply (QueryModel, ClauseGenerationContext);
      var clause = (GroupJoinClause) QueryModel.BodyClauses[0];

      Assert.That (ClauseGenerationContext.GetContextInfo (_node), Is.SameAs (clause));
    }

    [Test]
    public void Apply_AdaptsSelector ()
    {
      _node.Apply (QueryModel, ClauseGenerationContext);
      var clause = QueryModel.SelectClause;

      Assert.That (clause.Selector, Is.SameAs (_node.GetResolvedResultSelector (ClauseGenerationContext)));
    }
  }
}

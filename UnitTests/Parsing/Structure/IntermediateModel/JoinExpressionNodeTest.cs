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
using Remotion.Linq.UnitTests.TestDomain;

namespace Remotion.Linq.UnitTests.Parsing.Structure.IntermediateModel
{
  [TestFixture]
  public class JoinExpressionNodeTest : ExpressionNodeTestBase
  {
    private JoinExpressionNode _node;
    private Expression _innerSequence;
    private Expression<Func<string, string>> _outerKeySelector;
    private Expression<Func<string, string>> _innerKeySelector;
    private Expression<Func<string, string, string>> _resultSelector;
    private JoinClause _joinClause;

    [SetUp]
    public override void SetUp ()
    {
      base.SetUp ();

      _innerSequence = ExpressionHelper.CreateExpression();
      _outerKeySelector = ExpressionHelper.CreateLambdaExpression<string, string> (o => o.ToString ());
      _innerKeySelector = ExpressionHelper.CreateLambdaExpression<string, string> (i => i.ToString ());
      _resultSelector = ExpressionHelper.CreateLambdaExpression<string, string, string> ((o, i) => o.ToString () + i.ToString ());

      _node = new JoinExpressionNode (CreateParseInfo (SourceNode, "join"), _innerSequence, _outerKeySelector, _innerKeySelector, _resultSelector);
      _joinClause = ExpressionHelper.CreateJoinClause<Cook> ();
    }

    [Test]
    public void GetSupportedMethods ()
    {
      Assert.That (
          JoinExpressionNode.GetSupportedMethods(),
          Is.EquivalentTo (
              new[]
              {
                  GetGenericMethodDefinition (
                      () => Queryable.Join<object, object, object, object> (null, null, o => null, o => null, (o, i) => null)),
                  GetGenericMethodDefinition (
                      () => Enumerable.Join<object, object, object, object> (null, null, o => null, o => null, (o, i) => null)),
              }));
    }

    [Test]
    public void GetResolvedOuterKeySelector ()
    {
      var resolvedExpression = _node.GetResolvedOuterKeySelector (ClauseGenerationContext);
      var expectedExpression = ExpressionHelper.Resolve<string, string> (SourceClause, o => o.ToString());

      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, resolvedExpression);
    }

    [Test]
    public void GetResolvedInnerKeySelector ()
    {
      ClauseGenerationContext.AddContextInfo (_node, _joinClause);

      var resolvedExpression = _node.GetResolvedInnerKeySelector (ClauseGenerationContext);
      
      var expectedExpression = ExpressionHelper.Resolve<string, string> (_joinClause, i => i.ToString ());
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, resolvedExpression);
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "Cannot retrieve an IQuerySource for the given JoinExpressionNode. "
        + "Be sure to call Apply before calling methods that require IQuerySources, and pass in the same QuerySourceClauseMapping to both.")]
    public void GetResolvedInnerKeySelector_WithoutClause ()
    {
      _node.GetResolvedInnerKeySelector (ClauseGenerationContext);
    }

    [Test]
    public void GetResolvedResultSelector ()
    {
      ClauseGenerationContext.AddContextInfo (_node, _joinClause);

      var resolvedExpression = _node.GetResolvedResultSelector (ClauseGenerationContext);
      
      var expectedExpression = ExpressionHelper.Resolve<string, string, string> (SourceClause, _joinClause, (o, i) => o.ToString () + i.ToString ());
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, resolvedExpression);
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "Cannot retrieve an IQuerySource for the given JoinExpressionNode. "
        + "Be sure to call Apply before calling methods that require IQuerySources, and pass in the same QuerySourceClauseMapping to both.")]
    public void GetResolvedResultSelector_WithoutClause ()
    {
      _node.GetResolvedResultSelector (ClauseGenerationContext);
    }

    [Test]
    public void Resolve ()
    {
      ClauseGenerationContext.AddContextInfo (_node, _joinClause);

      var parameter = Expression.Parameter (typeof (string), "s");
      var result = _node.Resolve (parameter, parameter, ClauseGenerationContext);

      var expectedResult = ExpressionHelper.Resolve<string, string, string> (SourceClause, _joinClause, (o, i) => o.ToString() + i.ToString());
      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void Apply ()
    {
      var result = _node.Apply (QueryModel, ClauseGenerationContext);
      Assert.That (result, Is.SameAs (QueryModel));

      var clause = (JoinClause) QueryModel.BodyClauses[0];

      Assert.That (clause.ItemName, Is.EqualTo ("i"));
      Assert.That (clause.ItemType, Is.SameAs (typeof (string)));
      Assert.That (clause.InnerSequence, Is.SameAs (_innerSequence));
      Assert.That (clause.OuterKeySelector, Is.SameAs (_node.GetResolvedOuterKeySelector (ClauseGenerationContext)));
      Assert.That (clause.InnerKeySelector, Is.SameAs (_node.GetResolvedInnerKeySelector (ClauseGenerationContext)));     
    }

    [Test]
    public void Apply_AddsMapping ()
    {
      _node.Apply (QueryModel, ClauseGenerationContext);
      var clause = (JoinClause) QueryModel.BodyClauses[0];

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

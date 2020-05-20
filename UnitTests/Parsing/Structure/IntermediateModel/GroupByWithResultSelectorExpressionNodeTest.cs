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
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.Parsing.Structure.IntermediateModel;

namespace Remotion.Linq.UnitTests.Parsing.Structure.IntermediateModel
{
  [TestFixture]
  public class GroupByWithResultSelectorExpressionNodeTest : ExpressionNodeTestBase
  {
    private LambdaExpression _keySelector;
    private LambdaExpression _elementSelector;
    private LambdaExpression _resultSelectorWithElementSelector;
    private LambdaExpression _resultSelectorWithoutElementSelector;

    private IEnumerable<int> _sourceEnumerable;

    private MethodCallExpressionParseInfo _parseInfoWithElementSelector;
    private GroupByWithResultSelectorExpressionNode _nodeWithElementSelector;
    private GroupByWithResultSelectorExpressionNode _nodeWithoutElementSelector;

    public override void SetUp ()
    {
      base.SetUp();

      _keySelector = ExpressionHelper.CreateLambdaExpression<int, short> (i => (short) i);
      _elementSelector = ExpressionHelper.CreateLambdaExpression<int, string> (i => i.ToString());
      _resultSelectorWithElementSelector = 
          ExpressionHelper.CreateLambdaExpression<short, IEnumerable<string>, Tuple<short, int>> ((key, group) => Tuple.Create (key, group.Count()));

      _sourceEnumerable = ExpressionHelper.CreateIntQueryable();

      var methodCallExpressionWithElementSelector = (MethodCallExpression) ExpressionHelper.MakeExpression (
          () => _sourceEnumerable.GroupBy (
              i => (short) i,
              i => i.ToString(),
              (key, group) => Tuple.Create (key, group.Count())));
      _parseInfoWithElementSelector = new MethodCallExpressionParseInfo ("g", SourceNode, methodCallExpressionWithElementSelector);
      _nodeWithElementSelector = new GroupByWithResultSelectorExpressionNode (
          _parseInfoWithElementSelector,
          _keySelector,
          _elementSelector,
          _resultSelectorWithElementSelector);

      var methodCallExpressionWithoutElementSelector = (MethodCallExpression) ExpressionHelper.MakeExpression (
          () => _sourceEnumerable.GroupBy (
              i => (short) i,
              (key, group) => Tuple.Create (key, group.Count())));
      _resultSelectorWithoutElementSelector = 
          ExpressionHelper.CreateLambdaExpression<short, IEnumerable<int>, Tuple<short, int>> ((key, group) => Tuple.Create (key, group.Count()));
      _nodeWithoutElementSelector = new GroupByWithResultSelectorExpressionNode (
          new MethodCallExpressionParseInfo ("g", SourceNode, methodCallExpressionWithoutElementSelector),
          _keySelector,
          _resultSelectorWithoutElementSelector,
          null);
    }

    [Test]
    public void GetSupportedMethods ()
    {
      Assert.That (
          GroupByWithResultSelectorExpressionNode.GetSupportedMethods(),
          Is.EquivalentTo (
              new[]
              {
                  //Key- and result-selector
                  GetGenericMethodDefinition (() => Enumerable.GroupBy<object, object, object> (null, o => null, (k, g) => null)),
                  GetGenericMethodDefinition (() => Queryable.GroupBy<object, object, object> (null, o => null, (k, g) => null)),
                  //Key-, element- and result-selector
                  GetGenericMethodDefinition (() => Queryable.GroupBy<object, object, object, object> (null, o => null, o => null, (k, g) => null)),
                  GetGenericMethodDefinition (() => Enumerable.GroupBy<object, object, object, object> (null, o => null, o => null, (k, g) => null)),
              }));
    }

    [Test]
    public void Initialization_WithElementSelector ()
    {
      var expectedSelectorWithElementSelector = 
          ExpressionHelper.CreateLambdaExpression<IGrouping<short, string>, Tuple<short, int>> (group => Tuple.Create (group.Key, group.Count()));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedSelectorWithElementSelector, _nodeWithElementSelector.Selector);

      Assert.That (((GroupByExpressionNode) _nodeWithElementSelector.Source).KeySelector, Is.SameAs (_keySelector));
      Assert.That (((GroupByExpressionNode) _nodeWithElementSelector.Source).OptionalElementSelector, Is.SameAs (_elementSelector));

      var expectedSimulatedGroupByCallWithElementSelector =
          ExpressionHelper.MakeExpression (() => (_sourceEnumerable.GroupBy (i => (short) i, i => i.ToString())));
      ExpressionTreeComparer.CheckAreEqualTrees (
          expectedSimulatedGroupByCallWithElementSelector,
          ((GroupByExpressionNode) _nodeWithElementSelector.Source).ParsedExpression);

      Assert.That (_nodeWithElementSelector.AssociatedIdentifier, Is.EqualTo ("g"));
    }

    [Test]
    public void Initialization_WithoutElementSelector ()
    {
      var expectedSelectorWithoutElementSelector = 
          ExpressionHelper.CreateLambdaExpression<IGrouping<short, int>, Tuple<short, int>> (group => Tuple.Create (group.Key, group.Count()));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedSelectorWithoutElementSelector, _nodeWithoutElementSelector.Selector);

      Assert.That (((GroupByExpressionNode) _nodeWithoutElementSelector.Source).KeySelector, Is.SameAs (_keySelector));
      Assert.That (((GroupByExpressionNode) _nodeWithoutElementSelector.Source).OptionalElementSelector, Is.Null);

      var expectedSimulatedGroupByCallWithoutElementSelector = ExpressionHelper.MakeExpression (() => (_sourceEnumerable.GroupBy (i => (short) i)));
      ExpressionTreeComparer.CheckAreEqualTrees (
          expectedSimulatedGroupByCallWithoutElementSelector,
          ((GroupByExpressionNode) _nodeWithoutElementSelector.Source).ParsedExpression);

      Assert.That (_nodeWithElementSelector.AssociatedIdentifier, Is.EqualTo ("g"));
    }

    [Test]
    public void Initialization_InvalidResultSelector_WrongNumberOfParameters ()
    {
      var resultSelector = ExpressionHelper.CreateLambdaExpression<int, string, double, bool> ((i, s, d) => true);
      Assert.That (
          () => new GroupByWithResultSelectorExpressionNode (_parseInfoWithElementSelector, _keySelector, _elementSelector, resultSelector),
          Throws.ArgumentException
              .With.Message.Contains ("ResultSelector must have exactly two parameters."));
    }

    [Test]
    public void Resolve_ReplacesParameter_WithProjection ()
    {
      var expression = ExpressionHelper.CreateLambdaExpression<int, Tuple<short, int>> (i => Tuple.Create ((short) 1, 2));

      var querySource = ExpressionHelper.CreateGroupResultOperator ();
      ClauseGenerationContext.AddContextInfo (_nodeWithElementSelector.Source, querySource);

      var result = _nodeWithElementSelector.Resolve (expression.Parameters[0], expression.Body, ClauseGenerationContext);

      var expectedResult = Expression.Call (
          typeof (Tuple),
          "Create",
          new[] { typeof (short), typeof (int) },
          Expression.Constant ((short) 1),
          Expression.Constant (2));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void Apply ()
    {
      var sourceQueryModel = _nodeWithElementSelector.Source.Apply (QueryModel, ClauseGenerationContext);
      Assert.That (sourceQueryModel, Is.SameAs (QueryModel));

      var result = _nodeWithElementSelector.Apply (sourceQueryModel, ClauseGenerationContext);
      Assert.That (result, Is.Not.SameAs (QueryModel));

      var expectedFromExpression = new SubQueryExpression (sourceQueryModel);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedFromExpression, result.MainFromClause.FromExpression);

      var expectedSelectClause = 
          ExpressionHelper.Resolve<IGrouping<short, string>, Tuple<short, int>> (result.MainFromClause, g => Tuple.Create (g.Key, g.Count()));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedSelectClause, result.SelectClause.Selector);

      Assert.That (result.ResultTypeOverride, Is.EqualTo (typeof (IEnumerable<Tuple<short, int>>)));
    }
  }
}
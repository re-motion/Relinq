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

      _keySelector = ExpressionHelper.CreateLambdaExpression<int, bool> (i => i > 5);
      _elementSelector = ExpressionHelper.CreateLambdaExpression<int, string> (i => i.ToString());
      _resultSelectorWithElementSelector = ExpressionHelper.CreateLambdaExpression<bool, IEnumerable<string>, KeyValuePair<bool, int>> (
          (key, group) => new KeyValuePair<bool, int> (key, group.Count()));

      _sourceEnumerable = ExpressionHelper.CreateIntQueryable();

      var methodCallExpressionWithElementSelector = (MethodCallExpression) ExpressionHelper.MakeExpression (
          () => _sourceEnumerable.GroupBy (
              i => i > 5,
              i => i.ToString(),
              (key, group) => new KeyValuePair<bool, int> (key, group.Count())));
      _parseInfoWithElementSelector = new MethodCallExpressionParseInfo ("g", SourceNode, methodCallExpressionWithElementSelector);
      _nodeWithElementSelector = new GroupByWithResultSelectorExpressionNode (
          _parseInfoWithElementSelector,
          _keySelector,
          _elementSelector,
          _resultSelectorWithElementSelector);

      var methodCallExpressionWithoutElementSelector = (MethodCallExpression) ExpressionHelper.MakeExpression (
          () => _sourceEnumerable.GroupBy (
              i => i > 5,
              (key, group) => new KeyValuePair<bool, int> (key, group.Count())));
      _resultSelectorWithoutElementSelector = ExpressionHelper.CreateLambdaExpression<bool, IEnumerable<int>, KeyValuePair<bool, int>> (
          (key, group) => new KeyValuePair<bool, int> (key, group.Count()));
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
      var expectedSelectorWithElementSelector = ExpressionHelper.CreateLambdaExpression<IGrouping<bool, string>, KeyValuePair<bool, int>> (
          group => new KeyValuePair<bool, int> (group.Key, group.Count()));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedSelectorWithElementSelector, _nodeWithElementSelector.Selector);

      Assert.That (((GroupByExpressionNode) _nodeWithElementSelector.Source).KeySelector, Is.SameAs (_keySelector));
      Assert.That (((GroupByExpressionNode) _nodeWithElementSelector.Source).OptionalElementSelector, Is.SameAs (_elementSelector));

      var expectedSimulatedGroupByCallWithElementSelector =
          ExpressionHelper.MakeExpression (() => (_sourceEnumerable.GroupBy (i => i > 5, i => i.ToString())));
      ExpressionTreeComparer.CheckAreEqualTrees (
          expectedSimulatedGroupByCallWithElementSelector,
          ((GroupByExpressionNode) _nodeWithElementSelector.Source).ParsedExpression);
    }

    [Test]
    public void Initialization_WithoutElementSelector ()
    {
      var expectedSelectorWithoutElementSelector = ExpressionHelper.CreateLambdaExpression<IGrouping<bool, int>, KeyValuePair<bool, int>> (
          group => new KeyValuePair<bool, int> (group.Key, group.Count()));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedSelectorWithoutElementSelector, _nodeWithoutElementSelector.Selector);

      Assert.That (((GroupByExpressionNode) _nodeWithoutElementSelector.Source).KeySelector, Is.SameAs (_keySelector));
      Assert.That (((GroupByExpressionNode) _nodeWithoutElementSelector.Source).OptionalElementSelector, Is.Null);

      var expectedSimulatedGroupByCallWithoutElementSelector = ExpressionHelper.MakeExpression (() => (_sourceEnumerable.GroupBy (i => i > 5)));
      ExpressionTreeComparer.CheckAreEqualTrees (
          expectedSimulatedGroupByCallWithoutElementSelector,
          ((GroupByExpressionNode) _nodeWithoutElementSelector.Source).ParsedExpression);
    }

    [Test]
    [ExpectedException (
        typeof (ArgumentException),
        ExpectedMessage = "ResultSelector must have exactly two parameters.",
        MatchType = MessageMatch.Contains)]
    public void Initialization_InvalidResultSelector_WrongNumberOfParameters ()
    {
      var resultSelector = ExpressionHelper.CreateLambdaExpression<int, string, double, bool> ((i, s, d) => true);
      new GroupByWithResultSelectorExpressionNode (_parseInfoWithElementSelector, _keySelector, _elementSelector, resultSelector);
    }
  }
}
// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (c) rubicon IT GmbH, www.rubicon.eu
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
    public void SupportedMethods ()
    {
      AssertSupportedMethod_Generic (
          GroupByWithResultSelectorExpressionNode.SupportedMethods,
          q => q.GroupBy (o => o.GetType(), o => o, (key, g) => 12),
          e => e.GroupBy (o => o.GetType(), o => o, (key, g) => 12));

      AssertSupportedMethod_Generic (
          GroupByWithResultSelectorExpressionNode.SupportedMethods,
          q => q.GroupBy (o => o.GetType(), (key, g) => 12),
          e => e.GroupBy (o => o.GetType(), (key, g) => 12));
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
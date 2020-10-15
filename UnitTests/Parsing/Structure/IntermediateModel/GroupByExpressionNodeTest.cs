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
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.Parsing.Structure.IntermediateModel;

namespace Remotion.Linq.UnitTests.Parsing.Structure.IntermediateModel
{
  [TestFixture]
  public class GroupByExpressionNodeTest : ExpressionNodeTestBase
  {
    private LambdaExpression _keySelector;
    private LambdaExpression _elementSelector;
    private GroupByExpressionNode _nodeWithElementSelector;
    private GroupByExpressionNode _nodeWithoutElementSelector;

    public override void SetUp ()
    {
      base.SetUp ();

      _keySelector = ExpressionHelper.CreateLambdaExpression<int, bool> (i => i > 5);
      _elementSelector = ExpressionHelper.CreateLambdaExpression<int, string> (i => i.ToString());
      _nodeWithElementSelector = new GroupByExpressionNode (CreateParseInfo (SourceNode, "g"), _keySelector, _elementSelector);
      _nodeWithoutElementSelector = new GroupByExpressionNode (CreateParseInfo (SourceNode, "g"), _keySelector, null);
    }

    [Test]
    public void GetSupportedMethods ()
    {
      Assert.That (
          GroupByExpressionNode.GetSupportedMethods(),
          Is.EquivalentTo (
              new[]
              {
                  //Key-selector
                  GetGenericMethodDefinition (() => Queryable.GroupBy<object, object> (null, o => null)),
                  GetGenericMethodDefinition (() => Enumerable.GroupBy<object, object> (null, o => null)),
                  //Key- and element-selector
                  GetGenericMethodDefinition (() => Queryable.GroupBy<object, object, object> (null, o => null, o => null)),
                  GetGenericMethodDefinition (() => Enumerable.GroupBy<object, object, object> (null, o => null, o => null)),
              }));
    }

    [Test]
    public void Initialization_InvalidKeySelector ()
    {
      var keySelector = ExpressionHelper.CreateLambdaExpression<int, string, bool> ((i, s) => true);
      Assert.That (
          () => new GroupByExpressionNode (CreateParseInfo (), keySelector, _elementSelector),
          Throws.ArgumentException);
    }

    [Test]
    public void Initialization_InvalidElementSelector ()
    {
      var elementSelector = ExpressionHelper.CreateLambdaExpression<int, string, bool> ((i, s) => true);
      Assert.That (
          () => new GroupByExpressionNode (CreateParseInfo (), _keySelector, elementSelector),
          Throws.ArgumentException);
    }

    [Test]
    public void Resolve ()
    {
      var querySource = ExpressionHelper.CreateGroupResultOperator ();
      ClauseGenerationContext.AddContextInfo (_nodeWithoutElementSelector, querySource);

      var lambdaExpression =
          ExpressionHelper.CreateLambdaExpression<IGrouping<short, string>, Tuple<short, int>> (g => Tuple.Create (g.Key, g.Count ()));
      
      var result = _nodeWithoutElementSelector.Resolve (lambdaExpression.Parameters[0], lambdaExpression.Body, ClauseGenerationContext);

      var expectedResult = ExpressionHelper.Resolve<IGrouping<short, string>, Tuple<short, int>> (querySource, g => Tuple.Create (g.Key, g.Count ()));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void GetResolvedKeySelector ()
    {
      var resolvedKeySelector = _nodeWithElementSelector.GetResolvedKeySelector (ClauseGenerationContext);

      var expectedExpression = ExpressionHelper.Resolve<int, bool> (SourceClause, i => i > 5);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, resolvedKeySelector);
    }

    [Test]
    public void GetResolvedOptionalElementSelector ()
    {
      var resolvedElementSelector = _nodeWithElementSelector.GetResolvedOptionalElementSelector (ClauseGenerationContext);

      var expectedExpression = ExpressionHelper.Resolve<int, string> (SourceClause, i => i.ToString());
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, resolvedElementSelector);
    }

    [Test]
    public void GetResolvedOptionalElementSelector_Null ()
    {
      var resolvedElementSelector = _nodeWithoutElementSelector.GetResolvedOptionalElementSelector (ClauseGenerationContext);
      Assert.That (resolvedElementSelector, Is.Null);
    }

    [Test]
    public void Apply ()
    {
      var newQueryModel = _nodeWithElementSelector.Apply (QueryModel, ClauseGenerationContext);

      Assert.That (newQueryModel, Is.SameAs (QueryModel));

      Assert.That (QueryModel.ResultOperators[0], Is.InstanceOf (typeof (GroupResultOperator)));
      var resultOperator = (GroupResultOperator) QueryModel.ResultOperators[0];

      Assert.That (resultOperator.ItemName, Is.EqualTo (_nodeWithElementSelector.AssociatedIdentifier));
      Assert.That (resultOperator.KeySelector, Is.SameAs (_nodeWithElementSelector.GetResolvedKeySelector (ClauseGenerationContext)));
      Assert.That (resultOperator.ElementSelector, Is.SameAs (_nodeWithElementSelector.GetResolvedOptionalElementSelector (ClauseGenerationContext)));
    }

    [Test]
    public void Apply_AddsMapping ()
    {
      _nodeWithElementSelector.Apply (QueryModel, ClauseGenerationContext);

      var resultOperator = (GroupResultOperator) QueryModel.ResultOperators[0];
      Assert.That (ClauseGenerationContext.GetContextInfo (_nodeWithElementSelector), Is.SameAs (resultOperator));
    }

    [Test]
    public void Apply_WithoutElementSelector_SuppliesStandardSelector ()
    {
      _nodeWithoutElementSelector.Apply (QueryModel, ClauseGenerationContext);
      var resultOperator = (GroupResultOperator) QueryModel.ResultOperators[0];

      var expectedElementSelector = ExpressionHelper.Resolve<int, int> (QueryModel.MainFromClause, i => i);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedElementSelector, resultOperator.ElementSelector);
    }
  }
}

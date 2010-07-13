// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// as published by the Free Software Foundation; either version 2.1 of the 
// License, or (at your option) any later version.
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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Parsing.Structure.IntermediateModel;

namespace Remotion.Data.Linq.UnitTests.Linq.Core.Parsing.Structure.IntermediateModel
{
  [TestFixture]
  public class GroupByWithResultSelectorExpressionNodeTest : ExpressionNodeTestBase
  {
    private LambdaExpression _keySelector;
    private LambdaExpression _elementSelector;
    private LambdaExpression _resultSelector;

    private GroupByWithResultSelectorExpressionNode _nodeWithElementSelector;

    public override void SetUp ()
    {
      base.SetUp ();

      _keySelector = ExpressionHelper.CreateLambdaExpression<int, bool> (i => i > 5);
      _elementSelector = ExpressionHelper.CreateLambdaExpression<int, string> (i => i.ToString ());
      _resultSelector = ExpressionHelper.CreateLambdaExpression<bool, IGrouping<bool, int>, KeyValuePair<bool, int>> (
          (key, group) => new KeyValuePair<bool, int> (key, group.Count ()));

      _nodeWithElementSelector = new GroupByWithResultSelectorExpressionNode (CreateParseInfo (SourceNode, "g"), _keySelector, _elementSelector, _resultSelector);
    }

    [Test]
    public void SupportedMethods ()
    {
      AssertSupportedMethod_Generic (
          GroupByWithResultSelectorExpressionNode.SupportedMethods,
          q => q.GroupBy (o => o.GetType (), o => o, (key, g) => 12),
          e => e.GroupBy (o => o.GetType (), o => o, (key, g) => 12));

      //AssertSupportedMethod_Generic (
      //    GroupByWithResultSelectorExpressionNode.SupportedMethods,
      //    q => q.GroupBy (o => o.GetType (), (key, g) => 12),
      //    e => e.GroupBy (o => o.GetType (), (key, g) => 12));
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_nodeWithElementSelector.KeySelector, Is.SameAs (_keySelector));
      Assert.That (_nodeWithElementSelector.OptionalElementSelector, Is.SameAs (_elementSelector));
      Assert.That (_nodeWithElementSelector.ResultSelector, Is.SameAs (_resultSelector));
    }

    [Test]
    [ExpectedException (typeof (ArgumentException))]
    public void Initialization_InvalidFirstArgument ()
    {
      var keySelector = ExpressionHelper.CreateLambdaExpression<int, string, bool> ((i, s) => true);
      new GroupByWithResultSelectorExpressionNode (CreateParseInfo (), keySelector, _elementSelector, _resultSelector);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException))]
    public void Initialization_InvalidSecondArgument ()
    {
      var elementSelector = ExpressionHelper.CreateLambdaExpression<int, string, double, bool> ((i, s, d) => true);
      new GroupByWithResultSelectorExpressionNode (CreateParseInfo (), _keySelector, elementSelector, _resultSelector);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException))]
    public void Initialization_InvalidThirdArgument ()
    {
      var resultSelector = ExpressionHelper.CreateLambdaExpression<int, bool> (i => true);
      new GroupByWithResultSelectorExpressionNode (CreateParseInfo (), _keySelector, _elementSelector, resultSelector);
    }

    //[Test]
    //public void Resolve ()
    //{
    //  Assert.Fail ("TODO");
    //}

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

      var expectedExpression = ExpressionHelper.Resolve<int, string> (SourceClause, i => i.ToString ());
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, resolvedElementSelector);
    }

    //[Test]
    //public void GetResolvedResultSelector ()
    //{
    //  var resolvedElementSelector = _nodeWithElementSelector.GetResolvedOptionalElementSelector (ClauseGenerationContext);

    //  var expectedExpression = ExpressionHelper.Resolve<int, string> (SourceClause, i => i.ToString ());
    //  ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, resolvedElementSelector);
    //}

    //[Test]
    //public void GetResolvedOptionalElementSelector_Null ()
    //{
    //  var resolvedElementSelector = _nodeWithoutElementSelector.GetResolvedOptionalElementSelector (ClauseGenerationContext);
    //  Assert.That (resolvedElementSelector, Is.Null);
    //}

    //[Test]
    //public void Apply ()
    //{
    //  var newQueryModel = _nodeWithElementSelector.Apply (QueryModel, ClauseGenerationContext);

    //  Assert.That (newQueryModel, Is.SameAs (QueryModel));

    //  Assert.That (QueryModel.ResultOperators[0], Is.InstanceOfType (typeof (GroupResultOperator)));
    //  var resultOperator = (GroupResultOperator) QueryModel.ResultOperators[0];

    //  Assert.That (resultOperator.ItemName, Is.EqualTo (_nodeWithElementSelector.AssociatedIdentifier));
    //  Assert.That (resultOperator.KeySelector, Is.SameAs (_nodeWithElementSelector.GetResolvedKeySelector (ClauseGenerationContext)));
    //  Assert.That (resultOperator.ElementSelector, Is.SameAs (_nodeWithElementSelector.GetResolvedOptionalElementSelector (ClauseGenerationContext)));
    //}

    //[Test]
    //public void Apply_AddsMapping ()
    //{
    //  _nodeWithElementSelector.Apply (QueryModel, ClauseGenerationContext);

    //  var resultOperator = (GroupResultOperator) QueryModel.ResultOperators[0];
    //  Assert.That (ClauseGenerationContext.GetContextInfo (_nodeWithElementSelector), Is.SameAs (resultOperator));
    //}

    //[Test]
    //public void Apply_WithoutElementSelector_SuppliesStandardSelector ()
    //{
    //  _nodeWithoutElementSelector.Apply (QueryModel, ClauseGenerationContext);
    //  var resultOperator = (GroupResultOperator) QueryModel.ResultOperators[0];

    //  var expectedElementSelector = ExpressionHelper.Resolve<int, int> (QueryModel.MainFromClause, i => i);
    //  ExpressionTreeComparer.CheckAreEqualTrees (expectedElementSelector, resultOperator.ElementSelector);
    //}
  }
}
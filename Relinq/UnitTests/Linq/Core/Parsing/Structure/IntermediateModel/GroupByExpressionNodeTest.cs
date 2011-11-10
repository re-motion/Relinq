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
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Linq.UnitTests.Linq.Core.TestUtilities;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Parsing.Structure.IntermediateModel;

namespace Remotion.Linq.UnitTests.Linq.Core.Parsing.Structure.IntermediateModel
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
    public void SupportedMethod_KeySelectorOnly ()
    {
      AssertSupportedMethod_Generic (
          GroupByExpressionNode.SupportedMethods,
          q => q.GroupBy (o => o.GetType ()),
          e => e.GroupBy (o => o.GetType ()));
    }

    [Test]
    public void SupportedMethod_KeyAndElementSelector ()
    {
      AssertSupportedMethod_Generic (
          GroupByExpressionNode.SupportedMethods,
          q => q.GroupBy (o => o.GetType (), o => o),
          e => e.GroupBy (o => o.GetType (), o => o));
    }

    [Test]
    [ExpectedException (typeof (ArgumentException))]
    public void Initialization_InvalidKeySelector ()
    {
      var keySelector = ExpressionHelper.CreateLambdaExpression<int, string, bool> ((i, s) => true);
      new GroupByExpressionNode (CreateParseInfo (), keySelector, _elementSelector);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException))]
    public void Initialization_InvalidElementSelector ()
    {
      var elementSelector = ExpressionHelper.CreateLambdaExpression<int, string, bool> ((i, s) => true);
      new GroupByExpressionNode (CreateParseInfo (), _keySelector, elementSelector);
    }

    [Test]
    public void Resolve ()
    {
      var querySource = ExpressionHelper.CreateGroupResultOperator ();
      ClauseGenerationContext.AddContextInfo (_nodeWithoutElementSelector, querySource);

      var lambdaExpression =
          ExpressionHelper.CreateLambdaExpression<IGrouping<string, string>, Tuple<string, int>> (g => Tuple.Create (g.Key, g.Count ()));
      
      var result = _nodeWithoutElementSelector.Resolve (lambdaExpression.Parameters[0], lambdaExpression.Body, ClauseGenerationContext);

      var expectedResult = ExpressionHelper.Resolve<IGrouping<string, string>, Tuple<string, int>> (querySource, g => Tuple.Create (g.Key, g.Count ()));
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

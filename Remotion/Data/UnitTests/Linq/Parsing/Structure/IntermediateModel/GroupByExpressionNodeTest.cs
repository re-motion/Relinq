// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// version 3.0 as published by the Free Software Foundation.
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
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Data.Linq.Clauses.ResultOperators;
using Remotion.Data.Linq.Parsing.Structure.IntermediateModel;
using System.Linq;
using NUnit.Framework.SyntaxHelpers;

namespace Remotion.Data.UnitTests.Linq.Parsing.Structure.IntermediateModel
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
    [ExpectedException (typeof (InvalidOperationException))]
    public void Resolve_ThrowsInvalidOperationException ()
    {
      _nodeWithoutElementSelector.Resolve (ExpressionHelper.CreateParameterExpression (), ExpressionHelper.CreateExpression (), ClauseGenerationContext);
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

      Assert.That (QueryModel.ResultOperators[0], Is.InstanceOfType (typeof (GroupResultOperator)));
      var resultOperator = (GroupResultOperator) QueryModel.ResultOperators[0];
      Assert.That (resultOperator.KeySelector, Is.SameAs (_nodeWithElementSelector.GetResolvedKeySelector (ClauseGenerationContext)));
      Assert.That (resultOperator.ElementSelector, Is.SameAs (_nodeWithElementSelector.GetResolvedOptionalElementSelector (ClauseGenerationContext)));
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
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
using System.Linq;
using System.Linq.Expressions;
using Microsoft.VisualBasic.CompilerServices;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Parsing.ExpressionTreeVisitors;
using Remotion.Data.Linq.Parsing.Structure;
using Remotion.Data.Linq.Parsing.Structure.IntermediateModel;
using Remotion.Data.Linq.UnitTests.Linq.Core.Parsing.Structure.TestDomain;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestQueryGenerators;

namespace Remotion.Data.Linq.UnitTests.Linq.Core.Parsing.ExpressionTreeVisitors
{
  [TestFixture]
  public class PreprocessingExpressionTreeVisitorTest
  {
    private MethodCallExpressionNodeTypeRegistry _nodeTypeRegistry;

    [SetUp]
    public void SetUp ()
    {
      _nodeTypeRegistry = MethodCallExpressionNodeTypeRegistry.CreateDefault();
    }

    [Test]
    public void TreeWithNoSubquery ()
    {
      Expression expression = Expression.Constant ("test");

      Expression newExpression = PreprocessingExpressionTreeVisitor.Process (expression, _nodeTypeRegistry);
      Assert.That (newExpression, Is.SameAs (expression));
    }

    [Test]
    public void TreeWithSubquery ()
    {
      Expression subQuery = SelectTestQueryGenerator.CreateSimpleQuery (ExpressionHelper.CreateCookQueryable()).Expression;
      Expression surroundingExpression = Expression.Lambda (subQuery);

      Expression newExpression = PreprocessingExpressionTreeVisitor.Process (surroundingExpression, _nodeTypeRegistry);

      Assert.That (newExpression, Is.Not.SameAs (surroundingExpression));
      Assert.That (newExpression, Is.InstanceOfType (typeof (LambdaExpression)));

      var newLambdaExpression = (LambdaExpression) newExpression;
      Assert.That (newLambdaExpression.Body, Is.InstanceOfType (typeof (SubQueryExpression)));

      var newSubQueryExpression = (SubQueryExpression) newLambdaExpression.Body;
      Assert.That (
          ((QuerySourceReferenceExpression) newSubQueryExpression.QueryModel.SelectClause.Selector).ReferencedQuerySource,
          Is.SameAs (newSubQueryExpression.QueryModel.MainFromClause));
    }

    [Test]
    public void VisitorUsesNodeTypeRegistry_ToParseAndAnalyzeSubQueries ()
    {
      Expression subQuery = ExpressionHelper.MakeExpression (() => CustomSelect (ExpressionHelper.CreateCookQueryable(), s => s));
      Expression surroundingExpression = Expression.Lambda (subQuery);

      var emptyNodeTypeRegistry = new MethodCallExpressionNodeTypeRegistry();
      emptyNodeTypeRegistry.Register (new[] { ((MethodCallExpression) subQuery).Method }, typeof (SelectExpressionNode));

      var newLambdaExpression =
          (LambdaExpression) PreprocessingExpressionTreeVisitor.Process (surroundingExpression, emptyNodeTypeRegistry);
      Assert.That (newLambdaExpression.Body, Is.InstanceOfType (typeof (SubQueryExpression)));
    }

    [Test]
    public void VisitorUsesExpressionTreeVisitor_ToGetPotentialQueryOperator ()
    {
      _nodeTypeRegistry.Register (new[] { typeof (QueryableFakeWithCount<>).GetMethod ("get_Count") }, typeof (CountExpressionNode));
      Expression subQuery = ExpressionHelper.MakeExpression (() => new QueryableFakeWithCount<int>().Count);
      Expression surroundingExpression = Expression.Lambda (subQuery);

      var newLambdaExpression =
          (LambdaExpression) PreprocessingExpressionTreeVisitor.Process (surroundingExpression, _nodeTypeRegistry);
      Assert.That (newLambdaExpression.Body, Is.InstanceOfType (typeof (SubQueryExpression)));
    }

    [Test]
    public void VisitBinaryExpression_LeftSideIsNoMethodCallExpression_ReturnsSameExpression ()
    {
      var expression = Expression.Equal (Expression.Constant(5), Expression.Constant (10));

      var result = PreprocessingExpressionTreeVisitor.Process (expression, _nodeTypeRegistry);

      Assert.That (result, Is.SameAs (expression));
    }

    // TODO Review 2942: Add a test for a method with wrong declaring type, but right name (i.e., add a CompareString method on some other type)
    // TODO Review 2942: Add a test for a method with right declaring type, but wrong name

    [Test]
    public void VisitBinaryExpression_Equal_LeftSideIsCompareStringExpression_ReturnsVBStringComparisonExpression ()
    {
      var left = Expression.Constant ("left");
      var right = Expression.Constant ("right");
      var expression = Expression.Equal(
          Expression.Call (typeof (Operators).GetMethod ("CompareString"), left, right, Expression.Constant (true)), Expression.Constant(0));
      
      var result = PreprocessingExpressionTreeVisitor.Process (expression, _nodeTypeRegistry);

      Assert.That (result, Is.TypeOf (typeof (VBStringComparisonExpression)));
      Assert.That (((VBStringComparisonExpression) result).Comparison.NodeType, Is.EqualTo(ExpressionType.Equal));
      Assert.That (((BinaryExpression) ((VBStringComparisonExpression) result).Comparison).Left, Is.SameAs (left));
      Assert.That (((BinaryExpression) ((VBStringComparisonExpression) result).Comparison).Right, Is.SameAs (right));
      Assert.That (((VBStringComparisonExpression) result).TextCompare, Is.True);
    }

    [Test]
    public void VisitBinaryExpression_NotEqual_LeftSideIsCompareStringExpression_ReturnsVBStringComparisonExpression ()
    {
      var left = Expression.Constant ("left");
      var right = Expression.Constant ("right");
      var expression = Expression.NotEqual(
          Expression.Call (typeof (Operators).GetMethod ("CompareString"), left, right, Expression.Constant (true)), Expression.Constant (0));

      var result = PreprocessingExpressionTreeVisitor.Process (expression, _nodeTypeRegistry);

      Assert.That (result, Is.TypeOf (typeof (VBStringComparisonExpression)));
      Assert.That (((VBStringComparisonExpression) result).Comparison.NodeType, Is.EqualTo (ExpressionType.NotEqual));
      Assert.That (((BinaryExpression) ((VBStringComparisonExpression) result).Comparison).Left, Is.SameAs (left));
      Assert.That (((BinaryExpression) ((VBStringComparisonExpression) result).Comparison).Right, Is.SameAs (right));
      Assert.That (((VBStringComparisonExpression) result).TextCompare, Is.True);
    }

    [Test]
    public void VisitBinaryExpression_GreaterThan_LeftSideIsCompareStringExpression_ReturnsVBStringComparisonExpression ()
    {
      var left = Expression.Constant ("left");
      var right = Expression.Constant ("right");
      var expression = Expression.GreaterThan(
          Expression.Call (typeof (Operators).GetMethod ("CompareString"), left, right, Expression.Constant (true)), Expression.Constant (0));

      var result = PreprocessingExpressionTreeVisitor.Process (expression, _nodeTypeRegistry);

      Assert.That (result, Is.TypeOf (typeof (BinaryExpression)));
      Assert.That (result.NodeType, Is.EqualTo(ExpressionType.GreaterThan));
      Assert.That (((VBStringComparisonExpression) ((BinaryExpression) result).Left).Comparison, Is.TypeOf(typeof(MethodCallExpression)));
    }

    [Test]
    public void VisitBinaryExpression_GreaterThanOrEqual_LeftSideIsCompareStringExpression_ReturnsVBStringComparisonExpression ()
    {
      var left = Expression.Constant ("left");
      var right = Expression.Constant ("right");
      var expression = Expression.GreaterThanOrEqual (
          Expression.Call (typeof (Operators).GetMethod ("CompareString"), left, right, Expression.Constant (true)), Expression.Constant (0));

      var result = PreprocessingExpressionTreeVisitor.Process (expression, _nodeTypeRegistry);

      Assert.That (result, Is.TypeOf (typeof (BinaryExpression)));
      Assert.That (result.NodeType, Is.EqualTo (ExpressionType.GreaterThanOrEqual));
      Assert.That (((VBStringComparisonExpression) ((BinaryExpression) result).Left).Comparison, Is.TypeOf (typeof (MethodCallExpression)));
    }

    [Test]
    public void VisitBinaryExpression_LessThan_LeftSideIsCompareStringExpression_ReturnsVBStringComparisonExpression ()
    {
      var left = Expression.Constant ("left");
      var right = Expression.Constant ("right");
      var expression = Expression.LessThan (
          Expression.Call (typeof (Operators).GetMethod ("CompareString"), left, right, Expression.Constant (true)), Expression.Constant (0));

      var result = PreprocessingExpressionTreeVisitor.Process (expression, _nodeTypeRegistry);

      Assert.That (result, Is.TypeOf (typeof (BinaryExpression)));
      Assert.That (result.NodeType, Is.EqualTo (ExpressionType.LessThan));
      Assert.That (((VBStringComparisonExpression) ((BinaryExpression) result).Left).Comparison, Is.TypeOf (typeof (MethodCallExpression)));
    }

    [Test]
    public void VisitBinaryExpression_LessThanOrEqual_LeftSideIsCompareStringExpression_ReturnsVBStringComparisonExpression ()
    {
      var left = Expression.Constant ("left");
      var right = Expression.Constant ("right");
      var expression = Expression.LessThanOrEqual (
          Expression.Call (typeof (Operators).GetMethod ("CompareString"), left, right, Expression.Constant (true)), Expression.Constant (0));

      var result = PreprocessingExpressionTreeVisitor.Process (expression, _nodeTypeRegistry);

      Assert.That (result, Is.TypeOf (typeof (BinaryExpression)));
      Assert.That (result.NodeType, Is.EqualTo (ExpressionType.LessThanOrEqual));
      Assert.That (((VBStringComparisonExpression) ((BinaryExpression) result).Left).Comparison, Is.TypeOf (typeof (MethodCallExpression)));
    }

    [Test]
    public void VisitUnknownExpression_Ignored ()
    {
      var expression = new UnknownExpression (typeof (object));
      var result = PreprocessingExpressionTreeVisitor.Process (expression, _nodeTypeRegistry);

      Assert.That (result, Is.SameAs (expression));
    }

    public static IQueryable<Cook> CustomSelect (IQueryable<Cook> source, Expression<Func<Cook, Cook>> selector)
    {
      throw new NotImplementedException();
    }
  }
}
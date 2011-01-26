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
using System.Linq.Expressions;
using Microsoft.VisualBasic.CompilerServices;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Parsing.ExpressionTreeVisitors;
using Remotion.Data.Linq.Parsing.ExpressionTreeVisitors.Transformation.PredefinedTransformations;
using Remotion.Data.Linq.UnitTests.Linq.Core.Parsing.ExpressionTreeVisitorTests;

namespace Remotion.Data.Linq.UnitTests.Linq.Core.Parsing.ExpressionTreeVisitors.Transformation.PredefinedTransformations
{
  [TestFixture]
  public class VBCompareStringExpressionTransformerTest
  {
    private VBCompareStringExpressionTransformer _transformer;

    [SetUp]
    public void SetUp ()
    {
      _transformer = new VBCompareStringExpressionTransformer();
    }

    [Test]
    public void Transform_LeftSideIsNoMethodCallExpression_ReturnsSameExpression ()
    {
      var expression = Expression.Equal (Expression.Constant (5), Expression.Constant (10));

      var result = _transformer.Transform (expression);

      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void Transform_Equal_LeftSideIsCompareStringExpression_WrongDeclaringType_ReturnsSameExpression ()
    {
      var left = Expression.Constant ("left");
      var right = Expression.Constant ("right");
      var expression = Expression.Equal (
          Expression.Call (typeof (TypeForNewExpression).GetMethod ("CompareString"), left, right, Expression.Constant (true)), Expression.Constant (0));

      var result = _transformer.Transform (expression);

      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void Transform_Equal_LeftSideIsCompareStringExpression_WrongMethodName_ReturnsSameExpression ()
    {
      var left = Expression.Constant ("left");
      var right = Expression.Constant ("right");
      var expression = Expression.Equal (
          Expression.Call (typeof (Operators).GetMethod ("CompareObject"), left, right, Expression.Constant (true)), Expression.Constant (0));

      var result = _transformer.Transform (expression);

      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void Transform_Equal_LeftSideIsCompareStringExpression_ReturnsVBStringComparisonExpression ()
    {
      var left = Expression.Constant ("left");
      var right = Expression.Constant ("right");
      var expression = Expression.Equal (
          Expression.Call (typeof (Operators).GetMethod ("CompareString"), left, right, Expression.Constant (true)), Expression.Constant (0));

      var result = _transformer.Transform (expression);

      Assert.That (result, Is.TypeOf (typeof (VBStringComparisonExpression)));
      Assert.That (((VBStringComparisonExpression) result).Comparison.NodeType, Is.EqualTo (ExpressionType.Equal));
      Assert.That (((BinaryExpression) ((VBStringComparisonExpression) result).Comparison).Left, Is.SameAs (left));
      Assert.That (((BinaryExpression) ((VBStringComparisonExpression) result).Comparison).Right, Is.SameAs (right));
      Assert.That (((VBStringComparisonExpression) result).TextCompare, Is.True);
    }

    [Test]
    public void Transform_NotEqual_LeftSideIsCompareStringExpression_ReturnsVBStringComparisonExpression ()
    {
      var left = Expression.Constant ("left");
      var right = Expression.Constant ("right");
      var expression = Expression.NotEqual (
          Expression.Call (typeof (Operators).GetMethod ("CompareString"), left, right, Expression.Constant (true)), Expression.Constant (0));

      var result = _transformer.Transform (expression);

      Assert.That (result, Is.TypeOf (typeof (VBStringComparisonExpression)));
      Assert.That (((VBStringComparisonExpression) result).Comparison.NodeType, Is.EqualTo (ExpressionType.NotEqual));
      Assert.That (((BinaryExpression) ((VBStringComparisonExpression) result).Comparison).Left, Is.SameAs (left));
      Assert.That (((BinaryExpression) ((VBStringComparisonExpression) result).Comparison).Right, Is.SameAs (right));
      Assert.That (((VBStringComparisonExpression) result).TextCompare, Is.True);
    }

    [Test]
    public void Transform_GreaterThan_LeftSideIsCompareStringExpression_ReturnsVBStringComparisonExpression ()
    {
      var left = Expression.Constant ("left");
      var right = Expression.Constant ("right");
      var expression = Expression.GreaterThan (
          Expression.Call (typeof (Operators).GetMethod ("CompareString"), left, right, Expression.Constant (true)), Expression.Constant (0));

      var result = _transformer.Transform (expression);

      Assert.That (result, Is.TypeOf (typeof (BinaryExpression)));
      Assert.That (result.NodeType, Is.EqualTo (ExpressionType.GreaterThan));
      Assert.That (((VBStringComparisonExpression) ((BinaryExpression) result).Left).Comparison, Is.TypeOf (typeof (MethodCallExpression)));
    }

    [Test]
    public void Transform_GreaterThanOrEqual_LeftSideIsCompareStringExpression_ReturnsVBStringComparisonExpression ()
    {
      var left = Expression.Constant ("left");
      var right = Expression.Constant ("right");
      var expression = Expression.GreaterThanOrEqual (
          Expression.Call (typeof (Operators).GetMethod ("CompareString"), left, right, Expression.Constant (true)), Expression.Constant (0));

      var result = _transformer.Transform (expression);

      Assert.That (result, Is.TypeOf (typeof (BinaryExpression)));
      Assert.That (result.NodeType, Is.EqualTo (ExpressionType.GreaterThanOrEqual));
      Assert.That (((VBStringComparisonExpression) ((BinaryExpression) result).Left).Comparison, Is.TypeOf (typeof (MethodCallExpression)));
    }

    [Test]
    public void Transform_LessThan_LeftSideIsCompareStringExpression_ReturnsVBStringComparisonExpression ()
    {
      var left = Expression.Constant ("left");
      var right = Expression.Constant ("right");
      var expression = Expression.LessThan (
          Expression.Call (typeof (Operators).GetMethod ("CompareString"), left, right, Expression.Constant (true)), Expression.Constant (0));

      var result = _transformer.Transform (expression);

      Assert.That (result, Is.TypeOf (typeof (BinaryExpression)));
      Assert.That (result.NodeType, Is.EqualTo (ExpressionType.LessThan));
      Assert.That (((VBStringComparisonExpression) ((BinaryExpression) result).Left).Comparison, Is.TypeOf (typeof (MethodCallExpression)));
    }

    [Test]
    public void Transform_LessThanOrEqual_LeftSideIsCompareStringExpression_ReturnsVBStringComparisonExpression ()
    {
      var left = Expression.Constant ("left");
      var right = Expression.Constant ("right");
      var expression = Expression.LessThanOrEqual (
          Expression.Call (typeof (Operators).GetMethod ("CompareString"), left, right, Expression.Constant (true)), Expression.Constant (0));

      var result = _transformer.Transform (expression);

      Assert.That (result, Is.TypeOf (typeof (BinaryExpression)));
      Assert.That (result.NodeType, Is.EqualTo (ExpressionType.LessThanOrEqual));
      Assert.That (((VBStringComparisonExpression) ((BinaryExpression) result).Left).Comparison, Is.TypeOf (typeof (MethodCallExpression)));
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = 
        "Binary expression with node type 'Add' is not supported in a VB string comparison.")]
    public void Transform_UnsupportedNodeType_LeftSideIsCompareStringExpression_ReturnsVBStringComparisonExpression ()
    {
      var left = Expression.Constant ("left");
      var right = Expression.Constant ("right");
      var expression = Expression.Add (
          Expression.Call (typeof (Operators).GetMethod ("CompareString"), left, right, Expression.Constant (true)), Expression.Constant (0));

      _transformer.Transform (expression);
    }
  }
}
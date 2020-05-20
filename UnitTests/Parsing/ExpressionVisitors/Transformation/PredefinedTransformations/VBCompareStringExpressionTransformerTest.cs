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
using System.Linq.Expressions;
using Microsoft.VisualBasic.CompilerServices;
using NUnit.Framework;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Parsing.ExpressionVisitors.Transformation.PredefinedTransformations;
using Remotion.Linq.UnitTests.Parsing.ExpressionVisitorTests;

namespace Remotion.Linq.UnitTests.Parsing.ExpressionVisitors.Transformation.PredefinedTransformations
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
    public void SupportedExpressionTypes ()
    {
      var result = _transformer.SupportedExpressionTypes;

      Assert.That (result.Length, Is.EqualTo (6));
      Assert.That (
          result,
          Is.EquivalentTo (
              new[]
              {
                  ExpressionType.Equal, ExpressionType.NotEqual, ExpressionType.LessThan, ExpressionType.GreaterThan, ExpressionType.LessThanOrEqual,
                  ExpressionType.GreaterThanOrEqual
              }));
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

      Assert.That (result, Is.AssignableTo (typeof (BinaryExpression)));
      Assert.That (result.NodeType, Is.EqualTo (ExpressionType.GreaterThan));
      Assert.That (((VBStringComparisonExpression) ((BinaryExpression) result).Left).Comparison, Is.AssignableTo (typeof (MethodCallExpression)));
    }

    [Test]
    public void Transform_GreaterThanOrEqual_LeftSideIsCompareStringExpression_ReturnsVBStringComparisonExpression ()
    {
      var left = Expression.Constant ("left");
      var right = Expression.Constant ("right");
      var expression = Expression.GreaterThanOrEqual (
          Expression.Call (typeof (Operators).GetMethod ("CompareString"), left, right, Expression.Constant (true)), Expression.Constant (0));

      var result = _transformer.Transform (expression);

      Assert.That (result, Is.AssignableTo (typeof (BinaryExpression)));
      Assert.That (result.NodeType, Is.EqualTo (ExpressionType.GreaterThanOrEqual));
      Assert.That (((VBStringComparisonExpression) ((BinaryExpression) result).Left).Comparison, Is.AssignableTo (typeof (MethodCallExpression)));
    }

    [Test]
    public void Transform_LessThan_LeftSideIsCompareStringExpression_ReturnsVBStringComparisonExpression ()
    {
      var left = Expression.Constant ("left");
      var right = Expression.Constant ("right");
      var expression = Expression.LessThan (
          Expression.Call (typeof (Operators).GetMethod ("CompareString"), left, right, Expression.Constant (true)), Expression.Constant (0));

      var result = _transformer.Transform (expression);

      Assert.That (result, Is.AssignableTo (typeof (BinaryExpression)));
      Assert.That (result.NodeType, Is.EqualTo (ExpressionType.LessThan));
      Assert.That (((VBStringComparisonExpression) ((BinaryExpression) result).Left).Comparison, Is.AssignableTo (typeof (MethodCallExpression)));
    }

    [Test]
    public void Transform_LessThanOrEqual_LeftSideIsCompareStringExpression_ReturnsVBStringComparisonExpression ()
    {
      var left = Expression.Constant ("left");
      var right = Expression.Constant ("right");
      var expression = Expression.LessThanOrEqual (
          Expression.Call (typeof (Operators).GetMethod ("CompareString"), left, right, Expression.Constant (true)), Expression.Constant (0));

      var result = _transformer.Transform (expression);

      Assert.That (result, Is.AssignableTo (typeof (BinaryExpression)));
      Assert.That (result.NodeType, Is.EqualTo (ExpressionType.LessThanOrEqual));
      Assert.That (((VBStringComparisonExpression) ((BinaryExpression) result).Left).Comparison, Is.AssignableTo (typeof (MethodCallExpression)));
    }

    [Test]
    public void Transform_UnsupportedNodeType_LeftSideIsCompareStringExpression_ReturnsVBStringComparisonExpression ()
    {
      var left = Expression.Constant ("left");
      var right = Expression.Constant ("right");
      var expression = Expression.Add (
          Expression.Call (typeof (Operators).GetMethod ("CompareString"), left, right, Expression.Constant (true)), Expression.Constant (0));
      Assert.That (
          () => _transformer.Transform (expression),
          Throws.InstanceOf<NotSupportedException>()
              .With.Message.EqualTo ("Binary expression with node type 'Add' is not supported in a VB string comparison."));
    }

    
  }
}
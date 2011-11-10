// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (C) rubicon IT GmbH, www.rubicon.eu
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
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Linq.Parsing.ExpressionTreeVisitors.Transformation.PredefinedTransformations;

namespace Remotion.Linq.UnitTests.Linq.Core.Parsing.ExpressionTreeVisitors.Transformation.PredefinedTransformations
{
  [TestFixture]
  public class InvocationOfLambdaExpressionTransformerTest
  {
    private InvocationOfLambdaExpressionTransformer _transformer;

    [SetUp]
    public void SetUp ()
    {
      _transformer = new InvocationOfLambdaExpressionTransformer();
    }

    [Test]
    public void SupportedExpressionTypes ()
    {
      Assert.That (_transformer.SupportedExpressionTypes, Is.EqualTo (new[] { ExpressionType.Invoke }));
    }

    [Test]
    public void Transform_InnerLambdaExpression ()
    {
      // Input: ((p1, p2) => (p1 + p2).ToString()) (1.0, 2.0)
      Expression<Func<double, double, string>> innerExpression = (p1, p2) => (p1 + p2).ToString();
      var invokeExpression = Expression.Invoke (innerExpression, Expression.Constant (1.0), Expression.Constant (2.0));

      var result = _transformer.Transform (invokeExpression);

      // Output: (1.0 + 2.0).ToString()
      var expectedExpression = Expression.Call (
          Expression.Add (
              Expression.Constant (1.0), 
              Expression.Constant (2.0)),
          typeof (double).GetMethod ("ToString", Type.EmptyTypes));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }
    
    [Test]
    public void Transform_InnerLambdaExpression_WrappedInTrivialConvert ()
    {
      // Input: ((Func<double, double, string>) ((p1, p2) => (p1 + p2).ToString())) (1.0, 2.0)
      Expression<Func<double, double, string>> innerExpression = (p1, p2) => (p1 + p2).ToString ();
      var invokeExpression = Expression.Invoke (
          Expression.Convert (innerExpression, innerExpression.Type), 
          Expression.Constant (1.0), 
          Expression.Constant (2.0));

      var result = _transformer.Transform (invokeExpression);

      // Output: (1.0 + 2.0).ToString()
      var expectedExpression = Expression.Call (
          Expression.Add (
              Expression.Constant (1.0),
              Expression.Constant (2.0)),
          typeof (double).GetMethod ("ToString", Type.EmptyTypes));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void Transform_InnerLambdaExpression_WrappedInTrivialConvert_MultipleConverts ()
    {
      // Input: ((Func<double, double, string>) (Func<double, double, string>) ((p1, p2) => (p1 + p2).ToString())) (1.0, 2.0)
      Expression<Func<double, double, string>> innerExpression = (p1, p2) => (p1 + p2).ToString ();
      var invokeExpression = Expression.Invoke (
          Expression.Convert (Expression.Convert (innerExpression, innerExpression.Type), innerExpression.Type),
          Expression.Constant (1.0),
          Expression.Constant (2.0));

      var result = _transformer.Transform (invokeExpression);

      // Output: (1.0 + 2.0).ToString()
      var expectedExpression = Expression.Call (
          Expression.Add (
              Expression.Constant (1.0),
              Expression.Constant (2.0)),
          typeof (double).GetMethod ("ToString", Type.EmptyTypes));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void Transform_InnerLambdaExpression_WrappedInTrivialConvert_WithCastMethod ()
    {
      // Input: ((Func<double, double, string>) ((p1, p2) => (p1 + p2).ToString())) (1.0, 2.0), with custom cast operator
      Expression<Func<double, double, string>> innerExpression = (p1, p2) => (p1 + p2).ToString ();
      var invokeExpression = Expression.Invoke (
          Expression.Convert (innerExpression, innerExpression.Type, GetType().GetMethod ("TrivialCastMethod")),
          Expression.Constant (1.0),
          Expression.Constant (2.0));

      var result = _transformer.Transform (invokeExpression);

      Assert.That (result, Is.SameAs (invokeExpression));
    }

    [Test]
    public void Transform_InnerLambdaExpression_WrappedInNonTrivialConvert ()
    {
      // Input: (((Func<double, double, string>) (object) (Func<double, double, string>) ((p1, p2) => (p1 + p2).ToString())) (1.0, 2.0)
      Expression<Func<double, double, string>> innerExpression = (p1, p2) => (p1 + p2).ToString ();
      var invokeExpression = Expression.Invoke (
          Expression.Convert (Expression.Convert (innerExpression, typeof (object)), innerExpression.Type),
          Expression.Constant (1.0),
          Expression.Constant (2.0));

      var result = _transformer.Transform (invokeExpression);

      Assert.That (result, Is.SameAs (invokeExpression));
    }

    [Test]
    public void Transform_OtherInnerExpression ()
    {
      Func<int, double, string> innerDelegate = (p1, p2) => (p1 + p2).ToString ();
      var invokeExpression = Expression.Invoke (Expression.Constant(innerDelegate), Expression.Constant (1), Expression.Constant (2.0));

      var result = _transformer.Transform (invokeExpression);

      Assert.That (result, Is.SameAs (invokeExpression));
    }

    public static Func<double, double, string> TrivialCastMethod (Func<double, double, string> argument)
    {
      return argument;
    }
  }
}
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
using NUnit.Framework;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.Parsing.ExpressionVisitors.Transformation.PredefinedTransformations;

namespace Remotion.Linq.UnitTests.Parsing.ExpressionVisitors.Transformation.PredefinedTransformations
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
    [Ignore ("TODO 4769")]
    public void Transform_InnerLambdaExpression_WithCompile ()
    {
      Expression<Func<double, double, string>> innerExpression = (p1, p2) => (p1 + p2).ToString ();
      var invokeExpression = (InvocationExpression) ExpressionHelper.MakeExpression (() => innerExpression.Compile() (1.0, 2.0));

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
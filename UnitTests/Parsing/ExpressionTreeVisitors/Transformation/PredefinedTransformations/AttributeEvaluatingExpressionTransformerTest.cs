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
using JetBrains.Annotations;
using NUnit.Framework;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.Parsing.ExpressionTreeVisitors.Transformation;
using Remotion.Linq.Parsing.ExpressionTreeVisitors.Transformation.PredefinedTransformations;
using Remotion.Linq.UnitTests.TestDomain;

namespace Remotion.Linq.UnitTests.Parsing.ExpressionTreeVisitors.Transformation.PredefinedTransformations
{
  [TestFixture]
  public class AttributeEvaluatingExpressionTransformerTest
  {
    private AttributeEvaluatingExpressionTransformer _transformer;

    [SetUp]
    public void SetUp ()
    {
      _transformer = new AttributeEvaluatingExpressionTransformer();
    }

    [Test]
    public void SupportedExpressionTypes ()
    {
      Assert.That (_transformer.SupportedExpressionTypes, Is.EqualTo (new[] { ExpressionType.Call, ExpressionType.MemberAccess }));
    }

    [Test]
    public void Transform_NoAttribute ()
    {
      var expression = ExpressionHelper.CreateMethodCallExpression<Cook>();

      var result = _transformer.Transform (expression);

      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void Transform_WithAttribute ()
    {
      var instance = Expression.Constant (null, typeof (DomainType));
      var expression = Expression.Call (instance, "MethodWithAttribute", Type.EmptyTypes);

      var result = _transformer.Transform (expression);

      Assert.That (result, Is.Not.SameAs (expression));
      Assert.That (result, Is.InstanceOf<ConstantExpression>().With.Property ("Value").EqualTo ("Replaced!"));
    }

    [Test]
    public void Transform_WithTwoAttributes ()
    {
      var instance = Expression.Constant (null, typeof (DomainType));
      var expression = Expression.Call (instance, "MethodWithTwoAttributes", Type.EmptyTypes);

      Assert.That (
          () => _transformer.Transform (expression),
          Throws.InvalidOperationException.With.Message.EqualTo (
              "There is more than one attribute providing transformers declared for method "
              + "'Remotion.Linq.UnitTests.Parsing.ExpressionTreeVisitors.Transformation.PredefinedTransformations.AttributeEvaluatingExpressionTransformerTest+DomainType.MethodWithTwoAttributes'."));
    }

    [Test]
    public void Transform_DerivedMethodWithAttribute ()
    {
      var instance = Expression.Constant (null, typeof (DerivedDomainType));
      var expression = Expression.Call (instance, "MethodWithAttribute", Type.EmptyTypes);

      var result = _transformer.Transform (expression);

      Assert.That (result, Is.Not.SameAs (expression));
      Assert.That (result, Is.InstanceOf<ConstantExpression> ().With.Property ("Value").EqualTo ("Replaced!"));
    }

    [Test]
    public void Transform_NullTransformer ()
    {
      var instance = Expression.Constant (null, typeof (DomainType));
      var expression = Expression.Call (instance, "MethodWithNullTransformer", Type.EmptyTypes);

      Assert.That (
          () => _transformer.Transform (expression),
          Throws.InvalidOperationException.With.Message.EqualTo (
              "The 'NullTransformerAttribute' on method "
              + "'Remotion.Linq.UnitTests.Parsing.ExpressionTreeVisitors.Transformation.PredefinedTransformations.AttributeEvaluatingExpressionTransformerTest+DomainType.MethodWithNullTransformer'"
              + " returned 'null' instead of a transformer."));
    }

    [Test]
    public void Transform_Property_NoAttribute ()
    {
      var instance = Expression.Constant (null, typeof (DomainType));
      var expression = Expression.Property (instance, "PropertyWithoutTransformer");

      var result = _transformer.Transform (expression);

      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void Transform_Property_WithAttribute ()
    {
      var instance = Expression.Constant (null, typeof (DomainType));
      var expression = Expression.Property (instance, "PropertyWithTransformer");

      var result = _transformer.Transform (expression);

      Assert.That (result, Is.Not.SameAs (expression));
      Assert.That (result, Is.InstanceOf<ConstantExpression> ().With.Property ("Value").EqualTo ("Replaced!"));
    }

    [Test]
    public void Transform_Property_WithAttribute_PrivateGetter ()
    {
      var instance = Expression.Constant (null, typeof (DomainType));
      var expression = Expression.Property (instance, "PropertyWithTransformerAndPrivateGetter");

      var result = _transformer.Transform (expression);

      Assert.That (result, Is.Not.SameAs (expression));
      Assert.That (result, Is.InstanceOf<ConstantExpression> ().With.Property ("Value").EqualTo ("Replaced!"));
    }

    [Test]
    public void Transform_Field ()
    {
      var instance = Expression.Constant (null, typeof (DomainType));
      var expression = Expression.Field (instance, "Field");

      var result = _transformer.Transform (expression);

      Assert.That (result, Is.SameAs (expression));
    }

    public class DomainType
    {
      [UsedImplicitly]
      [Transformer]
      public virtual int MethodWithAttribute ()
      {
        return 0;
      }

      [UsedImplicitly]
      [Transformer]
      [Transformer]
      public int MethodWithTwoAttributes ()
      {
        return 0;
      }

      [UsedImplicitly]
      [NullTransformer]
      public int MethodWithNullTransformer ()
      {
        return 0;
      }

      [UsedImplicitly]
      public int PropertyWithoutTransformer { get; set; }
      [UsedImplicitly]
      public int PropertyWithTransformer { [Transformer] get; set; }
      [UsedImplicitly]
      public int PropertyWithTransformerAndPrivateGetter { [Transformer] private get; set; }

      public int Field;
    }

    public class DerivedDomainType : DomainType
    {
      [UsedImplicitly]
      public override int MethodWithAttribute ()
      {
        return base.MethodWithAttribute();
      }
    }

    [AttributeUsage (AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    private class TransformerAttribute : Attribute, AttributeEvaluatingExpressionTransformer.IMethodCallExpressionTransformerAttribute
    {
      public IExpressionTransformer<MethodCallExpression> GetExpressionTransformer (MethodCallExpression expression)
      {
        return new Transformer (expression);
      }
    }

    public class Transformer : IExpressionTransformer<MethodCallExpression>
    {
      private readonly MethodCallExpression _expressionViaCtor;

      public Transformer (MethodCallExpression expressionViaCtor)
      {
        Assert.That (expressionViaCtor, Is.Not.Null);

        _expressionViaCtor = expressionViaCtor;
      }

      public ExpressionType[] SupportedExpressionTypes
      {
        get { throw new NotImplementedException(); }
      }

      public Expression Transform (MethodCallExpression expression)
      {
        Assert.That (expression, Is.SameAs (_expressionViaCtor));

        return Expression.Constant ("Replaced!");
      }
    }

    [AttributeUsage (AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    private class NullTransformerAttribute : Attribute, AttributeEvaluatingExpressionTransformer.IMethodCallExpressionTransformerAttribute
    {
      public IExpressionTransformer<MethodCallExpression> GetExpressionTransformer (MethodCallExpression expression)
      {
        return null;
      }
    }
  }
}
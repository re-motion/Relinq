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
using JetBrains.Annotations;
using NUnit.Framework;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.Parsing.ExpressionTreeVisitors.Transformation;
using Remotion.Linq.Parsing.ExpressionTreeVisitors.Transformation.PredefinedTransformations;
using Remotion.Linq.UnitTests.TestDomain;

namespace Remotion.Linq.UnitTests.Parsing.ExpressionTreeVisitors.Transformation.PredefinedTransformations
{
  [TestFixture]
  public class MethodCallExpressionTransformerAttributeTest
  {
    private MethodCallExpressionTransformerAttribute _attribute;

    [SetUp]
    public void SetUp ()
    {
      _attribute = new MethodCallExpressionTransformerAttribute (typeof (FakeTransformer));
    }

    [Test]
    public void AttributeUsage ()
    {
      var usage = (AttributeUsageAttribute) typeof (MethodCallExpressionTransformerAttribute).GetCustomAttributes (typeof (AttributeUsageAttribute), false).Single();
      Assert.That (usage.AllowMultiple, Is.False);
      Assert.That (usage.Inherited, Is.False);
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_attribute.TransformerType, Is.SameAs (typeof (FakeTransformer)));
    }

    [Test]
    public void Initialization_InvalidType ()
    {
      Assert.That (
          () => new MethodCallExpressionTransformerAttribute (typeof (string)),
          Throws.ArgumentException.With.Message.EqualTo (
              "Parameter 'transformerType' is a 'System.String', which cannot be assigned to type "
              + "'Remotion.Linq.Parsing.ExpressionTreeVisitors.Transformation.IExpressionTransformer`1[System.Linq.Expressions.MethodCallExpression]'."
              + "\r\nParameter name: transformerType"));
    }

    [Test]
    public void GetExpressionTransformer ()
    {
      var expressionTransformer = _attribute.GetExpressionTransformer (ExpressionHelper.CreateMethodCallExpression<Cook>());

      Assert.That (expressionTransformer, Is.TypeOf<FakeTransformer>());
    }

    [Test]
    public void GetExpressionTransformer_NotInstantiable ()
    {
      var attribute = new MethodCallExpressionTransformerAttribute (typeof (FakeTransformerWithoutDefaultCtor));
      Assert.That (
          () => attribute.GetExpressionTransformer (ExpressionHelper.CreateMethodCallExpression<Cook>()),
          Throws.InvalidOperationException.With.Message.EqualTo (
              "The method call transformer "
              + "'Remotion.Linq.UnitTests.Parsing.ExpressionTreeVisitors.Transformation.PredefinedTransformations.MethodCallExpressionTransformerAttributeTest+FakeTransformerWithoutDefaultCtor' "
              + "has no public default constructor and therefore cannot be used with the MethodCallExpressionTransformerAttribute."));
    }

    public class FakeTransformer : IExpressionTransformer<MethodCallExpression>
    {
      public ExpressionType[] SupportedExpressionTypes
      {
        get { throw new NotImplementedException(); }
      }

      public Expression Transform (MethodCallExpression expression)
      {
        throw new NotImplementedException();
      }
    }

    public class FakeTransformerWithoutDefaultCtor : IExpressionTransformer<MethodCallExpression>
    {
      [UsedImplicitly]
      public FakeTransformerWithoutDefaultCtor (int i)
      {
      }

      public ExpressionType[] SupportedExpressionTypes
      {
        get { throw new NotImplementedException(); }
      }

      public Expression Transform (MethodCallExpression expression)
      {
        throw new NotImplementedException();
      }
    }
  }
}
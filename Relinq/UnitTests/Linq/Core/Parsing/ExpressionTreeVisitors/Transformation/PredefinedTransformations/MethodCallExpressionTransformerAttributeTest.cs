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
using JetBrains.Annotations;
using NUnit.Framework;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.Parsing.ExpressionTreeVisitors.Transformation;
using Remotion.Linq.Parsing.ExpressionTreeVisitors.Transformation.PredefinedTransformations;
using Remotion.Linq.UnitTests.Linq.Core.TestDomain;

namespace Remotion.Linq.UnitTests.Linq.Core.Parsing.ExpressionTreeVisitors.Transformation.PredefinedTransformations
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
              + "'Remotion.Linq.UnitTests.Linq.Core.Parsing.ExpressionTreeVisitors.Transformation.PredefinedTransformations.MethodCallExpressionTransformerAttributeTest+FakeTransformerWithoutDefaultCtor' "
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
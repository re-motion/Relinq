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
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using Remotion.Linq.Parsing.ExpressionTreeVisitors.Transformation.PredefinedTransformations;

namespace Remotion.Linq.UnitTests.Linq.Core.Parsing.ExpressionTreeVisitors.Transformation.PredefinedTransformations
{
  [TestFixture]
  public class KeyValuePairNewExpressionTransformerTest
  {
    private KeyValuePairNewExpressionTransformer _transformer;
    [SetUp]
    public void SetUp ()
    {
      _transformer = new KeyValuePairNewExpressionTransformer ();
    }

    [Test]
    public void SupportedExpressionTypes ()
    {
      Assert.That (_transformer.SupportedExpressionTypes, Is.EqualTo (new[] { ExpressionType.New }));
    }

    [Test]
    public void Transform_OtherExpression ()
    {
      var expression = Expression.New(typeof(List<int>).GetConstructor(new[] { typeof(int) }), Expression.Constant(0));

      var result = _transformer.Transform (expression);

      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void Transform_KeyValuePair_DefaultCtor ()
    {
      var expression = Expression.New (typeof (KeyValuePair<string, int>));

      var result = _transformer.Transform (expression);

      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void Transform_KeyValuePair_ValueCtor ()
    {
      var expression = Expression.New (
          typeof (KeyValuePair<string, int>).GetConstructor (new[] { typeof (string), typeof(int) }),
          Expression.Constant ("test"),
          Expression.Constant (0));

      var result = _transformer.Transform (expression);

      if (Environment.Version.Major == 2)
      {
        var expectedExpression35 = Expression.New (
            typeof (KeyValuePair<string, int>).GetConstructor (new[] { typeof (string), typeof (int) }),
            new Expression[] { Expression.Constant ("test"), Expression.Constant (0) },
            new MemberInfo[] { typeof (KeyValuePair<string, int>).GetMethod ("get_Key"), typeof (KeyValuePair<string, int>).GetMethod ("get_Value") });
        ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression35, result);
      }
      else
      {
        var expectedExpression40 = Expression.New (
            typeof (KeyValuePair<string, int>).GetConstructor (new[] { typeof (string), typeof (int) }),
            new Expression[] { Expression.Constant ("test"), Expression.Constant (0) },
            new MemberInfo[] { typeof (KeyValuePair<string, int>).GetProperty ("Key"), typeof (KeyValuePair<string, int>).GetProperty ("Value") });
        ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression40, result);
      }
    }

    [Test]
    public void Transform_KeyValuePair_ValueCtor_WithMembers_IsIgnored ()
    {
      var expression = Expression.New (
          typeof (KeyValuePair<string, int>).GetConstructor (new[] { typeof (string), typeof (int) }),
          new Expression[] { Expression.Constant ("test"), Expression.Constant (0) },
          new MemberInfo[] { typeof (KeyValuePair<string, int>).GetMethod ("get_Key"), typeof (KeyValuePair<string, int>).GetMethod ("get_Value") });

      var result = _transformer.Transform (expression);

      Assert.That (result, Is.SameAs (expression));
    }
  }
}
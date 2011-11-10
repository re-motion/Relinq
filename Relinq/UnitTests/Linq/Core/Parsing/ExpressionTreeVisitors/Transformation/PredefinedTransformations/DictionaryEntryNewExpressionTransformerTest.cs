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
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using Remotion.Linq.Parsing.ExpressionTreeVisitors.Transformation.PredefinedTransformations;

namespace Remotion.Linq.UnitTests.Linq.Core.Parsing.ExpressionTreeVisitors.Transformation.PredefinedTransformations
{
  [TestFixture]
  public class DictionaryEntryNewExpressionTransformerTest
  {
    private DictionaryEntryNewExpressionTransformer _transformer;

    [SetUp]
    public void SetUp ()
    {
      _transformer = new DictionaryEntryNewExpressionTransformer();
    }

    [Test]
    public void SupportedExpressionTypes ()
    {
      Assert.That (_transformer.SupportedExpressionTypes, Is.EqualTo (new[] { ExpressionType.New }));
    }

    [Test]
    public void Transform_OtherExpression ()
    {
      var expression = Expression.New (typeof (List<int>).GetConstructor (new[] { typeof (int) }), Expression.Constant (0));

      var result = _transformer.Transform (expression);

      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void Transform_DictionaryEntry_DefaultCtor ()
    {
      var expression = Expression.New (typeof (DictionaryEntry));

      var result = _transformer.Transform (expression);

      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void Transform_DictionaryEntry_ValueCtor ()
    {
      var expression = Expression.New (
          typeof (DictionaryEntry).GetConstructor (new[] { typeof (object), typeof (object) }),
          Expression.Constant ("test"),
          Expression.Constant ("v"));

      var result = _transformer.Transform (expression);

      if (Environment.Version.Major == 2)
      {
        var expectedExpression35 = Expression.New (
            typeof (DictionaryEntry).GetConstructor (new[] { typeof (object), typeof (object) }),
            new Expression[] { Expression.Convert (Expression.Constant ("test"), typeof (object)), Expression.Convert (Expression.Constant ("v"), typeof (object)) },
            new MemberInfo[] { typeof (DictionaryEntry).GetMethod ("get_Key"), typeof (DictionaryEntry).GetMethod ("get_Value") });
        ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression35, result);
      }
      else
      {
        var expectedExpression40 = Expression.New (
            typeof (DictionaryEntry).GetConstructor (new[] { typeof (object), typeof (object) }),
            new Expression[]
            { Expression.Convert (Expression.Constant ("test"), typeof (object)), Expression.Convert (Expression.Constant ("v"), typeof (object)) },
            new MemberInfo[] { typeof (DictionaryEntry).GetProperty ("Key"), typeof (DictionaryEntry).GetProperty ("Value") });
        ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression40, result);
      }
    }

    [Test]
    public void Transform_DictionaryEntry_ValueCtor_WithMembers_IsIgnored ()
    {
      var expression = Expression.New (
          typeof (DictionaryEntry).GetConstructor (new[] { typeof (object), typeof (object) }),
          new Expression[] { Expression.Convert (Expression.Constant ("test"), typeof (object)), Expression.Convert (Expression.Constant ("v"), typeof (object)) },
          new MemberInfo[] { typeof (DictionaryEntry).GetMethod ("get_Key"), typeof (DictionaryEntry).GetMethod ("get_Value") });

      var result = _transformer.Transform (expression);

      Assert.That (result, Is.SameAs (expression));
    }
  }
}
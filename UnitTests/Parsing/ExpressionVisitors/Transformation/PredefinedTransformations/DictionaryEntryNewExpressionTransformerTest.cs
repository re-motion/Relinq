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
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.Parsing.ExpressionVisitors.Transformation.PredefinedTransformations;

namespace Remotion.Linq.UnitTests.Parsing.ExpressionVisitors.Transformation.PredefinedTransformations
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
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
using NUnit.Framework;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.Parsing.ExpressionVisitors.Transformation.PredefinedTransformations;

namespace Remotion.Linq.UnitTests.Parsing.ExpressionVisitors.Transformation.PredefinedTransformations
{
  [TestFixture]
  public class NullableValueTransformerTest
  {
    private NullableValueTransformer _transformer;

    [SetUp]
    public void SetUp ()
    {
      _transformer = new NullableValueTransformer();
    }

    [Test]
    public void SupportedExpressionTypes ()
    {
      Assert.That (_transformer.SupportedExpressionTypes, Is.EqualTo (new[] { ExpressionType.MemberAccess }));
    }

    [Test]
    public void Transform_NoValueOrHasValueMember()
    {
      var expression = Expression.MakeMemberAccess (Expression.Constant ("test"), typeof (string).GetProperty ("Length"));

      var result = _transformer.Transform (expression);

      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void Transform_ValueMember_NoGenericType ()
    {
      var expression = Expression.MakeMemberAccess (Expression.Constant(new DictionaryEntry()), typeof (DictionaryEntry).GetProperty ("Value"));

      var result = _transformer.Transform (expression);

      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void Transform_ValueMember_GenericTypeWithNoNullableGenericTypeDefinition ()
    {
      var expression = Expression.MakeMemberAccess (
          Expression.Constant (new KeyValuePair<string, string> ("key", "value")), typeof (KeyValuePair<string,string>).GetProperty ("Value"));

      var result = _transformer.Transform (expression);

      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void Transform_ValueMember_GenericTypeWithNullableGenericTypeDefinition ()
    {
      var expression = Expression.MakeMemberAccess (Expression.Constant (5, typeof (int?)), typeof (int?).GetProperty ("Value"));

      var result = _transformer.Transform (expression);

      var expectedExpression = Expression.Convert (expression.Expression, typeof (int));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void Transform_HasValueMember_GenericTypeWithNullableGenericTypeDefinition ()
    {
      var expression = Expression.Constant (5, typeof(int?));
      var memberExpression = Expression.MakeMemberAccess (expression, typeof (int?).GetProperty ("HasValue"));

      var result = _transformer.Transform (memberExpression);

      var expextedExpression = Expression.NotEqual (expression, Expression.Constant (null, typeof(int?)));
      ExpressionTreeComparer.CheckAreEqualTrees (expextedExpression, result);
    }

  }
}
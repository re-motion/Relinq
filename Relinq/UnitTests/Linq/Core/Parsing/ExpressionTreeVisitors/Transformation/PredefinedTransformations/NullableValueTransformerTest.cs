// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// as published by the Free Software Foundation; either version 2.1 of the 
// License, or (at your option) any later version.
// 
// re-motion is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-motion; if not, see http://www.gnu.org/licenses.
// 
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Linq.Parsing.ExpressionTreeVisitors.Transformation.PredefinedTransformations;

namespace Remotion.Linq.UnitTests.Linq.Core.Parsing.ExpressionTreeVisitors.Transformation.PredefinedTransformations
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
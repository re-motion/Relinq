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
using System.Collections.Generic;
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.Parsing;

namespace Remotion.Linq.UnitTests.Parsing
{
  [TestFixture]
  public class TupleExpressionBuilderTest
  {
    [Test]
    public void AggregateExpressionsIntoTuple_OneExpression ()
    {
      var expression1 = Expression.Constant ("expr1");
      var tupleExpression = TupleExpressionBuilder.AggregateExpressionsIntoTuple (new[] { expression1 });

      Assert.That (tupleExpression, Is.SameAs (expression1));
    }

    [Test]
    public void AggregateExpressionsIntoTuple_TwoExpressions ()
    {
      var expression1 = Expression.Constant ("expr1");
      var expression2 = Expression.Constant ("expr2");

      var result = TupleExpressionBuilder.AggregateExpressionsIntoTuple (new[] { expression1, expression2 });

      var tupleCtor = typeof (KeyValuePair<string, string>).GetConstructor (new[] { typeof (string), typeof (string) });
      var expectedResult = Expression.New (
          tupleCtor,
          new[] { expression1, expression2 },
          tupleCtor.DeclaringType.GetMethod ("get_Key"),
          tupleCtor.DeclaringType.GetMethod ("get_Value"));

      ExpressionTreeComparer.CheckAreEqualTrees (result, expectedResult);
    }

    [Test]
    public void AggregateExpressionsIntoTuple_ThreeeExpressions ()
    {
      var expression1 = Expression.Constant ("expr1");
      var expression2 = Expression.Constant ("expr2");
      var expression3 = Expression.Constant ("expr3");

      var result = TupleExpressionBuilder.AggregateExpressionsIntoTuple (new[] { expression1, expression2, expression3 });

      var innerTupleCtor = typeof (KeyValuePair<string, string>).GetConstructor (new[] { typeof (string), typeof (string) });
      var outerTupleCtor =
          typeof (KeyValuePair<string, KeyValuePair<string, string>>).GetConstructor (
              new[] { typeof (string), typeof (KeyValuePair<string, string>) });

      var innerTupleKeyGetter = innerTupleCtor.DeclaringType.GetMethod ("get_Key");
      var innerTupleValueGetter = innerTupleCtor.DeclaringType.GetMethod ("get_Value");
      var outerTupleKeyGetter = outerTupleCtor.DeclaringType.GetMethod ("get_Key");
      var outerTupleValueGetter = outerTupleCtor.DeclaringType.GetMethod ("get_Value");

      var expectedResult = Expression.New (
          outerTupleCtor,
          new Expression[]
          {
              expression1,
              Expression.New (innerTupleCtor, new[] { expression2, expression3 }, innerTupleKeyGetter, innerTupleValueGetter)
          },
          outerTupleKeyGetter,
          outerTupleValueGetter);

      ExpressionTreeComparer.CheckAreEqualTrees (result, expectedResult);
    }

    [Test]
    public void GetExpressionsFromTuple_OneExpression ()
    {
      var expression = Expression.Constant ("expr1");
      
      var result = TupleExpressionBuilder.GetExpressionsFromTuple (expression);

      var enumerator = result.GetEnumerator ();
      enumerator.MoveNext ();
      Assert.That (enumerator.Current, Is.SameAs (expression));
    }

    [Test]
    public void GetExpressionsFromTuple_TwoExpressions ()
    {
      var expression1 = Expression.Constant ("expr1");
      var expression2 = Expression.Constant ("expr2");
      var tupleCtor = typeof (KeyValuePair<string, string>).GetConstructor (new[] { typeof (string), typeof (string) });
      var tupleExpression = Expression.New (
          tupleCtor,
          new[] { expression1, expression2 },
          tupleCtor.DeclaringType.GetMethod ("get_Key"),
          tupleCtor.DeclaringType.GetMethod ("get_Value"));

      var result = TupleExpressionBuilder.GetExpressionsFromTuple (tupleExpression);

      var enumerator = result.GetEnumerator();
      enumerator.MoveNext();
      ExpressionTreeComparer.CheckAreEqualTrees (
          enumerator.Current, MemberExpression.MakeMemberAccess (tupleExpression, tupleCtor.DeclaringType.GetProperty ("Key")));
      enumerator.MoveNext();
      ExpressionTreeComparer.CheckAreEqualTrees (
          enumerator.Current, MemberExpression.MakeMemberAccess (tupleExpression, tupleCtor.DeclaringType.GetProperty ("Value")));
    }

    [Test]
    public void GetExpressionsFromTuple_ThreeExpressions ()
    {
      var expression1 = Expression.Constant ("expr1");
      var expression2 = Expression.Constant ("expr2");
      var expression3 = Expression.Constant ("expr3");

      var innerTupleCtor = typeof (KeyValuePair<string, string>).GetConstructor (new[] { typeof (string), typeof (string) });
      var outerTupleCtor =
          typeof (KeyValuePair<string, KeyValuePair<string, string>>).GetConstructor (
              new[] { typeof (string), typeof (KeyValuePair<string, string>) });

      var innerTupleKeyGetter = innerTupleCtor.DeclaringType.GetMethod ("get_Key");
      var innerTupleValueGetter = innerTupleCtor.DeclaringType.GetMethod ("get_Value");
      var outerTupleKeyGetter = outerTupleCtor.DeclaringType.GetMethod ("get_Key");
      var outerTupleValueGetter = outerTupleCtor.DeclaringType.GetMethod ("get_Value");

      var tupleExpression = Expression.New (
          outerTupleCtor,
          new Expression[]
          {
              expression1,
              Expression.New (innerTupleCtor, new[] { expression2, expression3 }, innerTupleKeyGetter, innerTupleValueGetter)
          },
          outerTupleKeyGetter,
          outerTupleValueGetter);

      var result = TupleExpressionBuilder.GetExpressionsFromTuple (tupleExpression);

      var enumerator = result.GetEnumerator();
      enumerator.MoveNext();
      ExpressionTreeComparer.CheckAreEqualTrees (
          enumerator.Current, MemberExpression.MakeMemberAccess (tupleExpression, outerTupleCtor.DeclaringType.GetProperty ("Key")));
      enumerator.MoveNext();
      ExpressionTreeComparer.CheckAreEqualTrees (
          enumerator.Current,
          MemberExpression.MakeMemberAccess (
              MemberExpression.MakeMemberAccess (tupleExpression, outerTupleCtor.DeclaringType.GetProperty ("Value")),
              innerTupleCtor.DeclaringType.GetProperty ("Key")));
      enumerator.MoveNext();
      ExpressionTreeComparer.CheckAreEqualTrees (
          enumerator.Current,
          MemberExpression.MakeMemberAccess (
              MemberExpression.MakeMemberAccess (tupleExpression, outerTupleCtor.DeclaringType.GetProperty ("Value")),
              innerTupleCtor.DeclaringType.GetProperty ("Value")));
    }
  }
}
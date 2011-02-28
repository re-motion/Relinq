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
using System.Collections.Generic;
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Linq.Parsing;

namespace Remotion.Linq.UnitTests.Linq.Core.Parsing
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
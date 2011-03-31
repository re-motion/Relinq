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
using System.Reflection;
using NUnit.Framework;
using Remotion.Linq.Parsing.ExpressionTreeVisitors.Transformation.PredefinedTransformations;

namespace Remotion.Linq.UnitTests.Linq.Core.Parsing.ExpressionTreeVisitors.Transformation.PredefinedTransformations
{
  [TestFixture]
  public class TupleNewExpressionTransformerTest
  {
    private TupleNewExpressionTransformer _transformer;

    [SetUp]
    public void SetUp ()
    {
      _transformer = new TupleNewExpressionTransformer ();
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
    public void Transform_Tuple2_ValueCtor ()
    {
      var expression = Expression.New (
          typeof (Tuple<string, int>).GetConstructor (new[] { typeof (string), typeof (int) }),
          Expression.Constant ("test"),
          Expression.Constant (0));

      var result = _transformer.Transform (expression);

      if (Environment.Version.Major == 2)
      {
        var expectedExpression35 = Expression.New (
            typeof (Tuple<string, int>).GetConstructor (new[] { typeof (string), typeof (int) }),
            new Expression[] { Expression.Constant ("test"), Expression.Constant (0) },
            new MemberInfo[] { typeof (Tuple<string, int>).GetMethod ("get_Item1"), typeof (Tuple<string, int>).GetMethod ("get_Item2") });
        ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression35, result);
      }
      else
      {
        var expectedExpression40 = Expression.New (
            typeof (Tuple<string, int>).GetConstructor (new[] { typeof (string), typeof (int) }),
            new Expression[] { Expression.Constant ("test"), Expression.Constant (0) },
            new MemberInfo[] { typeof (Tuple<string, int>).GetProperty ("Item1"), typeof (Tuple<string, int>).GetProperty ("Item2") });
        ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression40, result);
      }
    }

    [Test]
    public void Transform_Tuple3_ValueCtor ()
    {
      var expression = Expression.New (
          typeof (Tuple<string, int, bool>).GetConstructor (new[] { typeof (string), typeof (int), typeof (bool) }),
          Expression.Constant ("test"),
          Expression.Constant (0),
          Expression.Constant (true));

      var result = _transformer.Transform (expression);

      if (Environment.Version.Major == 2)
      {
        var expectedExpression35 = Expression.New (
            typeof (Tuple<string, int, bool>).GetConstructor (new[] { typeof (string), typeof (int), typeof (bool) }),
            new Expression[] { Expression.Constant ("test"), Expression.Constant (0), Expression.Constant (true) },
            new MemberInfo[]
            {
                typeof (Tuple<string, int, bool>).GetMethod ("get_Item1"),
                typeof (Tuple<string, int, bool>).GetMethod ("get_Item2"),
                typeof (Tuple<string, int, bool>).GetMethod ("get_Item3")
            });
        ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression35, result);
      }
      else
      {
        var expectedExpression40 = Expression.New (
            typeof (Tuple<string, int, bool>).GetConstructor (new[] { typeof (string), typeof (int), typeof (bool) }),
            new Expression[] { Expression.Constant ("test"), Expression.Constant (0), Expression.Constant (true) },
            new MemberInfo[]
            {
                typeof (Tuple<string, int, bool>).GetProperty ("Item1"),
                typeof (Tuple<string, int, bool>).GetProperty ("Item2"),
                typeof (Tuple<string, int, bool>).GetProperty ("Item3")
            });
        ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression40, result);
      }
    }

    [Test]
    public void Transform_Tuple_ValueCtor_WithMembers_IsIgnored ()
    {
      var expression = Expression.New (
          typeof (Tuple<string, int, bool>).GetConstructor (new[] { typeof (string), typeof (int), typeof (bool) }),
          new Expression[] { Expression.Constant ("test"), Expression.Constant (0), Expression.Constant (true) },
          new MemberInfo[]
          {
              typeof (Tuple<string, int, bool>).GetProperty ("Item1"), 
              typeof (Tuple<string, int, bool>).GetProperty ("Item2"),
              typeof (Tuple<string, int, bool>).GetProperty ("Item3")
          });

      var result = _transformer.Transform (expression);

      Assert.That (result, Is.SameAs (expression));
    }
  }
}
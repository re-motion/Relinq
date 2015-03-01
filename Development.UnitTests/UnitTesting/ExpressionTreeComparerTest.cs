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
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Linq.Development.UnitTesting;

namespace Remotion.Linq.Development.UnitTests.UnitTesting
{
  [TestFixture]
  public class ExpressionTreeComparerTest
  {
    private class TestableExpressionTreeComparer : ExpressionTreeComparerBase
    {
      public TestableExpressionTreeComparer (string expectedInitial, string actualInitial, params Type[] additionalStructurallyComparedTypes)
          : base(expectedInitial, actualInitial, additionalStructurallyComparedTypes)
      {
      }

      public new void CheckAreEqualNodes (Expression expected, Expression actual)
      {
        base.CheckAreEqualNodes (expected, actual);
      }
    }

    [Test]
    public void CheckAreEqualTrees_WithSimpleExpressionAndEqualValues_DoesNotThrow ()
    {
      Expression expected = Expression.Constant ("a");
      Expression actual = Expression.Constant ("a");
      Assert.That (() => ExpressionTreeComparer.CheckAreEqualTrees (expected, actual), Throws.Nothing);
    }

    [Test]
    public void CheckAreEqualTrees_WithNestedExpressionAndEqualValues_DoesNotThrow ()
    {
      Expression expected = Expression.Equal (Expression.Constant ("a"), Expression.Constant ("b"));
      Expression actual = Expression.Equal (Expression.Constant ("a"), Expression.Constant ("b"));
      Assert.That (() => ExpressionTreeComparer.CheckAreEqualTrees (expected, actual), Throws.Nothing);
    }

    [Test]
    public void CheckAreEqualTrees_WithListValueAndEqualValues_DoesNotThrow ()
    {
      Expression expected = Expression.Constant (new[] { "a" }, typeof (string[]));
      Expression actual = Expression.Constant (new[] { "a" }, typeof (string[]));
      Assert.That (() => ExpressionTreeComparer.CheckAreEqualTrees (expected, actual), Throws.Nothing);
    }

    [Test]
    public void CheckAreEqualTrees_WithStructurallyComparedTypesAndEqualValues_DoesNotThrow ()
    {
      Expression expected = Expression.Constant (Tuple.Create ("a"));
      Expression actual = Expression.Constant (Tuple.Create ("a"));
      var expressionTreeComparer = new TestableExpressionTreeComparer ("expected", "actual", typeof (Tuple<string>));
      Assert.That (() => expressionTreeComparer.CheckAreEqualNodes (expected, actual), Throws.Nothing);
    }

    [Test]
    public void CheckAreEqualTrees_WithSimpleExpressionAndValuesNotEqual_Throws ()
    {
      Expression expected = Expression.Constant ("a");
      Expression actual = Expression.Constant ("b");
      Assert.That (
          () => ExpressionTreeComparer.CheckAreEqualTrees (expected, actual),
          Throws.InvalidOperationException.With.Message.EqualTo (
              "Trees are not equal: Property 'Value'\nNode 1: \"a\"\nNode 2: \"b\"\n"
              + "Tree 1: \"a\"\nTree 2: \"b\""));
    }

    [Test]
    public void CheckAreEqualTrees_WithNestedExpressionAndValuesNotEqual_Throws ()
    {
      Expression expected = Expression.Equal (Expression.Constant ("a"), Expression.Constant ("b"));
      Expression actual = Expression.Equal (Expression.Constant ("b"), Expression.Constant ("a"));
      Assert.That (
          () => ExpressionTreeComparer.CheckAreEqualTrees (expected, actual),
          Throws.InvalidOperationException.With.Message.EqualTo (
              "Trees are not equal: Property 'Value'\n"
#if !NET_3_5
              + "Node 1: \"b\"\nNode 2: \"a\"\n"
#else
              + "Node 1: \"a\"\nNode 2: \"b\"\n"
#endif
#if !NET_3_5
              + "Tree 1: (\"a\" == \"b\")\nTree 2: (\"b\" == \"a\")"
#else
              + "Tree 1: (\"a\" = \"b\")\nTree 2: (\"b\" = \"a\")"
#endif
              ));
    }

    [Test]
    public void CheckAreEqualTrees_WithListValueAndLengthNotEqual_Throws ()
    {
      Expression expected = Expression.Constant (new[] { "a", "b", "c", "d" }, typeof (string[]));
      Expression actual = Expression.Constant (new[] { "a", "b", "c" }, typeof (string[]));
      Assert.That (
          () => ExpressionTreeComparer.CheckAreEqualTrees (expected, actual),
          Throws.InvalidOperationException.With.Message.EqualTo (
              "Trees are not equal: Number of elements in property 'Value'\nNode 1: 4\nNode 2: 3\n"
              + "Tree 1: value(System.String[])\nTree 2: value(System.String[])"));
    }

    [Test]
    public void CheckAreEqualTrees_WithListValueAndValueNotEqual_Throws ()
    {
      Expression expected = Expression.Constant (new[] { "a", "c" }, typeof (string[]));
      Expression actual = Expression.Constant (new[] { "a", "b" }, typeof (string[]));
      Assert.That (
          () => ExpressionTreeComparer.CheckAreEqualTrees (expected, actual),
          Throws.InvalidOperationException.With.Message.EqualTo (
              "Trees are not equal: Property 'Value'\nNode 1: value(System.String[])\nNode 2: value(System.String[])\n"
              + "Tree 1: value(System.String[])\nTree 2: value(System.String[])"));
    }

    [Test]
    public void CheckAreEqualTrees_WithListValueAndItemTypesNotEqual_Throws ()
    {
      Expression expected = Expression.Constant (new object[] { "a" }, typeof (object[]));
      Expression actual = Expression.Constant (new object[] { 6 }, typeof (object[]));
      Assert.That (
          () => ExpressionTreeComparer.CheckAreEqualTrees (expected, actual),
          Throws.InvalidOperationException.With.Message.EqualTo (
              "The item types of the items in the lists in property 'Value' differ: One is 'System.String', the other is 'System.Int32'.\n"
              + "Tree 1: value(System.Object[])\nTree 2: value(System.Object[])"));
    }

    [Test]
    public void CheckAreEqualTrees_WithListValueAndOneListIsNull_Throws ()
    {
      Expression expected = Expression.Constant (new[] { "a" }, typeof (string[]));
      Expression actual = Expression.Constant (null, typeof (string[]));
      Assert.That (
          () => ExpressionTreeComparer.CheckAreEqualTrees (expected, actual),
          Throws.InvalidOperationException.With.Message.EqualTo ("One of the lists in property 'Value' is null."));
    }

    [Test]
    public void CheckAreEqualTrees_WithStructurallyComparedTypesAndNotEqualValues_Throws ()
    {
      Expression expected = Expression.Constant (Tuple.Create ("a"));
      Expression actual = Expression.Constant (Tuple.Create ("b"));
      var expressionTreeComparer = new TestableExpressionTreeComparer ("expected", "actual", typeof (Tuple<string>));
      Assert.That (
          () => expressionTreeComparer.CheckAreEqualNodes (expected, actual),
          Throws.InvalidOperationException.With.Message.EqualTo (
              "Trees are not equal: Property 'Item1'\n"
#if !NET_3_5
              + "Node 1: (a)\nNode 2: (b)\n"
#else
              + "Node 1: <a>\nNode 2: <b>\n"
#endif
              + "Tree 1: expected\nTree 2: actual"));
    }

    [Test]
    public void CheckAreEqualTrees_WithDerivedExpressionAndBaseClassIsEqual_DoesNotThrow ()
    {
      Expression expected = Expression.Call (Expression.Parameter (typeof (object), "o"), typeof (object).GetMethod ("ToString"));
      Expression<Func<object, string>> actualLamba = o => o.ToString();
      Expression actual = actualLamba.Body;
      Assert.That (() => ExpressionTreeComparer.CheckAreEqualTrees (expected, actual), Throws.Nothing);
    }
  }
}
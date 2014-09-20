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
    public void CheckAreEqualTrees_WithSimpleExpressionAndValuesNotEqual_Throws ()
    {
      Expression expected = Expression.Constant ("a");
      Expression actual = Expression.Constant ("b");
      Assert.That (
          () => ExpressionTreeComparer.CheckAreEqualTrees (expected, actual),
          Throws.InvalidOperationException.With.Message.EqualTo (
              "Trees are not equal: Property Value\nNode 1: \"a\"\nNode 2: \"b\"\nTree 1: \"a\"\nTree 2: \"b\""));
    }

    [Test]
    public void CheckAreEqualTrees_WithNestedExpressionAndValuesNotEqual_Throws ()
    {
      Expression expected = Expression.Equal (Expression.Constant ("a"), Expression.Constant ("b"));
      Expression actual = Expression.Equal (Expression.Constant ("b"), Expression.Constant ("a"));
      Assert.That (
          () => ExpressionTreeComparer.CheckAreEqualTrees (expected, actual),
          Throws.InvalidOperationException.With.Message.EqualTo (
              "Trees are not equal: Property Value\nNode 1: \"b\"\nNode 2: \"a\"\nTree 1: (\"a\" == \"b\")\nTree 2: (\"b\" == \"a\")"));
    }

    [Test]
    public void CheckAreEqualTrees_WithDerivedExpressionAndBaseClassIsEqual_DoesNotThrow()
    {
      Expression expected = Expression.Call (Expression.Parameter (typeof (object), "o"), typeof (object).GetMethod ("ToString"));
      Expression<Func<object, string>> actualLamba = o => o.ToString();
      Expression actual = actualLamba.Body;
      Assert.That (() => ExpressionTreeComparer.CheckAreEqualTrees (expected, actual), Throws.Nothing);
    }
  }
}
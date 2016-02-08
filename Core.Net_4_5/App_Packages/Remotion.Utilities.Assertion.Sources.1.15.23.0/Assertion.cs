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
using System.Diagnostics;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace Remotion.Utilities
{
  /// <summary>
  /// Provides methods that throw an <see cref="InvalidOperationException"/> if an assertion fails.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///   This class contains methods that are conditional to the DEBUG and TRACE attributes (<see cref="DebugAssert(bool)"/> and <see cref="TraceAssert(bool)"/>). 
  ///   </para><para>
  ///   Note that assertion expressions passed to these methods are not evaluated (read: executed) if the respective symbol are not defined during
  ///   compilation, nor are the methods called. This increases performance for production builds, but make sure that your assertion expressions do
  ///   not cause any side effects! See <see cref="ConditionalAttribute"/> or <see cref="Debug"/> and <see cref="T:System.Diagnostics.Trace"/> the for more information 
  ///   about conditional compilation.
  ///   </para><para>
  ///   Assertions are no replacement for checking input parameters of public methods (see <see cref="ArgumentUtility"/>).  
  ///   </para>
  /// </remarks>
  static partial class Assertion
  {
    private const string c_msgIsTrue = "Assertion failed: Expression evaluates to true.";
    private const string c_msgIsFalse = "Assertion failed: Expression evaluates to false.";
    private const string c_msgIsNull = "Assertion failed: Expression evaluates to a null reference.";
    private const string c_msgIsNotNull = "Assertion failed: Expression does not evaluate to a null reference.";
    private static readonly object[] s_emptyArguments = new object[0];

    [Conditional ("DEBUG")]
    [AssertionMethod]
    public static void DebugAssert ([AssertionCondition (AssertionConditionType.IS_TRUE)] bool assertion, string message)
    {
      IsTrue (assertion, message);
    }

    [Conditional ("DEBUG")]
    [AssertionMethod]
    [StringFormatMethod("message")]
    public static void DebugAssert ([AssertionCondition (AssertionConditionType.IS_TRUE)] bool assertion, string message, params object[] arguments)
    {
      IsTrue (assertion, message, arguments);
    }

    [Conditional ("DEBUG")]
    [AssertionMethod]
    public static void DebugAssert ([AssertionCondition (AssertionConditionType.IS_TRUE)] bool assertion)
    {
      IsTrue (assertion);
    }

    [Conditional ("TRACE")]
    [AssertionMethod]
    public static void TraceAssert ([AssertionCondition (AssertionConditionType.IS_TRUE)] bool assertion, string message)
    {
      IsTrue (assertion, message);
    }

    [Conditional ("TRACE")]
    [AssertionMethod]
    [StringFormatMethod ("message")]
    public static void TraceAssert ([AssertionCondition (AssertionConditionType.IS_TRUE)] bool assertion, string message, params object[] arguments)
    {
      IsTrue (assertion, message, arguments);
    }

    [Conditional ("TRACE")]
    [AssertionMethod]
    public static void TraceAssert ([AssertionCondition (AssertionConditionType.IS_TRUE)] bool assertion)
    {
      IsTrue (assertion);
    }

    [AssertionMethod]
    public static void IsTrue ([AssertionCondition (AssertionConditionType.IS_TRUE)] bool assertion, string message)
    {
      IsTrue (assertion, message, s_emptyArguments);
    }

    [AssertionMethod]
    [StringFormatMethod("message")]
    public static void IsTrue ([AssertionCondition (AssertionConditionType.IS_TRUE)] bool assertion, string message, params object[] arguments)
    {
      if (!assertion)
        throw new InvalidOperationException (string.Format (message, arguments));
    }

    [AssertionMethod]
    public static void IsTrue ([AssertionCondition (AssertionConditionType.IS_TRUE)] bool assertion)
    {
      IsTrue (assertion, c_msgIsFalse);
    }

    [AssertionMethod]
    public static void IsFalse ([AssertionCondition (AssertionConditionType.IS_FALSE)] bool expression, string message)
    {
      IsFalse (expression, message, s_emptyArguments);
    }

    [AssertionMethod]
    public static void IsFalse ([AssertionCondition (AssertionConditionType.IS_FALSE)] bool expression)
    {
      IsFalse (expression, c_msgIsTrue);
    }

    [AssertionMethod]
    [StringFormatMethod ("message")]
    public static void IsFalse ([AssertionCondition (AssertionConditionType.IS_FALSE)] bool expression, string message, params object[] arguments)
    {
      if (expression)
        throw new InvalidOperationException (string.Format (message, arguments));
    }

    [AssertionMethod]
    public static T IsNotNull<T> ([AssertionCondition (AssertionConditionType.IS_NOT_NULL)] T obj, string message)
    {
      return IsNotNull (obj, message, s_emptyArguments);
    }

    [AssertionMethod]
    public static T IsNotNull<T> ([AssertionCondition (AssertionConditionType.IS_NOT_NULL)] T obj)
    {
      return IsNotNull (obj, c_msgIsNull);
    }

    [AssertionMethod]
    [StringFormatMethod ("message")]
    public static T IsNotNull<T> ([AssertionCondition (AssertionConditionType.IS_NOT_NULL)] T obj, string message, params object[] arguments)
    {
      // ReSharper disable CompareNonConstrainedGenericWithNull
      if (obj == null)
        // ReSharper restore CompareNonConstrainedGenericWithNull
        throw new InvalidOperationException (string.Format (message, arguments));

      return obj;
    }

    [AssertionMethod]
    public static void IsNull ([AssertionCondition (AssertionConditionType.IS_NULL)] object obj, string message)
    {
      IsNull (obj, message, s_emptyArguments);
    }

    [AssertionMethod]
    public static void IsNull ([AssertionCondition (AssertionConditionType.IS_NULL)] object obj)
    {
      IsNull (obj, c_msgIsNotNull);
    }

    [AssertionMethod]
    [StringFormatMethod("message")]
    public static void IsNull ([AssertionCondition (AssertionConditionType.IS_NULL)] object obj, string message, params object[] arguments)
    {
      if (obj != null)
        throw new InvalidOperationException (string.Format (message, arguments));
    }
  }
}

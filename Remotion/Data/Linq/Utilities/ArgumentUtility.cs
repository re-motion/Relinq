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
using JetBrains.Annotations;

namespace Remotion.Data.Linq.Utilities
{
  /// <summary>
  /// This utility class provides methods for checking arguments.
  /// </summary>
  /// <remarks>
  /// Some methods of this class return the value of the parameter. In some cases, this is useful because the value will be converted to another 
  /// type:
  /// <code><![CDATA[
  /// void foo (object o) 
  /// {
  ///   int i = ArgumentUtility.CheckNotNullAndType<int> ("o", o);
  /// }
  /// ]]></code>
  /// In some other cases, the input value is returned unmodified. This makes it easier to use the argument checks in calls to base class constructors
  /// or property setters:
  /// <code><![CDATA[
  /// class MyType : MyBaseType
  /// {
  ///   public MyType (string name) : base (ArgumentUtility.CheckNotNullOrEmpty ("name", name))
  ///   {
  ///   }
  /// 
  ///   public override Name
  ///   {
  ///     set { base.Name = ArgumentUtility.CheckNotNullOrEmpty ("value", value); }
  ///   }
  /// }
  /// ]]></code>
  /// </remarks>
  public static class ArgumentUtility
  {
    // Copied from Remotion.Data.Linq.Utilities.ArgumentUtility
    [AssertionMethod]
    public static T CheckNotNull<T> ([InvokerParameterName] string argumentName, [AssertionCondition (AssertionConditionType.IS_NOT_NULL)] T actualValue)
    {
      // ReSharper disable CompareNonConstrainedGenericWithNull
      if (actualValue == null)
          // ReSharper restore CompareNonConstrainedGenericWithNull
        throw new ArgumentNullException (argumentName);

      return actualValue;
    }

    // Copied from Remotion.Data.Linq.Utilities.ArgumentUtility
    [AssertionMethod]
    public static string CheckNotNullOrEmpty ([InvokerParameterName] string argumentName, [AssertionCondition (AssertionConditionType.IS_NOT_NULL)] string actualValue)
    {
      CheckNotNull (argumentName, actualValue);
      if (actualValue.Length == 0)
        throw new ArgumentEmptyException (argumentName);

      return actualValue;
    }

    /// <summary>Returns the value itself if it is not <see langword="null"/> and of the specified value type.</summary>
    /// <typeparam name="TExpected"> The type that <paramref name="actualValue"/> must have. </typeparam>
    /// <exception cref="ArgumentNullException">The <paramref name="actualValue"/> is a <see langword="null"/>.</exception>
    /// <exception cref="ArgumentTypeException">The <paramref name="actualValue"/> is an instance of another type.</exception>
    // Copied from Remotion.Data.Linq.Utilities.ArgumentUtility
    [AssertionMethod]
    public static TExpected CheckNotNullAndType<TExpected> ([InvokerParameterName] string argumentName, [AssertionCondition (AssertionConditionType.IS_NOT_NULL)] object actualValue)
        // where TExpected: struct
    {
      if (actualValue == null)
        throw new ArgumentNullException (argumentName);

      if (! (actualValue is TExpected))
        throw new ArgumentTypeException (argumentName, typeof (TExpected), actualValue.GetType());
      return (TExpected) actualValue;
    }

    /// <summary>Checks whether <paramref name="actualType"/> can be assigned to <paramref name="expectedType"/>.</summary>
    /// <exception cref="ArgumentTypeException">The <paramref name="actualType"/> cannot be assigned to <paramref name="expectedType"/>.</exception>
    // Copied from Remotion.Data.Linq.Utilities.ArgumentUtility
    [AssertionMethod]
    public static Type CheckTypeIsAssignableFrom ([InvokerParameterName] string argumentName, Type actualType, [AssertionCondition (AssertionConditionType.IS_NOT_NULL)] Type expectedType)
    {
      CheckNotNull ("expectedType", expectedType);
      if (actualType != null)
      {
        if (!expectedType.IsAssignableFrom (actualType))
        {
          string message = string.Format ("Argument {0} is a {2}, which cannot be assigned to type {1}.", argumentName, expectedType, actualType);
          throw new ArgumentTypeException (message, argumentName, expectedType, actualType);
        }
      }

      return actualType;
    }

    [AssertionMethod]
    public static T CheckNotEmpty<T> ([InvokerParameterName] string argumentName, T enumerable)
        where T : IEnumerable
    {
      // ReSharper disable CompareNonConstrainedGenericWithNull
      if (enumerable != null)
      // ReSharper restore CompareNonConstrainedGenericWithNull
      {
        var collection = enumerable as ICollection;
        if (collection != null)
        {
          if (collection.Count == 0)
            throw new ArgumentEmptyException (argumentName);
          else
            return enumerable;
        }

        IEnumerator enumerator = enumerable.GetEnumerator ();
        var disposableEnumerator = enumerator as IDisposable;
        using (disposableEnumerator) // using (null) is allowed in C#
        {
          if (!enumerator.MoveNext ())
            throw new ArgumentEmptyException (argumentName);
        }
      }

      return enumerable;
    }
  }
}

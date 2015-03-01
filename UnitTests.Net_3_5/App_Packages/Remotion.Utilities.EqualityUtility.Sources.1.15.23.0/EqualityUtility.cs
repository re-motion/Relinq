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

// ReSharper disable once CheckNamespace
namespace Remotion.Utilities
{
  /// <summary>
  /// Provides methods for determining equality and hash codes.
  /// </summary>
  static partial class EqualityUtility
  {
    /// <summary>
    /// Gets an object's hash code or null, if the object is <see langword="null"/>.
    /// </summary>
    public static int SafeGetHashCode<T> (T obj)
    {
      return (obj == null) ? 0 : obj.GetHashCode ();
    }

    /// <summary>
    ///   Gets the hash code of the individual arguments, XOR'd with bits rotated.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     This method XOR's the hash codes of all individual arguments in order to create a compound hash code
    ///     for the entire set of arguments. Between XOR's, the compound hash code is rotated by 11 bits in order
    ///     to better distribute hash codes of types that aggregate their hash results toward the least-significant
    ///     side of the result (small numbers, booleans).
    ///   </para>
    ///   <para>
    ///     Overloads with type arguments are identical to the object-array version, they only reduce the amount
    ///     of boxing going on (better performance).
    ///   </para>
    /// </remarks>
    public static int GetRotatedHashCode<A0, A1> (A0 a0, A1 a1)
    {
      int hc = SafeGetHashCode (a0);
      Rotate (ref hc);
      hc ^= SafeGetHashCode (a1);
      return hc;
    }

    /// <inheritdoc cref="GetRotatedHashCode{A0,A1}" />
    public static int GetRotatedHashCode<A0, A1, A2> (A0 a0, A1 a1, A2 a2)
    {
      int hc = SafeGetHashCode (a0);
      Rotate (ref hc);
      hc ^= SafeGetHashCode (a1);
      Rotate (ref hc);
      hc ^= SafeGetHashCode (a2);
      return hc;
    }

    /// <inheritdoc cref="GetRotatedHashCode{A0,A1}" />
    public static int GetRotatedHashCode<A0, A1, A2, A3> (A0 a0, A1 a1, A2 a2, A3 a3)
    {
      int hc = SafeGetHashCode (a0);
      Rotate (ref hc);
      hc ^= SafeGetHashCode (a1);
      Rotate (ref hc);
      hc ^= SafeGetHashCode (a2);
      Rotate (ref hc);
      hc ^= SafeGetHashCode (a3);
      return hc;
    }

    /// <inheritdoc cref="GetRotatedHashCode{A0,A1}" />
    public static int GetRotatedHashCode<A0, A1, A2, A3, A4> (A0 a0, A1 a1, A2 a2, A3 a3, A4 a4)
    {
      int hc = SafeGetHashCode (a0);
      Rotate (ref hc);
      hc ^= SafeGetHashCode (a1);
      Rotate (ref hc);
      hc ^= SafeGetHashCode (a2);
      Rotate (ref hc);
      hc ^= SafeGetHashCode (a3);
      Rotate (ref hc);
      hc ^= SafeGetHashCode (a4);
      return hc;
    }

    /// <inheritdoc cref="GetRotatedHashCode{A0,A1}" />
    public static int GetRotatedHashCode<A0, A1, A2, A3, A4, A5> (A0 a0, A1 a1, A2 a2, A3 a3, A4 a4, A5 a5)
    {
      int hc = SafeGetHashCode (a0);
      Rotate (ref hc);
      hc ^= SafeGetHashCode (a1);
      Rotate (ref hc);
      hc ^= SafeGetHashCode (a2);
      Rotate (ref hc);
      hc ^= SafeGetHashCode (a3);
      Rotate (ref hc);
      hc ^= SafeGetHashCode (a4);
      Rotate (ref hc);
      hc ^= SafeGetHashCode (a5);
      return hc;
    }

    /// <inheritdoc cref="GetRotatedHashCode{A0,A1}" />
    public static int GetRotatedHashCode<A0, A1, A2, A3, A4, A5, A6> (A0 a0, A1 a1, A2 a2, A3 a3, A4 a4, A5 a5, A6 a6)
    {
      int hc = SafeGetHashCode (a0);
      Rotate (ref hc);
      hc ^= SafeGetHashCode (a1);
      Rotate (ref hc);
      hc ^= SafeGetHashCode (a2);
      Rotate (ref hc);
      hc ^= SafeGetHashCode (a3);
      Rotate (ref hc);
      hc ^= SafeGetHashCode (a4);
      Rotate (ref hc);
      hc ^= SafeGetHashCode (a5);
      Rotate (ref hc);
      hc ^= SafeGetHashCode (a6);
      return hc;
    }

    /// <inheritdoc cref="GetRotatedHashCode{A0,A1}" />
    public static int GetRotatedHashCode (params object[] fields)
    {
      int hc = 0;
      for (int i = 0; i < fields.Length; ++i)
      {
        hc ^= SafeGetHashCode (fields[i]);
        Rotate (ref hc);
      }
      return hc;
    }

    /// <summary>
    /// Gets the rotated hash code for an enumeration of objects.
    /// </summary>
    /// <param name="objects">The objects whose combined hash code should be calculated.</param>
    /// <returns>The rotate-combined hash codes of the <paramref name="objects"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="objects"/> parameter was <see langword="null"/>.</exception>
    public static int GetRotatedHashCode (IEnumerable objects)
    {
      ArgumentUtility.CheckNotNull ("objects", objects);
      int hc = 0;
      foreach (object value in objects)
      {
        hc ^= SafeGetHashCode (value);
        Rotate (ref hc);
      }
      return hc;
    }

    private static void Rotate (ref int value)
    {
      const int rotateBy = 11;
      value = (value << rotateBy) ^ (value >> (32 - rotateBy));
    }

    /// <summary>
    /// Gets a hash code for the given enumerable. The hash code is calculated by combining the hash codes of the enumerated objects using the
    /// XOR operation. This is usually suboptimal to <see cref="GetRotatedHashCode(IEnumerable)"/> unless the ordering of enumerated objects
    /// should explicitly be ignored.
    /// </summary>
    /// <param name="objects">The object enumeration for which a hash code should be calculated.</param>
    /// <returns>The combined hash code of all objects in the enumeration.</returns>
    /// <remarks>For a given set of objects, this method will always return the same value, regardless of the objects' order.</remarks>
    public static int GetXorHashCode (IEnumerable objects)
    {
      int hc = 0;
      foreach (object value in objects)
        hc ^= SafeGetHashCode (value);

      return hc;
    }

    /// <summary>
    /// Returns whether two equatable objects are equal.
    /// </summary>
    /// <remarks>
    /// Similar to <see cref="Equals{T}"/>, but without any boxing (better performance). 
    /// Equatable objects implement the <see cref="IEquatable{T}"/> interface. 
    /// </remarks>
    public static bool EqualsEquatable<T> (T a, T b)
      where T : IEquatable<T>
    {
      if (a == null)
        return (b == null);
      else
        return a.Equals ((T) b);
    }

    /// <summary>
    /// Returns whether an equatable object equals another object.
    /// </summary>
    public static bool EqualsEquatable<T> (T a, object b)
      where T : class, IEquatable<T>
    {
      T other = b as T;
      if (other != null)
        return a.Equals (other);
      else
        return false;
    }

    /// <summary>
    /// Returns whether an equatable value-type object equals another object.
    /// </summary>
    public static bool EqualsEquatableValue<T> (T a, object b)
      where T : struct, IEquatable<T>
    {
      if (b is T) // ignore incorrect ReSharper warning
        return a.Equals ((T) b);
      else
        return false;
    }

    public static bool NotNullAndSameType<T> (T a, T b)
      where T: class, IEquatable<T>
    {
      ArgumentUtility.CheckNotNull ("a", a);
      return (b != null) && a.GetType() == b.GetType();
    }

    /// <summary>
    /// Returns whether two objects are equal.
    /// </summary>
    /// <remarks>
    /// Similar to <see cref="object.Equals(object,object)"/>, only with less boxing going on (better performance).
    /// </remarks>
    public static bool Equals<T> (T a, T b)
    {
      if (a == null)
        return (b == null);
      else
        return a.Equals ((object) b);
    }
   
  }
}

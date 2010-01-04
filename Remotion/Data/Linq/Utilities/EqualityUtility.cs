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

namespace Remotion.Data.Linq.Utilities
{
  /// <summary>
  /// Provides methods for determining equality and hash codes.
  /// </summary>
  public static class EqualityUtility
  {
    /// <summary>
    /// Gets an object's hash code or null, if the object is <see langword="null"/>.
    /// </summary>
    public static int SafeGetHashCode<T> (T obj)
    {
      return (obj == null) ? 0 : obj.GetHashCode ();
    }

    public static int GetRotatedHashCode<A0, A1> (A0 a0, A1 a1)
    {
      int hc = SafeGetHashCode (a0);
      Rotate (ref hc);
      hc ^= SafeGetHashCode (a1);
      return hc;
    }

    public static int GetRotatedHashCode<A0, A1, A2> (A0 a0, A1 a1, A2 a2)
    {
      int hc = SafeGetHashCode (a0);
      Rotate (ref hc);
      hc ^= SafeGetHashCode (a1);
      Rotate (ref hc);
      hc ^= SafeGetHashCode (a2);
      return hc;
    }

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

    public static int GetRotatedHashCode (params object[] fields)
    {
      int hc = 0;
      for (int i = 0; i < fields.Length; ++i)
      {
        object value = fields[i];
        if (value != null)
        {
          hc ^= value.GetHashCode ();
          Rotate (ref hc);
        }
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

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
using System.Reflection;
using Remotion.Linq.Utilities;

namespace Remotion.Linq.UnitTests.Linq.Core.TestUtilities
{
  /// <summary>
  /// Provides utility functions for accessing non-public members.
  /// </summary>
  public static class PrivateInvoke
  {
    public static object InvokeNonPublicMethod (object target, string methodName, params object[] arguments)
    {
      ArgumentUtility.CheckNotNull ("target", target);

      if (arguments == null)
        arguments = new object[] { null };

      return InvokeMember(methodName, BindingFlags.Instance | BindingFlags.NonPublic  | BindingFlags.InvokeMethod, target, target.GetType (), arguments);
    }

    public static object GetNonPublicField (object target, string fieldName)
    {
      ArgumentUtility.CheckNotNull ("target", target);

      return GetNonPublicField (target, target.GetType(), fieldName);
    }

    public static object GetNonPublicField (object target, Type declaringType, string fieldName)
    {
      ArgumentUtility.CheckNotNull ("target", target);

      return InvokeMember (fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField, target, declaringType, null);
    }

    public static void SetNonPublicField (object target, string fieldName, object value)
    {
      ArgumentUtility.CheckNotNull ("target", target);

      InvokeMember (fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.SetField, target, target.GetType (), new[] { value });
    }

    private static object InvokeMember (string memberName, BindingFlags bindingFlags, object target, Type type, object[] arguments)
    {
      try
      {
        return type.InvokeMember (memberName, bindingFlags, null, target, arguments);
      }
      catch (TargetInvocationException e)
      {
        typeof (Exception).GetMethod ("InternalPreserveStackTrace", BindingFlags.Instance | BindingFlags.NonPublic).Invoke (e.InnerException, null);
        throw e.InnerException;
      }
    }
  }
}

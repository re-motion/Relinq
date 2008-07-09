/* Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh
 *
 * This program is free software: you can redistribute it and/or modify it under 
 * the terms of the re:motion license agreement in license.txt. If you did not 
 * receive it, please visit http://www.re-motion.org/licensing.
 * 
 * Unless otherwise provided, this software is distributed on an "AS IS" basis, 
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 */

using System;
using System.Collections.Generic;

namespace Remotion.Data.Linq.QueryProviderImplementation
{
  // Standard implementation taken from http://msdn2.microsoft.com/en-us/library/bb546158(VS.90).aspx
  internal static class TypeSystem
  {
    internal static Type GetElementType (Type seqType)
    {
      Type ienum = FindIEnumerable (seqType);
      if (ienum == null)
        return seqType;
      return ienum.GetGenericArguments ()[0];
    }

    private static Type FindIEnumerable (Type seqType)
    {
      if (seqType == null || seqType == typeof (string))
        return null;

      if (seqType.IsArray)
        return typeof (IEnumerable<>).MakeGenericType (seqType.GetElementType ());

      if (seqType.IsGenericType)
      {
        foreach (Type arg in seqType.GetGenericArguments ())
        {
          Type ienum = typeof (IEnumerable<>).MakeGenericType (arg);
          if (ienum.IsAssignableFrom (seqType))
          {
            return ienum;
          }
        }
      }

      Type[] ifaces = seqType.GetInterfaces ();
      if (ifaces != null && ifaces.Length > 0)
      {
        foreach (Type iface in ifaces)
        {
          Type ienum = FindIEnumerable (iface);
          if (ienum != null)
            return ienum;
        }
      }

      if (seqType.BaseType != null && seqType.BaseType != typeof (object))
      {
        return FindIEnumerable (seqType.BaseType);
      }

      return null;
    }
  }
}

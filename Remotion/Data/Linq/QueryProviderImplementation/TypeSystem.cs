// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2008 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// version 3.0 as published by the Free Software Foundation.
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

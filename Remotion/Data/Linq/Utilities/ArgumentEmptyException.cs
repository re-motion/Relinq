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
using System.Runtime.Serialization;

namespace Remotion.Data.Linq.Utilities
{
  /// <summary>
  /// This exception is thrown if an argument is empty although it must have a content.
  /// </summary>
  [Serializable]
  public class ArgumentEmptyException : ArgumentException
  {
    public ArgumentEmptyException (string paramName)
      : base (FormatMessage (paramName), paramName)
    {
    }

    public ArgumentEmptyException (SerializationInfo info, StreamingContext context)
        : base (info, context)
    {
    }

    private static string FormatMessage (string paramName)
    {
      return string.Format ("Parameter '{0}' cannot be empty.", paramName);
    }
  }
}

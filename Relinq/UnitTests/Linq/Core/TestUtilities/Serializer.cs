// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (c) rubicon IT GmbH, www.rubicon.eu
// 
// re-linq is free software; you can redistribute it and/or modify it under 
// the terms of the GNU Lesser General Public License as published by the 
// Free Software Foundation; either version 2.1 of the License, 
// or (at your option) any later version.
// 
// re-linq is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-linq; if not, see http://www.gnu.org/licenses.
// 
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Remotion.Utilities;


namespace Remotion.Linq.UnitTests.Linq.Core.TestUtilities
{
  /// <summary>
  /// Provides quick serialization and deserialization functionality for unit tests.
  /// </summary>
  /// <remarks>The methods of this class use a <see cref="BinaryFormatter"/> for serialization.</remarks>
  public static class Serializer
  {
    public static T SerializeAndDeserialize<T> (T t)
    {
      ArgumentUtility.CheckNotNull ("t", t);
      return (T) Serializer.Deserialize (Serializer.Serialize ((object) t));
    }

    public static byte[] Serialize (object o)
    {
      ArgumentUtility.CheckNotNull ("o", o);

      using (MemoryStream stream = new MemoryStream ())
      {
        BinaryFormatter formatter = new BinaryFormatter ();
        formatter.Serialize (stream, o);
        return stream.ToArray();
      }
    }

    public static object Deserialize (byte[] bytes)
    {
      ArgumentUtility.CheckNotNull ("bytes", bytes);

      using (MemoryStream stream = new MemoryStream (bytes))
      {
        BinaryFormatter formatter = new BinaryFormatter ();
        return formatter.Deserialize (stream);
      }
    }
  }
}

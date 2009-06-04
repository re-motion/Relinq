// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
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
using Remotion.Utilities;

namespace Remotion.Data.Linq
{
  /// <summary>
  /// Acts as a base class for classes generating unique identifiers by appending numbers to a given prefix. Concrete subclasses override 
  /// <see cref="IsKnownIdentifier"/> to specify whether an identifier is unique or not. This is used whenever an identifier for a clause is 
  /// generated which should be unique for the whole query, e.g. by <see cref="QueryModel.GetUniqueIdentifier"/>.
  /// </summary>
  public abstract class UniqueIdentifierGeneratorBase
  {
    private int _identifierCounter;

    protected abstract bool IsKnownIdentifier (string identifier);

    /// <summary>
    /// Gets a unique identifier starting with the given <paramref name="prefix"/>. The identifier is generating by appending a number to the
    /// prefix so that the resulting string does not match a known identifier.
    /// </summary>
    /// <param name="prefix">The prefix to use for the identifier.</param>
    /// <returns>A unique identifier starting with <paramref name="prefix"/>.</returns>
    public string GetUniqueIdentifier (string prefix)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("prefix", prefix);

      string identifier;
      do
      {
        identifier = prefix + _identifierCounter;
        ++_identifierCounter;
      } while (IsKnownIdentifier (identifier));

      return identifier;
    }

    /// <summary>
    /// Resets the generator so that it behaves as if it was newly instantiated. This means that some identifiers might be returned that are the same
    /// as identifiers returned before the <see cref="Reset"/>.
    /// </summary>
    public virtual void Reset ()
    {
      _identifierCounter = 0;
    }
  }
}
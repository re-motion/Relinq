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
using System.Collections.Generic;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Clauses
{
  /// <summary>
  /// Maps old <see cref="IClause"/> instances to new <see cref="IClause"/> instances. This is used by <see cref="QueryModel.Clone"/>
  /// and the clauses' Clone methods in order to be able to correctly update references to old clauses to point to the new clauses.
  /// </summary>
  public class ClonedClauseMapping
  {
    private readonly Dictionary<IClause, IClause> _lookup = new Dictionary<IClause, IClause> ();

    public void AddMapping (IClause oldClause, IClause newClause)
    {
      ArgumentUtility.CheckNotNull ("oldClause", oldClause);
      ArgumentUtility.CheckNotNull ("newClause", newClause);

      try
      {
        _lookup.Add (oldClause, newClause);
      }
      catch (ArgumentException)
      {
        throw new InvalidOperationException ("Clause has already been associated with a new clause.");
      }
    }

    public IClause GetClause (IClause oldClause)
    {
      ArgumentUtility.CheckNotNull ("oldClause", oldClause);
      try
      {
        return _lookup[oldClause];
      }
      catch (KeyNotFoundException)
      {
        throw new KeyNotFoundException ("Clause has not been associated with a new clause.");
      }
    }
  }
}
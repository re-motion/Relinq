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
using System.Collections.Generic;

namespace Remotion.Data.Linq.Clauses
{
  public static class ClauseFinder
  {
    public static T FindClause<T> (IClause startingPoint) where T : class, IClause
    {
      
      IClause currentClause = startingPoint;
      while (currentClause != null && !(currentClause is T))
        currentClause = currentClause.PreviousClause;
      return (T) currentClause;
    }


    public static IEnumerable<T> FindClauses<T>(IClause startingPoint) where T : class, IClause
    {
      IClause currentClause = startingPoint;
      while (currentClause != null)
      {
        T currentClauseAsT = currentClause as T;
        if (currentClauseAsT != null)
          yield return currentClauseAsT;

        currentClause = currentClause.PreviousClause;
      }
    }

  }
}

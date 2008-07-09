/* Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh
 *
 * This program is free software: you can redistribute it and/or modify it under 
 * the terms of the re:motion license agreement in license.txt. If you did not 
 * receive it, please visit http://www.re-motion.org/licensing.
 * 
 * Unless otherwise provided, this software is distributed on an "AS IS" basis, 
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 */

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

using System;
using System.Collections.Generic;

namespace Rubicon.Data.Linq.Clauses
{
  public static class ClauseFinderHelper
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
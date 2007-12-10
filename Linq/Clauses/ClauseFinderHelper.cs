using System;

namespace Rubicon.Data.Linq.Clauses
{
  public static class ClauseFinderHelper
  {
    public static T FindClause<T> (IClause startingPoint)
    {
      IClause currentClause = startingPoint;
      while (currentClause != null && !(currentClause is T))
        currentClause = currentClause.PreviousClause;
      return (T) currentClause;
    }
  }
}
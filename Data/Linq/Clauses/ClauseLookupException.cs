using System;

namespace Remotion.Data.Linq.Clauses
{
  public class ClauseLookupException : Exception
  {
    public ClauseLookupException (string message)
        : base (message)
    {
    }
  }
}
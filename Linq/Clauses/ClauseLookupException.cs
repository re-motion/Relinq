using System;

namespace Rubicon.Data.Linq.Clauses
{
  public class ClauseLookupException : Exception
  {
    public ClauseLookupException (string message)
        : base (message)
    {
    }
  }
}
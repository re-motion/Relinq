using System;

namespace Rubicon.Data.Linq.Clauses
{
  public class FieldAccessResolveException : Exception
  {
    public FieldAccessResolveException (string message)
        : base (message)
    {
    }
  }
}
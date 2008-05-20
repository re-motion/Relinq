using System;

namespace Remotion.Data.Linq.Parsing.FieldResolving
{
  public class FieldAccessResolveException : Exception
  {
    public FieldAccessResolveException (string message)
        : base (message)
    {
    }

    public FieldAccessResolveException (string message, Exception inner)
        : base (message, inner)
    {
    }
  }
}
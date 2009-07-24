using System;
using System.Collections;
using System.Linq.Expressions;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Clauses.ResultOperators
{
  /// <summary>
  /// Holds information about an untyped sequence of items and an expression describing the structure of the items.
  /// </summary>
  public struct UntypedSequenceInfo
  {
    public UntypedSequenceInfo (IEnumerable sequence, Expression itemExpression)
        : this ()
    {
      ArgumentUtility.CheckNotNull ("sequence", sequence);
      ArgumentUtility.CheckNotNull ("itemExpression", itemExpression);

      Sequence = sequence;
      ItemExpression = itemExpression;
    }

    public IEnumerable Sequence { get; private set; }
    public Expression ItemExpression { get; private set; }
  }
}
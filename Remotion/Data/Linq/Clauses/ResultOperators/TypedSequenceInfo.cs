using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Clauses.ResultOperators
{
  /// <summary>
  /// Holds information about a typed sequence of items and an expression describing the structure of the items.
  /// </summary>
  /// <typeparam name="T">The item type of the sequence.</typeparam>
  public struct TypedSequenceInfo<T>
  {
    public TypedSequenceInfo (IEnumerable<T> sequence, Expression itemExpression)
        : this()
    {
      ArgumentUtility.CheckNotNull ("sequence", sequence);
      ArgumentUtility.CheckNotNull ("itemExpression", itemExpression);

      Sequence = sequence;
      ItemExpression = itemExpression;
    }

    public IEnumerable<T> Sequence { get; private set; }
    public Expression ItemExpression { get; private set; }
  }
}
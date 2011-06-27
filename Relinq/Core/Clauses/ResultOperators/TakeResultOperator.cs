// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// re-linq is free software; you can redistribute it and/or modify it under 
// the terms of the GNU Lesser General Public License as published by the 
// Free Software Foundation; either version 2.1 of the License, 
// or (at your option) any later version.
// 
// re-linq is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-linq; if not, see http://www.gnu.org/licenses.
// 
using System;
using System.Linq;
using System.Linq.Expressions;
using Remotion.Linq.Clauses.ExpressionTreeVisitors;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.Utilities;

namespace Remotion.Linq.Clauses.ResultOperators
{
  /// <summary>
  /// Represents taking only a specific number of items returned by a query. 
  /// This is a result operator, operating on the whole result set of a query.
  /// </summary>
  /// <example>
  /// In C#, the "Take" call in the following example corresponds to a <see cref="TakeResultOperator"/>.
  /// <code>
  /// var query = (from s in Students
  ///              select s).Take(3);
  /// </code>
  /// </example>
  public class TakeResultOperator : SequenceTypePreservingResultOperatorBase // TODO 3207
  {
    private Expression _count;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="TakeResultOperator"/>.
    /// </summary>
    /// <param name="count">The number of elements which should be returned.</param>
    public TakeResultOperator (Expression count)
    {
      ArgumentUtility.CheckNotNull ("count", count);
      Count = count;
    }

    public Expression Count
    {
      get { return _count; }
      set 
      {
        ArgumentUtility.CheckNotNull ("value", value);
        if (value.Type != typeof (int))
        {
          var message = string.Format ("The value expression returns '{0}', an expression returning 'System.Int32' was expected.", value.Type);
          throw new ArgumentException (message, "value");
        }

        _count = value; 
      }
    }

    /// <summary>
    /// Gets the constant <see cref="int"/> value of the <see cref="Count"/> property, assuming it is a <see cref="ConstantExpression"/>. If it is
    /// not, an expression is thrown.
    /// </summary>
    /// <returns>The constant <see cref="int"/> value of the <see cref="Count"/> property.</returns>
    public int GetConstantCount ()
    {
      return GetConstantValueFromExpression<int> ("count", Count);
    }

    public override ResultOperatorBase Clone (CloneContext cloneContext)
    {
      return new TakeResultOperator (Count);
    }

    public override StreamedSequence ExecuteInMemory<T> (StreamedSequence input)
    {
      var sequence = input.GetTypedSequence<T> ();
      var result = sequence.Take (GetConstantCount ());
      return new StreamedSequence (result.AsQueryable (), (StreamedSequenceInfo) GetOutputDataInfo (input.DataInfo));
    }

    public override void TransformExpressions (Func<Expression, Expression> transformation)
    {
      ArgumentUtility.CheckNotNull ("transformation", transformation);
      Count = transformation (Count);
    }

    public override string ToString ()
    {
      return "Take(" + FormattingExpressionTreeVisitor.Format (Count) + ")";
    }
  }
}

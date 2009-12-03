// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// as published by the Free Software Foundation; either version 2.1 of the 
// License, or (at your option) any later version.
// 
// re-motion is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-motion; if not, see http://www.gnu.org/licenses.
// 
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Remotion.Data.Linq.Clauses.ExpressionTreeVisitors;
using Remotion.Data.Linq.Clauses.StreamedData;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.Clauses.ResultOperators
{
  /// <summary>
  /// Represents the except part of a query. This is a result operator, operating on the whole result set of a query.
  /// </summary>
  /// <example>
  /// In C#, the "except" clause in the following example corresponds to a <see cref="ExceptResultOperator"/>.
  /// <code>
  /// var query = (from s in Students
  ///              select s).Except(students2);
  /// </code>
  /// </example>
  public class ExceptResultOperator : SequenceTypePreservingResultOperatorBase
  {
    private Expression _source2;
    
    public ExceptResultOperator (Expression source2)
    {
      ArgumentUtility.CheckNotNull ("source2", source2);
      Source2 = source2;
    }

    /// <summary>
    /// Gets or sets the second source of this result operator, that is, an enumerable containing the items removed from the input sequence.
    /// </summary>
    public Expression Source2
    {
      get { return _source2; }
      set 
      {
        ArgumentUtility.CheckNotNull ("value", value);
        ReflectionUtility.GetItemTypeOfIEnumerable (value.Type, "value"); // check that Source2 really is an IEnumerable<T>

        _source2 = value; 
      }
    }

    /// <summary>
    /// Gets the value of <see cref="Source2"/>, assuming <see cref="Source2"/> holds a <see cref="ConstantExpression"/>. If it doesn't,
    /// an exception is thrown.
    /// </summary>
    /// <returns>The constant value of <see cref="Source2"/>.</returns>
    public IEnumerable GetConstantSource2 ()
    {
      var source2AsConstantExpression = Source2 as ConstantExpression;
      if (source2AsConstantExpression != null)
      {
        return (IEnumerable) source2AsConstantExpression.Value;
      }
      else
      {
        var message = string.Format (
            "Source2 ('{0}') is no ConstantExpression, it is a {1}.",
            FormattingExpressionTreeVisitor.Format (Source2),
            Source2.GetType ().Name);
        throw new InvalidOperationException (message);
      }
    }

    public override ResultOperatorBase Clone (CloneContext cloneContext)
    {
      return new ExceptResultOperator (Source2);
    }

    public override StreamedSequence ExecuteInMemory<T> (StreamedSequence input)
    {
      var sequence = input.GetTypedSequence<T> ();
      var result = sequence.Except ((IEnumerable<T>) GetConstantSource2());
      return new StreamedSequence (result.AsQueryable (), (StreamedSequenceInfo) GetOutputDataInfo (input.DataInfo));
    }

    public override void TransformExpressions (Func<Expression, Expression> transformation)
    {
      ArgumentUtility.CheckNotNull ("transformation", transformation);
      Source2 = transformation (Source2);
    }

    public override string ToString ()
    {
      return "Except(" + FormattingExpressionTreeVisitor.Format (Source2) + ")";
    }
    
  }
}

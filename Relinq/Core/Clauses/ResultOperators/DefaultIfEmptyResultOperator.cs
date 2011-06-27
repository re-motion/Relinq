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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Remotion.Linq.Clauses.ExpressionTreeVisitors;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.Utilities;

namespace Remotion.Linq.Clauses.ResultOperators
{
  /// <summary>
  /// Represents a guard clause yielding a singleton sequence with a default value if no items are returned by a query.
  /// This is a result operator, operating on the whole result set of a query.
  /// </summary>
  /// <example>
  /// In C#, the "Defaultifempty" call in the following example corresponds to a <see cref="DefaultIfEmptyResultOperator"/>.
  /// <code>
  /// var query = (from s in Students
  ///              select s).DefaultIfEmpty ("student");
  /// </code>
  /// </example>
  public class DefaultIfEmptyResultOperator : SequenceTypePreservingResultOperatorBase
  {
    public DefaultIfEmptyResultOperator (Expression optionalDefaultValue) // TODO 3207
    {
      OptionalDefaultValue = optionalDefaultValue;
    }

    /// <summary>
    /// Gets or sets the optional default value.
    /// </summary>
    /// <value>The optional default value.</value>
    public Expression OptionalDefaultValue { get; set; }

    /// <summary>
    /// Gets the constant <see cref="object"/> value of the <see cref="OptionalDefaultValue"/> property, assuming it is a <see cref="ConstantExpression"/>. If it is
    /// not, an expression is thrown. If it is <see langword="null" />, <see langword="null" /> is returned.
    /// </summary>
    /// <returns>The constant <see cref="object"/> value of the <see cref="OptionalDefaultValue"/> property.</returns>
    public object GetConstantOptionalDefaultValue ()
    {
      if (OptionalDefaultValue == null)
        return null;

      return GetConstantValueFromExpression<object> ("default value", OptionalDefaultValue);
    }

    public override ResultOperatorBase Clone (CloneContext cloneContext)
    {
      return new DefaultIfEmptyResultOperator (OptionalDefaultValue);
    }

    public override StreamedSequence ExecuteInMemory<T> (StreamedSequence input)
    {
      var sequence = input.GetTypedSequence<T> ();
      IEnumerable<T> result = 
          OptionalDefaultValue != null ? sequence.DefaultIfEmpty ((T) GetConstantOptionalDefaultValue ()) : sequence.DefaultIfEmpty ();
      return new StreamedSequence (result.AsQueryable(), (StreamedSequenceInfo) GetOutputDataInfo (input.DataInfo));
    }

    public override void TransformExpressions (Func<Expression, Expression> transformation)
    {
      ArgumentUtility.CheckNotNull ("transformation", transformation);
      
      if (OptionalDefaultValue != null)
        OptionalDefaultValue = transformation (OptionalDefaultValue);
    }

    public override string ToString ()
    {
      if (OptionalDefaultValue == null)
        return "DefaultIfEmpty()";
      else
        return "DefaultIfEmpty(" + FormattingExpressionTreeVisitor.Format (OptionalDefaultValue) + ")";
    }
  }
}

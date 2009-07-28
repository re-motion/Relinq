// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// version 3.0 as published by the Free Software Foundation.
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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Remotion.Data.Linq.Clauses.ExpressionTreeVisitors;
using Remotion.Data.Linq.Clauses.StreamedData;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Clauses.ResultOperators
{
  /// <summary>
  /// Represents the defaultifempty part of a query. This is a result operator, operating on the whole result set of a query.
  /// </summary>
  /// <example>
  /// In C#, the "defaultifempty" clause in the following example corresponds to a <see cref="DefaultIfEmptyResultOperator"/>.
  /// <code>
  /// var query = (from s in Students
  ///              select s).DefaultIfEmpty ("student");
  /// </code>
  /// </example>
  public class DefaultIfEmptyResultOperator : SequenceTypePreservingResultOperatorBase
  {
    public DefaultIfEmptyResultOperator (Expression optionalDefaultValue)
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
      var optionalDefaultValueAsConstant = OptionalDefaultValue as ConstantExpression;
      if (optionalDefaultValueAsConstant != null)
      {
        return optionalDefaultValueAsConstant.Value;
      }
      else
      {
        var message = string.Format (
            "OptionalDefaultValue ('{0}') is no ConstantExpression, it is a {1}.",
            FormattingExpressionTreeVisitor.Format (OptionalDefaultValue),
            OptionalDefaultValue.GetType ().Name);
        throw new InvalidOperationException (message);
      }
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
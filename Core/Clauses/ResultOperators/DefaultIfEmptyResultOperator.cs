// Copyright (c) rubicon IT GmbH, www.rubicon.eu
//
// See the NOTICE file distributed with this work for additional information
// regarding copyright ownership.  rubicon licenses this file to you under 
// the Apache License, Version 2.0 (the "License"); you may not use this 
// file except in compliance with the License.  You may obtain a copy of the 
// License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, WITHOUT 
// WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.  See the 
// License for the specific language governing permissions and limitations
// under the License.
// 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.Utilities;
using Remotion.Utilities;

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
  public sealed class DefaultIfEmptyResultOperator : SequenceTypePreservingResultOperatorBase
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
    /// not, an <see cref="InvalidOperationException"/> is thrown. If it is <see langword="null" />, <see langword="null" /> is returned.
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
      return new StreamedSequence (result.AsQueryable(), GetOutputDataInfo (input.DataInfo));
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
        return "DefaultIfEmpty(" + OptionalDefaultValue.BuildString() + ")";
    }
  }
}

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
  /// Represents the removal of a given set of items from the result set of a query.
  /// This is a result operator, operating on the whole result set of a query.
  /// </summary>
  /// <example>
  /// In C#, the "Except" call in the following example corresponds to a <see cref="ExceptResultOperator"/>.
  /// <code>
  /// var query = (from s in Students
  ///              select s).Except(students2);
  /// </code>
  /// </example>
  public sealed class ExceptResultOperator : SequenceTypePreservingResultOperatorBase
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
        ReflectionUtility.CheckTypeIsClosedGenericIEnumerable (value.Type, "value");

        _source2 = value; 
      }
    }

    /// <summary>
    /// Gets the value of <see cref="Source2"/>, assuming <see cref="Source2"/> holds a <see cref="ConstantExpression"/>. If it doesn't,
    /// an <see cref="InvalidOperationException"/> is thrown.
    /// </summary>
    /// <returns>The constant value of <see cref="Source2"/>.</returns>
    public IEnumerable<T> GetConstantSource2<T> ()
    {
      return GetConstantValueFromExpression<IEnumerable<T>> ("source2", Source2);
    }

    public override ResultOperatorBase Clone (CloneContext cloneContext)
    {
      return new ExceptResultOperator (Source2);
    }

    public override StreamedSequence ExecuteInMemory<T> (StreamedSequence input)
    {
      ArgumentUtility.CheckNotNull ("input", input);

      var sequence = input.GetTypedSequence<T> ();
      var result = sequence.Except (GetConstantSource2<T>());
      return new StreamedSequence (result.AsQueryable (), GetOutputDataInfo (input.DataInfo));
    }

    public override void TransformExpressions (Func<Expression, Expression> transformation)
    {
      ArgumentUtility.CheckNotNull ("transformation", transformation);
      Source2 = transformation (Source2);
    }

    public override string ToString ()
    {
      return "Except(" + Source2.BuildString() + ")";
    }
    
  }
}

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
using System.Linq;
using System.Linq.Expressions;
using Remotion.Linq.Clauses.StreamedData;

namespace Remotion.Linq.Clauses.ResultOperators
{
  /// <summary>
  /// Represents taking only the greatest one of the items returned by a query.
  /// This is a result operator, operating on the whole result set of a query.
  /// </summary>
  /// <remarks>
  /// The semantics of "greatest" are defined by the query provider. "Max" query methods taking a selector are represented as a combination
  /// of a <see cref="SelectClause"/> and a <see cref="MaxResultOperator"/>.
  /// </remarks>
  /// <example>
  /// In C#, the "Max" call in the following example corresponds to a <see cref="MaxResultOperator"/>.
  /// <code>
  /// var query = (from s in Students
  ///              select s.ID).Max();
  /// </code>
  /// </example>
  public sealed class MaxResultOperator : ChoiceResultOperatorBase
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="MaxResultOperator"/>.
    /// </summary>
    public MaxResultOperator ()
      : base (false)
    {
    }

    public override ResultOperatorBase Clone (CloneContext cloneContext)
    {
      return new MaxResultOperator();
    }

    public override StreamedValue ExecuteInMemory<T> (StreamedSequence input)
    {
      var sequence = input.GetTypedSequence<T> ();
      var result = sequence.Max ();
      return new StreamedValue (result, (StreamedValueInfo) GetOutputDataInfo (input.DataInfo));
    }

    /// <inheritdoc />
    public override void TransformExpressions (Func<Expression, Expression> transformation)
    {
      //nothing to do here
    }

    public override string ToString ()
    {
      return "Max()";
    }
  }
}

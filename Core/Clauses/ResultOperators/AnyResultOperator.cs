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
using JetBrains.Annotations;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Utilities;

namespace Remotion.Linq.Clauses.ResultOperators
{
  /// <summary>
  /// Represents a check whether any items are returned by a query.
  /// This is a result operator, operating on the whole result set of a query.
  /// </summary>
  /// <remarks>
  /// "Any" query methods taking a predicate are represented as into a combination of a <see cref="WhereClause"/> and an 
  /// <see cref="AnyResultOperator"/>.
  /// </remarks>
  /// <example>
  /// In C#, the "Any" call in the following example corresponds to an <see cref="AnyResultOperator"/>.
  /// <code>
  /// var result = (from s in Students
  ///              select s).Any();
  /// </code>
  /// </example>
  public sealed class AnyResultOperator : ValueFromSequenceResultOperatorBase
  {
    /// <inheritdoc cref="ResultOperatorBase.ExecuteInMemory" />
    public override StreamedValue ExecuteInMemory<T> (StreamedSequence input)
    {
      ArgumentUtility.CheckNotNull ("input", input);

      var sequence = input.GetTypedSequence<T> ();
      var result = sequence.Any ();
      return new StreamedValue (result, GetOutputDataInfo (input.DataInfo));
    }

    /// <inheritdoc />
    public override ResultOperatorBase Clone (CloneContext cloneContext)
    {
      return new AnyResultOperator ();
    }

    /// <inheritdoc />
    public override IStreamedDataInfo GetOutputDataInfo (IStreamedDataInfo inputInfo)
    {
      var sequenceInfo = ArgumentUtility.CheckNotNullAndType<StreamedSequenceInfo> ("inputInfo", inputInfo);

      return GetOutputDataInfo (sequenceInfo);
    }

    // ReSharper disable once UnusedParameter.Local
    private StreamedValueInfo GetOutputDataInfo ([NotNull] StreamedSequenceInfo sequenceInfo)
    {
      return new StreamedScalarValueInfo (typeof (bool));
    }

    /// <inheritdoc />
    public override void TransformExpressions (Func<Expression, Expression> transformation)
    {
      //nothing to do here
    }

    /// <inheritdoc />
    public override string ToString ()
    {
      return "Any()";
    }
  }
}

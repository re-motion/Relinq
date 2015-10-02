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
using System.Reflection;
using JetBrains.Annotations;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Utilities;

namespace Remotion.Linq.Clauses.ResultOperators
{
  /// <summary>
  /// Represents calculating the sum of the items returned by a query. 
  /// This is a result operator, operating on the whole result set of a query.
  /// </summary>
  /// <example>
  /// In C#, the "Sum" call in the following example corresponds to a <see cref="SumResultOperator"/>.
  /// <code>
  /// var query = (from s in Students
  ///              select s.ID).Sum();
  /// </code>
  /// </example>
  public sealed class SumResultOperator : ValueFromSequenceResultOperatorBase
  {
    public override ResultOperatorBase Clone (CloneContext cloneContext)
    {
      return new SumResultOperator();
    }

    public override StreamedValue ExecuteInMemory<T> (StreamedSequence input)
    {
      ArgumentUtility.CheckNotNull ("input", input);

      var method = typeof (Enumerable).GetRuntimeMethod ("Sum", new[] { typeof (IEnumerable<T>) });
      if (method == null)
      {
        var message = string.Format ("Cannot calculate the sum of objects of type '{0}' in memory.", typeof (T).FullName);
        throw new NotSupportedException (message);
      }

      var result = method.Invoke (null, new[] { input.GetTypedSequence<T> () });
      return new StreamedValue (result, (StreamedValueInfo) GetOutputDataInfo (input.DataInfo));
    }

    public override IStreamedDataInfo GetOutputDataInfo (IStreamedDataInfo inputInfo)
    {
      var sequenceInfo = ArgumentUtility.CheckNotNullAndType<StreamedSequenceInfo> ("inputInfo", inputInfo);
      return GetOutputDataInfo (sequenceInfo);
    }

    private IStreamedDataInfo GetOutputDataInfo ([NotNull] StreamedSequenceInfo sequenceInfo)
    {
      return new StreamedScalarValueInfo (sequenceInfo.ResultItemType);
    }

    /// <inheritdoc />
    public override void TransformExpressions (Func<Expression, Expression> transformation)
    {
      //nothing to do here
    }

    public override string ToString ()
    {
      return "Sum()";
    }
  }
}

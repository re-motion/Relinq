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
using Remotion.Utilities;

namespace Remotion.Linq.Clauses.ResultOperators
{
  /// <summary>
  /// Represents the transformation of a sequence to a query data source. 
  /// This is a result operator, operating on the whole result set of a query.
  /// </summary>
  /// <example>
  /// In C#, the "AsQueryable" call in the following example corresponds to a <see cref="AsQueryableResultOperator"/>.
  /// <code>
  /// var query = (from s in Students
  ///              select s).AsQueryable();
  /// </code>
  /// </example>
  public sealed class AsQueryableResultOperator : SequenceTypePreservingResultOperatorBase
  {
    /// <summary>
    /// A marker interface that must be implemented by the <see cref="IQueryModelVisitor "/> if the visitor supports the <see cref="AsQueryableResultOperator"/>.
    /// </summary>
    /// <remarks>
    /// Note that the interface will become obsolete with v3.0.0. See also RMLNQ-117.
    /// </remarks>
    public interface ISupportedByIQueryModelVistor
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AsQueryableResultOperator"/>.
    /// </summary>
    public AsQueryableResultOperator ()
    {
    }

    public override ResultOperatorBase Clone (CloneContext cloneContext)
    {
      return new AsQueryableResultOperator();
    }

    public override StreamedSequence ExecuteInMemory<T> (StreamedSequence input)
    {
      var sequence = input.GetTypedSequence<T>();
      return new StreamedSequence (sequence.AsQueryable(), GetOutputDataInfo (input.DataInfo));
    }

    public override void TransformExpressions (Func<Expression, Expression> transformation)
    {
      //nothing to do here
    }

    public override string ToString ()
    {
      return "AsQueryable()";
    }
  }
}

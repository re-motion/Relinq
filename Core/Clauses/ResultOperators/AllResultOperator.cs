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
using Remotion.Linq.Clauses.ExpressionVisitors;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.Utilities;
using Remotion.Utilities;

namespace Remotion.Linq.Clauses.ResultOperators
{
  /// <summary>
  /// Represents a check whether all items returned by a query satisfy a predicate.
  /// This is a result operator, operating on the whole result set of a query.
  /// </summary>
  /// <example>
  /// In C#, the "All" call in the following example corresponds to an <see cref="AllResultOperator"/>.
  /// <code>
  /// var result = (from s in Students
  ///              select s).All();
  /// </code>
  /// </example>
  public sealed class AllResultOperator : ValueFromSequenceResultOperatorBase
  {
    private Expression _predicate;

    /// <summary>
    /// Initializes a new instance of the <see cref="AllResultOperator"/> class.
    /// </summary>
    /// <param name="predicate">The predicate to evaluate. This is a resolved version of the body of the <see cref="LambdaExpression"/> that would be 
    /// passed to <see cref="Queryable.All{TSource}"/>.</param>
    public AllResultOperator (Expression predicate)
    {
      ArgumentUtility.CheckNotNull ("predicate", predicate);
      Predicate = predicate;
    }

    /// <summary>
    /// Gets or sets the predicate to evaluate on all items in the sequence.
    /// This is a resolved version of the body of the <see cref="LambdaExpression"/> that would be 
    /// passed to <see cref="Queryable.All{TSource}"/>.
    /// </summary>
    /// <value>The predicate.</value>
    public Expression Predicate
    {
      get { return _predicate; }
      set { _predicate = ArgumentUtility.CheckNotNull ("value", value); }
    }

    /// <inheritdoc cref="ResultOperatorBase.ExecuteInMemory" />
    public override StreamedValue ExecuteInMemory<T> (StreamedSequence input)
    {
      ArgumentUtility.CheckNotNull ("input", input);

      var sequence = input.GetTypedSequence<T> ();

      var predicateLambda = ReverseResolvingExpressionVisitor.ReverseResolve (input.DataInfo.ItemExpression, Predicate);
      var predicate = (Func<T, bool>) predicateLambda.Compile ();

      var result = sequence.All (predicate);
      return new StreamedValue (result, GetOutputDataInfo (input.DataInfo));
    }

    /// <inheritdoc />
    public override ResultOperatorBase Clone (CloneContext cloneContext)
    {
      return new AllResultOperator (Predicate);
    }

    /// <inheritdoc />
    public override void TransformExpressions (Func<Expression, Expression> transformation)
    {
      ArgumentUtility.CheckNotNull ("transformation", transformation);
      Predicate = transformation (Predicate);
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
    public override string ToString ()
    {
      return "All(" + Predicate.BuildString() + ")";
    }
  }
}

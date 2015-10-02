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
using Remotion.Linq.Utilities;
using Remotion.Utilities;

namespace Remotion.Linq.Clauses.ResultOperators
{
  /// <summary>
  /// Represents taking only a specific number of items returned by a query. 
  /// This is a result operator, operating on the whole result set of a query.
  /// </summary>
  /// <example>
  /// In C#, the "Take" call in the following example corresponds to a <see cref="TakeResultOperator"/>.
  /// <code>
  /// var query = (from s in Students
  ///              select s).Take(3);
  /// </code>
  /// </example>
  public sealed class TakeResultOperator : SequenceTypePreservingResultOperatorBase
  {
    private Expression _count;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="TakeResultOperator"/>.
    /// </summary>
    /// <param name="count">The number of elements which should be returned.</param>
    public TakeResultOperator (Expression count)
    {
      ArgumentUtility.CheckNotNull ("count", count);
      Count = count;
    }

    public Expression Count
    {
      get { return _count; }
      set 
      {
        ArgumentUtility.CheckNotNull ("value", value);
        if (value.Type != typeof (int))
        {
          var message = string.Format ("The value expression returns '{0}', an expression returning 'System.Int32' was expected.", value.Type);
          throw new ArgumentException (message, "value");
        }

        _count = value; 
      }
    }

    /// <summary>
    /// Gets the constant <see cref="int"/> value of the <see cref="Count"/> property, assuming it is a <see cref="ConstantExpression"/>. If it is
    /// not, an <see cref="InvalidOperationException"/> is thrown.
    /// </summary>
    /// <returns>The constant <see cref="int"/> value of the <see cref="Count"/> property.</returns>
    public int GetConstantCount ()
    {
      return GetConstantValueFromExpression<int> ("count", Count);
    }

    public override ResultOperatorBase Clone (CloneContext cloneContext)
    {
      return new TakeResultOperator (Count);
    }

    public override StreamedSequence ExecuteInMemory<T> (StreamedSequence input)
    {
      var sequence = input.GetTypedSequence<T> ();
      var result = sequence.Take (GetConstantCount ());
      return new StreamedSequence (result.AsQueryable (), GetOutputDataInfo (input.DataInfo));
    }

    public override void TransformExpressions (Func<Expression, Expression> transformation)
    {
      ArgumentUtility.CheckNotNull ("transformation", transformation);
      Count = transformation (Count);
    }

    public override string ToString ()
    {
      return "Take(" + Count.BuildString() + ")";
    }
  }
}

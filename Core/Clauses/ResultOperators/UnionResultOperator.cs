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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.Utilities;
using Remotion.Utilities;

namespace Remotion.Linq.Clauses.ResultOperators
{
  /// <summary>
  /// Represents forming the mathematical union of  a given set of items and the items returned by a query. 
  /// This is a result operator, operating on the whole result set of a query.
  /// </summary>
  /// <example>
  /// In C#, the "Union" call in the following example corresponds to a <see cref="UnionResultOperator"/>.
  /// <code>
  /// var query = (from s in Students
  ///              select s).Union(students2);
  /// </code>
  /// </example>
  public sealed class UnionResultOperator : SequenceFromSequenceResultOperatorBase, IQuerySource
  {
    private string _itemName;
    private Type _itemType;

    private Expression _source2;

    public UnionResultOperator (string itemName, Type itemType, Expression source2)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("itemName", itemName);
      ArgumentUtility.CheckNotNull ("itemType", itemType);
      ArgumentUtility.CheckNotNull ("source2", source2);
      
      ItemName = itemName;
      ItemType = itemType;
      Source2 = source2;
    }

    public string ItemName
    {
      get { return _itemName; }
      set { _itemName = ArgumentUtility.CheckNotNullOrEmpty ("value", value); }
    }

    public Type ItemType
    {
      get { return _itemType; }
      set { _itemType = ArgumentUtility.CheckNotNull ("value", value); }
    }

    /// <summary>
    /// Gets or sets the second source of this result operator, that is, an enumerable containing the items united with the input sequence.
    /// </summary>
    public Expression Source2
    {
      get { return _source2; }
      set  { _source2 = ArgumentUtility.CheckNotNull ("value", value); }
    }

    /// <summary>
    /// Gets the value of <see cref="Source2"/>, assuming <see cref="Source2"/> holds a <see cref="ConstantExpression"/>. If it doesn't,
    /// an <see cref="InvalidOperationException"/> is thrown.
    /// </summary>
    /// <returns>The constant value of <see cref="Source2"/>.</returns>
    public IEnumerable GetConstantSource2 ()
    {
      return GetConstantValueFromExpression<IEnumerable> ("source2", Source2);
    }

    public override ResultOperatorBase Clone (CloneContext cloneContext)
    {
      return new UnionResultOperator (_itemName, _itemType, _source2);
    }

    public override StreamedSequence ExecuteInMemory<T> (StreamedSequence input)
    {
      var sequence = input.GetTypedSequence<T> ();
      var result = sequence.Union ((IEnumerable<T>) GetConstantSource2 ());
      return new StreamedSequence (result.AsQueryable (), (StreamedSequenceInfo) GetOutputDataInfo (input.DataInfo));
    }

    public override IStreamedDataInfo GetOutputDataInfo (IStreamedDataInfo inputInfo)
    {
      var sequenceInfo = ArgumentUtility.CheckNotNullAndType<StreamedSequenceInfo> ("inputInfo", inputInfo);
      CheckSequenceItemType (sequenceInfo, _itemType);
      return new StreamedSequenceInfo (typeof (IQueryable<>).MakeGenericType (_itemType), new QuerySourceReferenceExpression (this));
    }

    public override void TransformExpressions (Func<Expression, Expression> transformation)
    {
      ArgumentUtility.CheckNotNull ("transformation", transformation);
      Source2 = transformation (Source2);
    }

    public override string ToString ()
    {
      return "Union(" + Source2.BuildString() + ")";
    }
  }
}

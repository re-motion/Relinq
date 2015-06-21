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
using System.Reflection;
using JetBrains.Annotations;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.Utilities;
using Remotion.Utilities;

namespace Remotion.Linq.Clauses.ResultOperators
{
  /// <summary>
  /// Represents a check whether the results returned by a query contain a specific item.
  /// This is a result operator, operating on the whole result set of a query.
  /// </summary>
  /// <example>
  /// In C#, the "Contains" call in the following example corresponds to a <see cref="ContainsResultOperator"/>.
  /// <code>
  /// var query = (from s in Students
  ///              select s).Contains (student);
  /// </code>
  /// </example>
  public sealed class ContainsResultOperator : ValueFromSequenceResultOperatorBase
  {
    private Expression _item;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContainsResultOperator"/> class.
    /// </summary>
    /// <param name="item">The item for which to be searched.</param>
    public ContainsResultOperator (Expression item)
    {
      ArgumentUtility.CheckNotNull ("item", item);
      Item = item;
    }

    /// <summary>
    /// Gets or sets an expression yielding the item for which to be searched. This must be compatible with (ie., assignable to) the source sequence 
    /// items.
    /// </summary>
    /// <value>The item expression.</value>
    public Expression Item
    {
      get { return _item; }
      set { _item = ArgumentUtility.CheckNotNull ("value", value); }
    }

    /// <summary>
    /// Gets the constant value of the <see cref="Item"/> property, assuming it is a <see cref="ConstantExpression"/>. If it is
    /// not, an <see cref="InvalidOperationException"/> is thrown.
    /// </summary>
    /// <typeparam name="T">The expected item type. If the item is not of this type, an <see cref="InvalidOperationException"/> is thrown.</typeparam>
    /// <returns>The constant value of the <see cref="Item"/> property.</returns>
    public T GetConstantItem<T> ()
    {
      return GetConstantValueFromExpression<T> ("item", Item);
    }

    public override StreamedValue ExecuteInMemory<T> (StreamedSequence input)
    {
      ArgumentUtility.CheckNotNull ("input", input);

      var sequence = input.GetTypedSequence<T> ();
      var item = GetConstantItem<T> ();
      var result = sequence.Contains (item);
      return new StreamedValue (result, GetOutputDataInfo (input.DataInfo));
    }

    public override ResultOperatorBase Clone (CloneContext cloneContext)
    {
      return new ContainsResultOperator (Item);
    }

    public override IStreamedDataInfo GetOutputDataInfo (IStreamedDataInfo inputInfo)
    {
      var sequenceInfo = ArgumentUtility.CheckNotNullAndType<StreamedSequenceInfo> ("inputInfo", inputInfo);

      return GetOutputDataInfo (sequenceInfo);
    }

    private StreamedValueInfo GetOutputDataInfo ([NotNull] StreamedSequenceInfo inputInfo)
    {
      if (!inputInfo.ResultItemType.GetTypeInfo().IsAssignableFrom (Item.Type.GetTypeInfo()))
      {
        var message = string.Format (
            "The items of the input sequence of type '{0}' are not compatible with the item expression of type '{1}'.",
            inputInfo.ResultItemType,
            Item.Type);

        throw new ArgumentException (message, "inputInfo");
      }

      return new StreamedScalarValueInfo (typeof (bool));
    }

    public override void TransformExpressions (Func<Expression, Expression> transformation)
    {
      ArgumentUtility.CheckNotNull ("transformation", transformation);
      Item = transformation (Item);
    }

    public override string ToString ()
    {
      return "Contains(" + Item.BuildString() + ")";
    }
  }
}

// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (c) rubicon IT GmbH, www.rubicon.eu
// 
// re-linq is free software; you can redistribute it and/or modify it under 
// the terms of the GNU Lesser General Public License as published by the 
// Free Software Foundation; either version 2.1 of the License, 
// or (at your option) any later version.
// 
// re-linq is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-linq; if not, see http://www.gnu.org/licenses.
// 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Remotion.Linq.Clauses.ExpressionTreeVisitors;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.Utilities;

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
  public class ContainsResultOperator : ValueFromSequenceResultOperatorBase
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
      return new StreamedValue (result, (StreamedValueInfo) GetOutputDataInfo (input.DataInfo));
    }

    public override ResultOperatorBase Clone (CloneContext cloneContext)
    {
      return new ContainsResultOperator (Item);
    }

    public override IStreamedDataInfo GetOutputDataInfo (IStreamedDataInfo inputInfo)
    {
      var sequenceInfo = ArgumentUtility.CheckNotNullAndType<StreamedSequenceInfo> ("inputInfo", inputInfo);

      if (!sequenceInfo.ResultItemType.IsAssignableFrom (Item.Type))
      {
        var message = string.Format (
            "The items of the input sequence of type '{0}' are not compatible with the item expression of type '{1}'.",
            sequenceInfo.ResultItemType,
            Item.Type);

        throw new ArgumentTypeException (message, "inputInfo", typeof (IEnumerable<>).MakeGenericType (Item.Type), sequenceInfo.ResultItemType);
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
      return "Contains(" + FormattingExpressionTreeVisitor.Format (Item) + ")";
    }
  }
}

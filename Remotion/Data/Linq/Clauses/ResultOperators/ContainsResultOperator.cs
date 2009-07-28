// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// version 3.0 as published by the Free Software Foundation.
// 
// re-motion is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-motion; if not, see http://www.gnu.org/licenses.
// 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Remotion.Data.Linq.Clauses.ExecutionStrategies;
using Remotion.Data.Linq.Clauses.ExpressionTreeVisitors;
using Remotion.Data.Linq.Clauses.StreamedData;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Clauses.ResultOperators
{
  /// <summary>
  /// Represents the contains part of a query. This is a result operator, operating on the whole result set of a query.
  /// </summary>
  /// <example>
  /// In C#, the "contains" clause in the following example corresponds to a <see cref="ContainsResultOperator"/>.
  /// <code>
  /// var query = (from s in Students
  ///              select s).Contains(student);
  /// </code>
  /// </example>
  public class ContainsResultOperator : ValueFromSequenceResultOperatorBase
  {
    private Expression _item;

    public ContainsResultOperator (Expression item)
        : base (ScalarExecutionStrategy.Instance)
    {
      Item = item;
    }

    public Expression Item
    {
      get { return _item; }
      set { _item = ArgumentUtility.CheckNotNull ("value", value); }
    }

    /// <summary>
    /// Gets the constant <see cref="object"/> value of the <see cref="Item"/> property, assuming it is a <see cref="ConstantExpression"/>. If it is
    /// not, an <see cref="InvalidOperationException"/> is thrown.
    /// </summary>
    /// <typeparam name="T">The expected item type. If the item is not of this type, an <see cref="InvalidOperationException"/> is thrown.</typeparam>
    /// <returns>The constant <see cref="object"/> value of the <see cref="Item"/> property.</returns>
    public T GetConstantItem<T> ()
    {
      if (!typeof (T).IsAssignableFrom (Item.Type))
      {
        var message = string.Format (
            "The value stored by Item ('{0}') is not of type '{1}', it is of type '{2}'.",
            FormattingExpressionTreeVisitor.Format (Item),
            typeof (T),
            Item.Type);
        throw new InvalidOperationException (message);
      }

      var itemAsConstantExpression = Item as ConstantExpression;
      if (itemAsConstantExpression != null)
      {
        return (T) itemAsConstantExpression.Value;
      }
      else
      {
        var message = string.Format (
            "Item ('{0}') is no ConstantExpression, it is a {1}.",
            FormattingExpressionTreeVisitor.Format (Item),
            Item.GetType ().Name);
        throw new InvalidOperationException (message);
      }
    }

    public override StreamedValue ExecuteInMemory<T> (StreamedSequence input)
    {
      ArgumentUtility.CheckNotNull ("input", input);

      var sequence = input.GetTypedSequence<T> ();
      var item = GetConstantItem<T> ();
      return new StreamedValue (sequence.Contains (item));
    }

    public override ResultOperatorBase Clone (CloneContext cloneContext)
    {
      return new ContainsResultOperator (Item);
    }

    public override IStreamedDataInfo GetOutputDataInfo (IStreamedDataInfo inputInfo)
    {
      var sequenceInfo = ArgumentUtility.CheckNotNullAndType<StreamedSequenceInfo> ("inputInfo", inputInfo);
      if (sequenceInfo.ItemExpression.Type != Item.Type)
      {
        var message = string.Format (
            "The input sequence must have items of type '{0}', but it has items of type '{1}'.",
            Item.Type, 
            sequenceInfo.ItemExpression.Type);

        throw new ArgumentTypeException (message, "inputInfo", typeof (IEnumerable<>).MakeGenericType (Item.Type), sequenceInfo.ItemExpression.Type);
      }

      return new StreamedValueInfo (typeof (bool));
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
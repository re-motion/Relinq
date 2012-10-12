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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Remotion.Linq.Clauses.ExpressionTreeVisitors;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.Utilities;

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
  public class UnionResultOperator : SequenceFromSequenceResultOperatorBase, IQuerySource
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
    /// an Exception is thrown.
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
      return "Union(" + FormattingExpressionTreeVisitor.Format (Source2) + ")";
    }
  }
}

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
using System.Linq;
using System.Linq.Expressions;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.Utilities;

namespace Remotion.Linq.Clauses.ResultOperators
{
  /// <summary>
  /// Represents a cast of the items returned by a query to a different type.
  /// This is a result operator, operating on the whole result set of a query.
  /// </summary>
  /// <example>
  /// In C#, "Cast" call in the following example corresponds to a <see cref="CastResultOperator"/>.
  /// <code>
  /// var query = (from s in Students
  ///              select s.ID).Cast&lt;int&gt;();
  /// </code>
  /// </example>
  public class CastResultOperator : SequenceFromSequenceResultOperatorBase
  {
    private Type _castItemType;

    public CastResultOperator (Type castItemType)
    {
      ArgumentUtility.CheckNotNull ("castItemType", castItemType);
      CastItemType = castItemType;
    }

    public Type CastItemType
    {
      get { return _castItemType; }
      set
      {
        ArgumentUtility.CheckNotNull ("value", value);
        _castItemType = value;
      }
    }

    public override ResultOperatorBase Clone (CloneContext cloneContext)
    {
      return new CastResultOperator (CastItemType);
    }
    
    public override StreamedSequence ExecuteInMemory<TInput> (StreamedSequence input)
    {
      ArgumentUtility.CheckNotNull ("input", input);

      var sequence = input.GetTypedSequence<TInput> ();
      var castMethod = typeof (Enumerable).GetMethod ("Cast", new[] { typeof (IEnumerable) }).MakeGenericMethod (CastItemType);
      var result = (IEnumerable) InvokeExecuteMethod (castMethod, sequence);
      return new StreamedSequence (result.AsQueryable(), (StreamedSequenceInfo) GetOutputDataInfo (input.DataInfo));
    }

    public override IStreamedDataInfo GetOutputDataInfo (IStreamedDataInfo inputInfo)
    {
      var sequenceInfo = ArgumentUtility.CheckNotNullAndType<StreamedSequenceInfo> ("inputInfo", inputInfo);
      return new StreamedSequenceInfo (
          typeof (IQueryable<>).MakeGenericType (CastItemType), 
          GetNewItemExpression (sequenceInfo.ItemExpression));
    }

    /// <inheritdoc />
    public override void TransformExpressions (Func<Expression, Expression> transformation)
    {
      //nothing to do here
    }

    public override string ToString ()
    {
      return "Cast<" + CastItemType + ">()";
    }

    private UnaryExpression GetNewItemExpression (Expression inputItemExpression)
    {
      return Expression.Convert (inputItemExpression, CastItemType);
    }
  }
}

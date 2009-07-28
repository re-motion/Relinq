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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Remotion.Data.Linq.Clauses.StreamedData;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Clauses.ResultOperators
{
  /// <summary>
  /// Represents the "Cast" part of a query. This is a result operator, operating on the whole result set of a query.
  /// </summary>
  /// <example>
  /// In C#, the "Cast" clause in the following example corresponds to a <see cref="CastResultOperator"/>.
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

    public override string ToString ()
    {
      return "Cast()";
    }

    private UnaryExpression GetNewItemExpression (Expression inputItemExpression)
    {
      return Expression.Convert (inputItemExpression, CastItemType);
    }
  }
}
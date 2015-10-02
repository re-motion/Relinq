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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.Utilities;
using Remotion.Utilities;

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
  public sealed class CastResultOperator : SequenceFromSequenceResultOperatorBase
  {
    private static readonly MethodInfo s_enumerableCastMethod = 
        typeof (Enumerable).GetRuntimeMethodChecked ("Cast", new[] { typeof (IEnumerable) });

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
      var castMethod = s_enumerableCastMethod.MakeGenericMethod (CastItemType);
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

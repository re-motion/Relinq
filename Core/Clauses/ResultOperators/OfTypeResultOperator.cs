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
  /// Represents filtering the items returned by a query to only return those items that are of a specific type. 
  /// This is a result operator, operating on the whole result set of a query.
  /// </summary>
  /// <example>
  /// In C#, the "OfType" call in the following example corresponds to a <see cref="OfTypeResultOperator"/>.
  /// <code>
  /// var query = (from s in Students
  ///              select s.ID).OfType&lt;int&gt;();
  /// </code>
  /// </example>
  public sealed class OfTypeResultOperator : SequenceFromSequenceResultOperatorBase
  {
    private static readonly MethodInfo s_enumerableOfTypeMethod = 
        typeof (Enumerable).GetRuntimeMethodChecked ("OfType", new[] { typeof (IEnumerable) });

    private Type _searchedItemType;

    public OfTypeResultOperator (Type searchedItemType)
    {
      ArgumentUtility.CheckNotNull ("searchedItemType", searchedItemType);
      SearchedItemType = searchedItemType;
    }

    public Type SearchedItemType
    {
      get { return _searchedItemType; }
      set
      {
        ArgumentUtility.CheckNotNull ("value", value);
        _searchedItemType = value;
      }
    }

    public override ResultOperatorBase Clone (CloneContext cloneContext)
    {
      return new OfTypeResultOperator (SearchedItemType);
    }

    public override StreamedSequence ExecuteInMemory<TInput> (StreamedSequence input)
    {
      var sequence = input.GetTypedSequence<TInput> ();
      var castMethod = s_enumerableOfTypeMethod.MakeGenericMethod (SearchedItemType);
      var result = (IEnumerable) InvokeExecuteMethod (castMethod, sequence);
      return new StreamedSequence (result.AsQueryable(), (StreamedSequenceInfo) GetOutputDataInfo (input.DataInfo));
    }

    public override IStreamedDataInfo GetOutputDataInfo (IStreamedDataInfo inputInfo)
    {
      var sequenceInfo = ArgumentUtility.CheckNotNullAndType<StreamedSequenceInfo> ("inputInfo", inputInfo);
      return new StreamedSequenceInfo (
          typeof (IQueryable<>).MakeGenericType (SearchedItemType),
          GetNewItemExpression (sequenceInfo.ItemExpression));
    }

    /// <inheritdoc />
    public override void TransformExpressions (Func<Expression, Expression> transformation)
    {
      //nothing to do here
    }

    public override string ToString ()
    {
      return "OfType<" + SearchedItemType + ">()";
    }

    private UnaryExpression GetNewItemExpression (Expression inputItemExpression)
    {
      return Expression.Convert (inputItemExpression, SearchedItemType);
    }
   
  }
}

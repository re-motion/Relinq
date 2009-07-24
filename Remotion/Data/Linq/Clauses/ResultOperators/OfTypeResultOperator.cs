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
using System.Linq;
using System.Linq.Expressions;
using Remotion.Data.Linq.Clauses.ExecutionStrategies;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Clauses.ResultOperators
{
  /// <summary>
  /// Represents the "OfType" part of a query. This is a result operator, operating on the whole result set of a query.
  /// </summary>
  /// <example>
  /// In C#, the "OfType" clause in the following example corresponds to a <see cref="OfTypeResultOperator"/>.
  /// <code>
  /// var query = (from s in Students
  ///              select s.ID).OfType&lt;int&gt;();
  /// </code>
  /// </example>
  public class OfTypeResultOperator: ResultOperatorBase
  {
    private Type _searchedItemType;

    public OfTypeResultOperator (Type searchedItemType)
        : base (CollectionExecutionStrategy.Instance)
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

    public override IExecuteInMemoryData ExecuteInMemory (IExecuteInMemoryData input)
    {
      ArgumentUtility.CheckNotNull ("input", input);
      return InvokeGenericExecuteMethod<ExecuteInMemorySequenceData, ExecuteInMemorySequenceData> (input, ExecuteInMemory<object>);
    }

    public ExecuteInMemorySequenceData ExecuteInMemory<TInput> (ExecuteInMemorySequenceData input)
    {
      var sequence = input.GetCurrentSequence<TInput> ();
      var castMethod = typeof (Enumerable).GetMethod ("OfType", new[] { typeof (IEnumerable) }).MakeGenericMethod (SearchedItemType);
      var result = InvokeExecuteMethod (castMethod, sequence.A);
      var resultItemExpression = Expression.Convert (sequence.B, SearchedItemType);
      return new ExecuteInMemorySequenceData (result, resultItemExpression);
    }

    public override Type GetResultType (Type inputResultType)
    {
      ArgumentUtility.CheckNotNull ("inputResultType", inputResultType);
      ReflectionUtility.GetItemTypeOfIEnumerable (inputResultType, "inputResultType"); // check whether inputResultType implements IEnumerable<T>

      return typeof (IQueryable<>).MakeGenericType (SearchedItemType);
    }

    public override string ToString ()
    {
      return "OfType()";
    }
    
  }
}
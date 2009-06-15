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
using System.Linq.Expressions;
using Remotion.Data.Linq.EagerFetching;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Clauses.ExecutionStrategies
{
  /// <summary>
  /// Represents a call to <see cref="IQueryExecutor.ExecuteCollection2{T}"/>. The generic TResult argument of the 
  /// <see cref="GetExecutionExpression{TResult}"/> method is a specialization of <see cref="IEnumerable{T}"/>.
  /// <seealso cref="IExecutionStrategy"/>
  /// </summary>
  public class CollectionExecutionStrategy : IExecutionStrategy
  {
    public static readonly CollectionExecutionStrategy Instance = new CollectionExecutionStrategy ();

    private CollectionExecutionStrategy ()
    {
    }

    public Expression<Func<IQueryExecutor, TResult>> GetExecutionExpression<TResult> (QueryModel queryModel, FetchRequestBase[] fetchRequests)
    {
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);
      ArgumentUtility.CheckNotNull ("fetchRequests", fetchRequests);

      var executeCollectionMethod = typeof (IQueryExecutor).GetMethod ("ExecuteCollection2");
      var itemType = ReflectionUtility.GetAscribedGenericArguments (typeof (TResult), typeof (IEnumerable<>))[0];

      var executorParameter = Expression.Parameter (typeof (IQueryExecutor), "queryExecutor");
      return Expression.Lambda<Func<IQueryExecutor, TResult>> (
          Expression.Call (executorParameter, executeCollectionMethod.MakeGenericMethod (itemType), Expression.Constant (queryModel), Expression.Constant (fetchRequests)),
          executorParameter);
    }
  }
}
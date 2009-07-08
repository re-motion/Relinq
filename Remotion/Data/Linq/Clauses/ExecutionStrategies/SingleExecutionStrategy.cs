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
using System.Linq;
using System.Linq.Expressions;
using Remotion.Data.Linq.EagerFetching;
using Remotion.Data.Linq.Parsing;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Clauses.ExecutionStrategies
{
  /// <summary>
  /// Represents a call to <see cref="IQueryExecutor.ExecuteCollection{T}"/> that returns at most or exactly one item. 
  /// The generic TResult argument of the <see cref="GetExecutionExpression{TResult}"/> method defines the item type to be returned.
  /// <seealso cref="IExecutionStrategy"/>
  /// </summary>
  public class SingleExecutionStrategy : IExecutionStrategy
  {
    public static readonly SingleExecutionStrategy InstanceWithDefaultWhenEmpty = new SingleExecutionStrategy (true);
    public static readonly SingleExecutionStrategy InstanceNoDefaultWhenEmpty = new SingleExecutionStrategy (false);

    private readonly bool _returnDefaultWhenEmpty;

    private SingleExecutionStrategy (bool returnDefaultWhenEmpty)
    {
      _returnDefaultWhenEmpty = returnDefaultWhenEmpty;
    }

    public Expression<Func<IQueryExecutor, TResult>> GetExecutionExpression<TResult> (QueryModel queryModel, FetchRequestBase[] fetchRequests)
    {
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);
      ArgumentUtility.CheckNotNull ("fetchRequests", fetchRequests);

      var executeCollectionMethod = typeof (IQueryExecutor).GetMethod ("ExecuteCollection");
      var singleMethod = ParserUtility.GetMethod (() => ((Enumerable.Single<TResult> (null))));
      var singleOrDefaultMethod = ParserUtility.GetMethod (() => ((Enumerable.SingleOrDefault<TResult> (null))));
      var singleMethodToUse = _returnDefaultWhenEmpty ? singleOrDefaultMethod : singleMethod;

      var executorParameter = Expression.Parameter (typeof (IQueryExecutor), "queryExecutor");
      var collectionMethodCall =
          Expression.Call (
              executorParameter,
              executeCollectionMethod.MakeGenericMethod (typeof (TResult)),
              Expression.Constant (queryModel),
              Expression.Constant (fetchRequests));

      var singleMethodCall = Expression.Call (singleMethodToUse, collectionMethodCall);
      return Expression.Lambda<Func<IQueryExecutor, TResult>> (singleMethodCall, executorParameter);
    }
  }
}
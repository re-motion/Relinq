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
using System.Linq.Expressions;
using Remotion.Data.Linq.Clauses.ExecutionStrategies;
using Remotion.Data.Linq.EagerFetching;

namespace Remotion.Data.Linq.Clauses
{
  /// <summary>
  /// Defines a strategy to execute a <see cref="QueryModel"/>. This is used by <see cref="QueryProviderBase"/> to dispatch a call to
  /// <see cref="QueryProviderBase.Execute{TResult}"/> to a specific <see cref="IQueryExecutor"/> method.
  /// Possible strategies are <see cref="ScalarExecutionStrategy"/>, <see cref="SingleExecutionStrategy"/>, and <see cref="CollectionExecutionStrategy"/>.
  /// </summary>
  public interface IExecutionStrategy
  {
    /// <summary>
    /// Gets a <see cref="LambdaExpression"/> that can be used to execute the query defined by <paramref name="queryModel"/> together with the given
    /// <paramref name="fetchRequests"/>. When compiled and executed, the returned expression will call one of <see cref="IQueryExecutor"/>'s methods
    /// with the correct type parameters.
    /// </summary>
    Expression<Func<IQueryExecutor, TResult>> GetExecutionExpression<TResult> (QueryModel queryModel, FetchRequestBase[] fetchRequests);
  }
}
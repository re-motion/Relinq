// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2008 rubicon informationstechnologie gmbh, www.rubicon.eu
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
using System.Reflection;
using Remotion.Data.Linq.EagerFetching;
using Remotion.Data.Linq.Parsing.Structure;
using Remotion.Utilities;

namespace Remotion.Data.Linq
{
  /// <summary>
  /// The class implements <see cref="IQueryProvider"/> to create and execute queries against a datasource.
  /// </summary>
  public abstract class QueryProviderBase : IQueryProvider
  {
    /// <summary>
    /// Initializes a new instance of  <see cref="QueryProviderBase"/> 
    /// </summary>
    /// <param name="executor">The executor is used to execute queries against the backend.</param>
    protected QueryProviderBase (IQueryExecutor executor)
    {
      ArgumentUtility.CheckNotNull ("executor", executor);
      Executor = executor;
    }

    public IQueryExecutor Executor { get; private set; }


    public IQueryable CreateQuery (Expression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      MethodInfo genericCreateQueryMethod = typeof (QueryProviderBase).GetMethod ("CreateQueryable", BindingFlags.NonPublic | BindingFlags.Instance);

      Type elementType = ReflectionUtility.GetAscribedGenericArguments (expression.Type, typeof (IEnumerable<>))[0];
      try
      {
        return (IQueryable) genericCreateQueryMethod.MakeGenericMethod (elementType).Invoke(this, new object[] {expression});
      }
      catch (TargetInvocationException tie)
      {
        throw tie.InnerException;
      }
    }

    /// <summary>
    /// Queryable's collection-returning standard query operators call this method.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="expression">The query as expression chain.</param>
    /// <returns><see cref="IQueryable{T}"/></returns>
    public IQueryable<T> CreateQuery<T> (Expression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      return CreateQueryable<T> (expression);
    }

    protected abstract IQueryable<T> CreateQueryable<T> (Expression expression);

    public virtual TResult Execute<TResult> (Expression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      return (TResult) Execute (expression);
    }

    public virtual object Execute (Expression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var fetchRequests = GetFetchRequests (ref expression);
      var queryModel = GenerateQueryModel (expression);
      return Executor.ExecuteSingle (queryModel, fetchRequests);
    }

    /// <summary>
    /// This is where the query is executed and the results are mapped to objects.
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="expression">The query as expression chain.</param>
    /// <returns></returns>
    public virtual IEnumerable<TResult> ExecuteCollection<TResult> (Expression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      return ExecuteCollection (expression).Cast<TResult> ();
    }

    /// <summary>
    /// This is where the query is executed and the results are mapped to objects.
    /// </summary>
    /// <param name="expression">The query as expression chain.</param>
    /// <returns></returns>
    public virtual IEnumerable ExecuteCollection (Expression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var fetchRequests = GetFetchRequests (ref expression);
      var queryModel = GenerateQueryModel (expression);
      return Executor.ExecuteCollection (queryModel, fetchRequests);
    }

    /// <summary>
    /// Gets all the fetch requests embedded in a query's <see cref="Expression"/> tree as a hierarchical set of <see cref="FetchRequest"/> objects.
    /// </summary>
    /// <param name="expression">The expression tree to search for fetch requests. If any is found, the parameter returns a new 
    /// <see cref="Expression"/> instance with all fetch expressions removed from the expression tree.</param>
    /// <returns>An array of <see cref="FetchRequest"/> objects that hold the top-level fetch requests for the expression tree.</returns>
    public FetchRequest[] GetFetchRequests (ref Expression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var fetchFilteringVisitor = new FetchFilteringExpressionTreeVisitor ();
      var result = fetchFilteringVisitor.Visit (expression);
      expression = result.NewExpression;
      return result.FetchRequests.ToArray();
    }

    /// <summary>
    /// The method generates a <see cref="QueryModel"/>.
    /// </summary>
    /// <param name="expression">The query as expression chain.</param>
    /// <returns>a <see cref="QueryModel"/></returns>
    public QueryModel GenerateQueryModel (Expression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var parser = new QueryParser(expression);
      return parser.GetParsedQuery();
    }
    

  }
}

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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Data.Linq.EagerFetching;
using Remotion.Data.Linq.Parsing.Structure;
using Remotion.Utilities;

namespace Remotion.Data.Linq
{
  /// <summary>
  /// Provides a default implementation of <see cref="IQueryProvider"/> that executes queries (subclasses of <see cref="QueryableBase{T}"/>) by
  /// first parsing them into a <see cref="QueryModel"/> and then passing that to a given implementation of <see cref="IQueryExecutor"/>.
  /// </summary>
  public abstract class QueryProviderBase : IQueryProvider
  {
    private readonly ExpressionTreeParser _expressionTreeParser;

    /// <summary>
    /// Initializes a new instance of <see cref="QueryProviderBase"/> using the default <see cref="MethodCallExpressionNodeTypeRegistry"/>.
    /// </summary>
    /// <param name="executor">The <see cref="IQueryExecutor"/> used to execute queries against a specific query backend.</param>
    protected QueryProviderBase (IQueryExecutor executor)
        : this (executor, MethodCallExpressionNodeTypeRegistry.CreateDefault ())
    {
      ArgumentUtility.CheckNotNull ("executor", executor);
      Executor = executor;
      _expressionTreeParser = new ExpressionTreeParser (MethodCallExpressionNodeTypeRegistry.CreateDefault ());
    }

    /// <summary>
    /// Initializes a new instance of <see cref="QueryProviderBase"/> using a custom <see cref="MethodCallExpressionNodeTypeRegistry"/>.
    /// </summary>
    /// <param name="executor">The <see cref="IQueryExecutor"/> used to execute queries against a specific query backend.</param>
    /// <param name="nodeTypeRegistry">The <see cref="MethodCallExpressionNodeTypeRegistry"/> containing the <see cref="MethodCallExpression"/>
    /// parsers that should be used when parsing queries.</param>
    protected QueryProviderBase (IQueryExecutor executor, MethodCallExpressionNodeTypeRegistry nodeTypeRegistry)
    {
      ArgumentUtility.CheckNotNull ("executor", executor);
      Executor = executor;
      _expressionTreeParser = new ExpressionTreeParser (nodeTypeRegistry);
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

    /// <summary>
    /// Executes the query defined by the specified expression by first extracting any fetch requests, then parsing it with a 
    /// <see cref="QueryParser"/>, and lastly running it through the <see cref="Executor"/>.
    /// This method is invoked through the <see cref="IQueryProvider"/> interface by methods such as 
    /// <see cref="Queryable.First{TSource}(System.Linq.IQueryable{TSource})"/> and 
    /// <see cref="Queryable.Count{TSource}(System.Linq.IQueryable{TSource})"/>, and it's also invoked by <see cref="QueryableBase{T}"/>
    /// when the <see cref="IQueryable{T}"/> is enumerated.
    /// </summary>
    public virtual TResult Execute<TResult> (Expression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var fetchRequests = GetFetchRequests (ref expression);
      var queryModel = GenerateQueryModel (expression);

      var executionStrategy = queryModel.SelectOrGroupClause.GetExecutionStrategy();
      var executionLambda = executionStrategy.GetExecutionExpression<TResult> (queryModel, fetchRequests);
      var result = executionLambda.Compile () (Executor);
      return result;
    }

    object IQueryProvider.Execute (Expression expression)
    {
      var executeMethod = 
          typeof (QueryProviderBase).GetMethod ("Execute", BindingFlags.Public | BindingFlags.Instance).MakeGenericMethod (expression.Type);
      return executeMethod.Invoke (this, new object[] { expression });
    }

    /// <summary>
    /// Gets all the fetch requests embedded in a query's <see cref="Expression"/> tree as a hierarchical set of <see cref="FetchRequestBase"/> objects.
    /// </summary>
    /// <param name="expression">The expression tree to search for fetch requests. If any is found, the parameter returns a new 
    /// <see cref="Expression"/> instance with all fetch expressions removed from the expression tree.</param>
    /// <returns>An array of <see cref="FetchRequestBase"/> objects that hold the top-level fetch requests for the expression tree.</returns>
    public FetchRequestBase[] GetFetchRequests (ref Expression expression)
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

      var parser = new QueryParser (_expressionTreeParser);
      return parser.GetParsedQuery (expression);
    }
  }
}

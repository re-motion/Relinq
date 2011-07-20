// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.Parsing.Structure;
using Remotion.Linq.Utilities;

namespace Remotion.Linq
{
  /// <summary>
  /// Provides a default implementation of <see cref="IQueryProvider"/> that executes queries (subclasses of <see cref="QueryableBase{T}"/>) by
  /// first parsing them into a <see cref="QueryModel"/> and then passing that to a given implementation of <see cref="IQueryExecutor"/>.
  /// Usually, <see cref="DefaultQueryProvider"/> should be used unless <see cref="CreateQuery{T}"/> must be manually implemented.
  /// </summary>
  public abstract class QueryProviderBase : IQueryProvider
  {
    private static readonly MethodInfo s_genericCreateQueryMethod =
        typeof (QueryProviderBase).GetMethods().Where (m => m.Name == "CreateQuery" && m.IsGenericMethod).Single();

    private readonly IQueryParser _queryParser;
    private readonly IQueryExecutor _executor;

    /// <summary>
    /// Initializes a new instance of <see cref="QueryProviderBase"/> using a custom <see cref="IQueryParser"/>. Use this
    /// constructor to customize how queries are parsed.
    /// </summary>
    /// <param name="queryParser">The <see cref="IQueryParser"/> used to parse queries. Specify an instance of <see cref="Parsing.Structure.QueryParser"/>
    ///   for default behavior.</param>
    /// <param name="executor">The <see cref="IQueryExecutor"/> used to execute queries against a specific query backend.</param>
    protected QueryProviderBase (IQueryParser queryParser, IQueryExecutor executor)
    {
      ArgumentUtility.CheckNotNull ("queryParser", queryParser);
      ArgumentUtility.CheckNotNull ("executor", executor);

      _queryParser = queryParser;
      _executor = executor;
    }

    /// <summary>
    /// Gets the <see cref="QueryParser"/> used by this <see cref="QueryProviderBase"/> to parse LINQ queries.
    /// </summary>
    /// <value>The query parser.</value>
    public IQueryParser QueryParser
    {
      get { return _queryParser; }
    }

    /// <summary>
    /// Gets or sets the implementation of <see cref="IQueryExecutor"/> used to execute queries created via <see cref="CreateQuery{T}"/>.
    /// </summary>
    /// <value>The executor used to execute queries.</value>
    public IQueryExecutor Executor
    {
      get { return _executor; }
    }

    [Obsolete ("This property has been replaced by the QueryParser property. Use QueryParser instead. (1.13.92)", true)]
    public ExpressionTreeParser ExpressionTreeParser
    {
      get { throw new NotImplementedException(); }
    }

    /// <summary>
    /// Constructs an <see cref="IQueryable"/> object that can evaluate the query represented by a specified expression tree. This
    /// method delegates to <see cref="CreateQuery{T}"/>.
    /// </summary>
    /// <param name="expression">An expression tree that represents a LINQ query.</param>
    /// <returns>
    /// An <see cref="IQueryable"/> that can evaluate the query represented by the specified expression tree.
    /// </returns>
    public IQueryable CreateQuery (Expression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      Type elementType = ReflectionUtility.GetItemTypeOfIEnumerable (expression.Type, "expression");
      return (IQueryable) s_genericCreateQueryMethod.MakeGenericMethod (elementType).Invoke (this, new object[] { expression });
    }

    /// <summary>
    /// Constructs an <see cref="IQueryable{T}"/> object that can evaluate the query represented by a specified expression tree. This method is 
    /// called by the standard query operators defined by the <see cref="Queryable"/> class.
    /// </summary>
    /// <param name="expression">An expression tree that represents a LINQ query.</param>
    /// <returns>
    /// An <see cref="IQueryable{T}"/> that can evaluate the query represented by the specified expression tree.
    /// </returns>
    public abstract IQueryable<T> CreateQuery<T> (Expression expression);

    /// <summary>
    /// Executes the query defined by the specified expression by parsing it with a 
    /// <see cref="QueryParser"/> and then running it through the <see cref="Executor"/>.
    /// This method is invoked through the <see cref="IQueryProvider"/> interface methods, for example by 
    /// <see cref="Queryable.First{TSource}(System.Linq.IQueryable{TSource})"/> and 
    /// <see cref="Queryable.Count{TSource}(System.Linq.IQueryable{TSource})"/>, and it's also used by <see cref="QueryableBase{T}"/>
    /// when the <see cref="IQueryable{T}"/> is enumerated.
    /// </summary>
    /// <remarks>
    /// Override this method to replace the query execution mechanism by a custom implementation.
    /// </remarks>
    public virtual IStreamedData Execute (Expression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var queryModel = GenerateQueryModel (expression);
      return queryModel.Execute (Executor);
    }

    /// <summary>
    /// Executes the query defined by the specified expression by parsing it with a
    /// <see cref="QueryParser"/> and then running it through the <see cref="Executor"/>.
    /// The result is cast to <typeparamref name="TResult"/>.
    /// </summary>
    /// <typeparam name="TResult">The type of the query result.</typeparam>
    /// <param name="expression">The query expression to be executed.</param>
    /// <returns>The result of the query cast to <typeparamref name="TResult"/>.</returns>
    /// <remarks>
    /// This method is called by the standard query operators that return a single value, such as 
    /// <see cref="Queryable.Count{TSource}(System.Linq.IQueryable{TSource})"/> or 
    /// <see cref="Queryable.First{TSource}(System.Linq.IQueryable{TSource})"/>.
    /// In addition, it is called by <see cref="QueryableBase{T}"/> to execute queries that return sequences.
    /// </remarks>
    TResult IQueryProvider.Execute<TResult> (Expression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var resultData = Execute (expression);
      return (TResult) resultData.Value;
    }

    /// <summary>
    /// Executes the query defined by the specified expression by parsing it with a
    /// <see cref="QueryParser"/> and then running it through the <see cref="Executor"/>.
    /// </summary>
    /// <param name="expression">The query expression to be executed.</param>
    /// <returns>The result of the query.</returns>
    /// <remarks>
    /// This method is similar to the <see cref="IQueryProvider.Execute{TResult}"/> method, but without the cast to a defined return type.
    /// </remarks>
    object IQueryProvider.Execute (Expression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var resultData = Execute (expression);
      return resultData.Value;
    }

    /// <summary>
    /// The method generates a <see cref="QueryModel"/>.
    /// </summary>
    /// <param name="expression">The query as expression chain.</param>
    /// <returns>a <see cref="QueryModel"/></returns>
    public virtual QueryModel GenerateQueryModel (Expression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      return _queryParser.GetParsedQuery (expression);
    }
  }
}

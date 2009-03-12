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
using Remotion.Data.Linq.Parsing.Structure;
using Remotion.Data.Linq.QueryProviderImplementation;
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
    public QueryProviderBase (IQueryExecutor executor)
    {
      ArgumentUtility.CheckNotNull ("executor", executor);
      Executor = executor;
    }

    public IQueryExecutor Executor { get; private set; }


    public IQueryable CreateQuery (Expression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      MethodInfo genericCreateQueryMethod = typeof (QueryProviderBase).GetMethod ("CreateQueryable", BindingFlags.NonPublic | BindingFlags.Instance);
      
      Type elementType = TypeSystem.GetElementType(expression.Type);
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

    public virtual object Execute (Expression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      return Executor.ExecuteSingle (GenerateQueryModel(expression));
    }

    public virtual TResult Execute<TResult> (Expression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      return (TResult) Executor.ExecuteSingle (GenerateQueryModel (expression));
    }

    /// <summary>
    /// This is where the query is executed and the results are mapped to objects.
    /// </summary>
    /// <param name="expression">The query as expression chain.</param>
    /// <returns></returns>
    public virtual IEnumerable ExecuteCollection (Expression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      return Executor.ExecuteCollection (GenerateQueryModel (expression));
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
      IEnumerable results = Executor.ExecuteCollection (GenerateQueryModel (expression));
      foreach (TResult result in results)
        yield return result;
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

      QueryModel queryModel = parser.GetParsedQuery();
      Console.WriteLine (queryModel.ToString());
      Console.WriteLine (queryModel.PrintQueryModel());
      return queryModel;
    }
    

  }
}

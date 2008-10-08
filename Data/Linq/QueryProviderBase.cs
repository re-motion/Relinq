/* Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh
 *
 * This program is free software: you can redistribute it and/or modify it under 
 * the terms of the re:motion license agreement in license.txt. If you did not 
 * receive it, please visit http://www.re-motion.org/licensing.
 * 
 * Unless otherwise provided, this software is distributed on an "AS IS" basis, 
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 */

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

    public virtual IEnumerable ExecuteCollection (Expression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      return Executor.ExecuteCollection (GenerateQueryModel (expression));
    }

    public virtual IEnumerable<TResult> ExecuteCollection<TResult> (Expression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      IEnumerable results = Executor.ExecuteCollection (GenerateQueryModel (expression));
      foreach (TResult result in results)
        yield return result;
    }

    public QueryModel GenerateQueryModel (Expression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var parser = new QueryParser(expression);
      return parser.GetParsedQuery();
    }
  }
}

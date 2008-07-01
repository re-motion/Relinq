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
      return Executor.ExecuteSingle (GenerateQueryExpression(expression));
    }

    public virtual TResult Execute<TResult> (Expression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      return (TResult) Executor.ExecuteSingle (GenerateQueryExpression (expression));
    }

    public virtual IEnumerable ExecuteCollection (Expression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      return Executor.ExecuteCollection (GenerateQueryExpression (expression));
    }

    public virtual IEnumerable<TResult> ExecuteCollection<TResult> (Expression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      IEnumerable results = Executor.ExecuteCollection (GenerateQueryExpression (expression));
      foreach (TResult result in results)
        yield return result;
    }

    private QueryModel GenerateQueryExpression (Expression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      QueryParser parser = new QueryParser(expression);
      return parser.GetParsedQuery();
    }
  }
}
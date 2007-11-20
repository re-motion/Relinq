using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Rubicon.Data.DomainObjects.Linq.QueryProviderImplementation;
using Rubicon.Utilities;

namespace Rubicon.Data.DomainObjects.Linq
{
  public class QueryProvider : IQueryProvider
  {
    private readonly IQueryExecutor _executor;

    public QueryProvider (IQueryExecutor executor)
    {
      ArgumentUtility.CheckNotNull ("executor", executor);
      _executor = executor;
    }

    public IQueryable CreateQuery (Expression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      MethodInfo genericCreateQueryMethod = typeof (QueryProvider).GetMethod ("GenericCreateQuery", BindingFlags.NonPublic | BindingFlags.Instance);
      
      Type elementType = TypeSystem.GetElementType(expression.Type);
      try
      {
        return (IQueryable) genericCreateQueryMethod.MakeGenericMethod (elementType).Invoke(this, new object[] {expression});
      }
      catch (System.Reflection.TargetInvocationException tie)
      {
        throw tie.InnerException;
      }
    }

    public IQueryable<T> CreateQuery<T> (Expression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      return GenericCreateQuery<T> (expression);
    }

    private IQueryable<T> GenericCreateQuery<T> (Expression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      return new StandardQueryable<T> (this, expression);
    }

    public object Execute (Expression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      return _executor.Execute (expression);
    }

    public TResult Execute<TResult> (Expression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      return _executor.Execute<TResult> (expression);
    }
    
  }
}
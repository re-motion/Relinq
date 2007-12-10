using System;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Rubicon.Data.Linq.Parsing;
using Rubicon.Data.Linq.QueryProviderImplementation;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq
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
      catch (TargetInvocationException tie)
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

    public virtual object Execute (Expression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      return _executor.ExecuteSingle (GenerateQueryExpression(expression));
    }

    public virtual TResult Execute<TResult> (Expression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      return (TResult) _executor.ExecuteSingle (GenerateQueryExpression (expression));
    }

    public virtual IEnumerable ExecuteCollection (Expression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      return _executor.ExecuteCollection (GenerateQueryExpression (expression));
    }

    public virtual TResult ExecuteCollection<TResult> (Expression expression)
        where TResult : IEnumerable
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      return (TResult) _executor.ExecuteCollection (GenerateQueryExpression (expression));
    }

    private QueryExpression GenerateQueryExpression (Expression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      QueryParser parser = new QueryParser(expression);
      return parser.GetParsedQuery();
      
    }
    
  }
}
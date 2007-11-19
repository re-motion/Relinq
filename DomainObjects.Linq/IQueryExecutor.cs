using System.Linq.Expressions;

namespace Rubicon.Data.DomainObjects.Linq
{
  public interface IQueryExecutor
  {
    TResult Execute<TResult> (Expression expression);
    object Execute (Expression expression);
  }
}
using System.Collections;

namespace Rubicon.Data.DomainObjects.Linq
{
  public interface IQueryExecutor
  {
    object ExecuteSingle (QueryExpression queryExpression);
    IEnumerable ExecuteCollection (QueryExpression queryExpression);
  }
}
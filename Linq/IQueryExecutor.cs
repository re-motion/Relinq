using System.Collections;

namespace Rubicon.Data.Linq
{
  public interface IQueryExecutor
  {
    object ExecuteSingle (QueryExpression queryExpression);
    IEnumerable ExecuteCollection (QueryExpression queryExpression);
  }
}
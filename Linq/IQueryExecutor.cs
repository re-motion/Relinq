using System.Collections;

namespace Rubicon.Data.Linq
{
  public interface IQueryExecutor
  {
    object ExecuteSingle (QueryModel queryModel);
    IEnumerable ExecuteCollection (QueryModel queryModel);
  }
}
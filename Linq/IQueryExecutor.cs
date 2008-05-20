using System.Collections;

namespace Remotion.Data.Linq
{
  public interface IQueryExecutor
  {
    object ExecuteSingle (QueryModel queryModel);
    IEnumerable ExecuteCollection (QueryModel queryModel);
  }
}
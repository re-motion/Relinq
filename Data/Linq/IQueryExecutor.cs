using System.Collections;
using Remotion.Collections;
using Remotion.Data.Linq.SqlGeneration;

namespace Remotion.Data.Linq
{
  public interface IQueryExecutor
  {
    object ExecuteSingle (QueryModel queryModel);
    IEnumerable ExecuteCollection (QueryModel queryModel);
  }
}
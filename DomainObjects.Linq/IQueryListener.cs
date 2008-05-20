using Remotion.Data.DomainObjects.Queries;

namespace Remotion.Data.DomainObjects.Linq
{
  public interface IQueryListener
  {
    void QueryConstructed (Query query);
  }
}
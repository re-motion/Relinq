using Rubicon.Data.DomainObjects.Queries;
namespace Rubicon.Data.DomainObjects.Linq
{
  public interface IQueryListener
  {
    void QueryConstructed (Query query);
  }
}
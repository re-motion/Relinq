using Rubicon.Data.DomainObjects.Linq.Clauses;

namespace Rubicon.Data.DomainObjects.Linq
{
  public interface IQueryElement
  {
    void Accept (IQueryVisitor visitor);
  }
}
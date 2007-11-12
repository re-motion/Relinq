namespace Rubicon.Data.DomainObjects.Linq
{
  public interface IQueryElement
  {
    void Accept (IQueryVisitor visitor);
  }
}
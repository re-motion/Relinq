namespace Rubicon.Data.Linq
{
  public interface IQueryElement
  {
    void Accept (IQueryVisitor visitor);
  }
}
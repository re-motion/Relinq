namespace Remotion.Data.Linq
{
  public interface IQueryElement
  {
    void Accept (IQueryVisitor visitor);
  }
}
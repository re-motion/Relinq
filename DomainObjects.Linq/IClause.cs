namespace Rubicon.Data.DomainObjects.Linq
{
  public interface IClause :IQueryElement
  {
    IClause PreviousClause { get; }
  }
}
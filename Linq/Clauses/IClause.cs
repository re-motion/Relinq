namespace Remotion.Data.Linq.Clauses
{
  public interface IClause :IQueryElement
  {
    IClause PreviousClause { get; }
  }
}
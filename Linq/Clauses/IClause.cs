namespace Rubicon.Data.Linq.Clauses
{
  public interface IClause :IQueryElement
  {
    IClause PreviousClause { get; }
  }
}
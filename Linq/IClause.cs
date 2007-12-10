namespace Rubicon.Data.Linq
{
  public interface IClause :IQueryElement
  {
    IClause PreviousClause { get; }
  }
}
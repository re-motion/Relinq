namespace Remotion.Data.Linq.Clauses
{
  public interface IBodyClause : IClause
  {
    QueryModel QueryModel { get; }
    void SetQueryModel (QueryModel model);
  }
}
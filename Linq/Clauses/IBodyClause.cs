using System;

namespace Rubicon.Data.Linq.Clauses
{
  public interface IBodyClause : IClause
  {
    QueryModel QueryModel { get; }
    void SetQueryModel (QueryModel model);
  }
}
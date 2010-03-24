namespace Remotion.Data.Linq.SqlBackend.SqlStatementModel
{
  /// <summary>
  /// Defines whether a join represents a "to-n" relation or a "to-1" relation.
  /// </summary>
  public enum JoinCardinality
  {
    One,
    Many
  }
}
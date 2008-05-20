namespace Remotion.Data.Linq.Parsing
{
  public enum ParseContext
  {
    TopLevelQuery, SubQueryInFrom, SubQueryInWhere, LetExpression, SubQueryInSelect
  }
}
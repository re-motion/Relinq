namespace Rubicon.Data.Linq.Parsing
{
  public enum ParseContext
  {
    TopLevelQuery, SubQueryInFrom, SubQueryInWhere, LetExpression
  }
}
using System.Linq;
using System.Linq.Expressions;

namespace Rubicon.Data.Linq.UnitTests.TestQueryGenerators
{
  public static class LetTestQueryGenerator
  {
    public static IQueryable<string> CreateSimpleLetClause (IQueryable<Student> source)
    {
      return from s in source let x = s.First + s.Last select x;
    }

    public static MethodCallExpression CreateSimpleSelect_LetExpression (IQueryable<Student> source)
    {
      IQueryable<string> query = CreateSimpleLetClause (source);
      return (MethodCallExpression) query.Expression;
    }
  }
}
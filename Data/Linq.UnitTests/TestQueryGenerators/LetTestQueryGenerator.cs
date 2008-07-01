using System.Linq;
using System.Linq.Expressions;

namespace Remotion.Data.Linq.UnitTests.TestQueryGenerators
{
  public static class LetTestQueryGenerator
  {
    public static IQueryable<string> CreateSimpleLetClause (IQueryable<Student> source)
    {
      return from s in source let x = s.First + s.Last select x;
    }

    public static IQueryable<string> CreateLet_WithJoin_NoTable (IQueryable<Student_Detail> source)
    {
      return from sd in source let x = sd.Student.First select x;
    }

    public static IQueryable<Student> CreateLet_WithJoin_WithTable (IQueryable<Student_Detail> source)
    {
      return from sd in source let x = sd.Student select x;
    }

    public static IQueryable<Student> CreateLet_WithTable (IQueryable<Student> source)
    {
      return from s in source let x = s select x;
    }

    public static IQueryable<string> CreateMultiLet_WithWhere (IQueryable<Student> source)
    {
      return from s in source let x = s.First let y = s.ID where y > 1 select x;
    }


    public static MethodCallExpression CreateSimpleSelect_LetExpression (IQueryable<Student> source)
    {
      IQueryable<string> query = CreateSimpleLetClause (source);
      return (MethodCallExpression) query.Expression;
    }
  }
}
using System;
using System.Linq;
using System.Linq.Expressions;
using Rubicon.Collections;
using Rubicon.Data.Linq.UnitTests.ParsingTest.FieldResolvingTest;

namespace Rubicon.Data.Linq.UnitTests.TestQueryGenerators
{
  public static class SelectTestQueryGenerator
  {
    public static IQueryable<Student> CreateSimpleQueryWithNonDBProjection (IQueryable<Student> source)
    {
      return from s in source select (Student) null;
    }

    public static IQueryable<Tuple<string, string>> CreateSimpleQueryWithFieldProjection (IQueryable<Student> source)
    {
      return from s in source select new Tuple<string, string> (s.First, s.Last);
    }

    public static IQueryable<Tuple<Student, string, string, string>> CreateSimpleQueryWithSpecialProjection (IQueryable<Student> source)
    {
      string k = "Test";
      return from s in source select Tuple.NewTuple (s, s.Last, k, "Test2");
    }

    public static IQueryable<string> CreateSimpleQueryWithProjection (IQueryable<Student> source)
    {
      return from s in source select s.First;
    }

    public static IQueryable<string> CreateSimpleSelectWithNonDbProjection (IQueryable<Student> source1)
    {
      return from s1 in source1 select s1.NonDBProperty;
    }

    public static IQueryable<int> CreateSimpleSelectWithNonEntityMemberAccess (IQueryable<Student> source1)
    {
      DateTime now = DateTime.Now;
      return from s1 in source1 select now.Day;
    }

    public static IQueryable<Student> CreateRelationMemberSelectQuery(IQueryable<Student_Detail> source)
    {
      return from sd in source select sd.Student;
    }

    public static MethodCallExpression CreateSimpleQuery_SelectExpression (IQueryable<Student> source)
    {
      IQueryable<Student> query = CreateSimpleQuery (source);
      return (MethodCallExpression) query.Expression;
    }

    public static IQueryable<Student> CreateSimpleQuery (IQueryable<Student> source)
    {
      return from s in source select s;
    }

    public static IQueryable<string> CreateSimpleQuery_WithProjection (IQueryable<Student> source)
    {
      return from s in source select s.First;
    }

    public static IQueryable<string> CreateUnaryBinaryLambdaInvocationConvertNewArrayExpressionQuery (IQueryable<Student> source1)
    {
      return from s1 in source1 select ((Func<string, string>) ((string s) => s1.First)) (s1.Last) + new string[] { s1.ToString () }[s1.ID];
    }

    
  }
}
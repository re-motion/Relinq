using System.Linq;
using System.Linq.Expressions;
using Rubicon.Collections;
using System;

namespace Rubicon.Data.Linq.UnitTests
{
  public static class TestQueryGenerator
  {
    public static IQueryable<Student> CreateSimpleQuery(IQueryable<Student> source)
    {
      return from s in source select s;
    }

    public static IQueryable<Student> CreateSimpleQueryWithNonDBProjection (IQueryable<Student> source)
    {
      return from s in source select (Student)null;
    }

    public static IQueryable<Tuple<string,string>> CreateSimpleQueryWithFieldProjection (IQueryable<Student> source)
    {
      return from s in source select new Tuple<string,string>(s.First,s.Last);
    }

    public static IQueryable<Tuple<Student, string,string,string >> CreateSimpleQueryWithSpecialProjection (IQueryable<Student> source)
    {
      string k = "Test";
      return from s in source select Tuple.NewTuple (s, s.Last,k,"Test2");
    }

    public static IQueryable<string> CreateSimpleQueryWithProjection (IQueryable<Student> source)
    {
      return from s in source select s.First;
    }

    public static IQueryable<Student> CreateSimpleWhereQuery (IQueryable<Student> source)
    {
      return from s in source where s.Last == "Garcia" select s;
    }


    public static IQueryable<Student> CreateMultiFromWhereQuery (IQueryable<Student> source1,IQueryable<Student> source2)
    {
      return from s1 in source1 from s2 in source2 where s1.Last == "Garcia" select s1;
    }

    public static IQueryable<Student> CreateMultiWhereQuery (IQueryable<Student> source)
    {
      return from s in source where s.Last == "Garcia" where s.First == "Hugo" where s.ID > 100 select s;
    }

    public static IQueryable<string> CreateSelectWhereQuery (IQueryable<Student> source)
    {
      return from s in source where s.Last == "Garcia" select s.First;
    }
        
    public static  IQueryable<Student> CreateMultiFromQuery (IQueryable<Student> source1,IQueryable<Student> source2)
    {
      return from s1 in source1 from s2 in source2 select s1;
    }

    public static IQueryable<Student> CreateReverseFromWhereQuery (IQueryable<Student> source1, IQueryable<Student> source2)
    {
      return from s1 in source1 where s1.Last == "Garcia" from s2 in source2 select s1;
    }

    public static IQueryable<string> CreateReverseFromWhereQueryWithProjection (IQueryable<Student> source1, IQueryable<Student> source2)
    {
      return from s1 in source1 where s1.Last == "Garcia" from s2 in source2 select s2.Last;
    }

    public static IQueryable<Student> CreateThreeFromQuery (IQueryable<Student> source1, IQueryable<Student> source2,IQueryable<Student> source3)
    {
      return from s1 in source1 from s2 in source2 from s3 in source3 select s1;
    }

    public static IQueryable<Student> CreateThreeFromQueryWithSelectS2 (IQueryable<Student> source1, IQueryable<Student> source2, IQueryable<Student> source3)
    {
      return from s1 in source1 from s2 in source2 from s3 in source3 select s2;
    }

    public static IQueryable<Student> CreateThreeFromWhereQuery (IQueryable<Student> source1, IQueryable<Student> source2, IQueryable<Student> source3)
    {
      return from s1 in source1 from s2 in source2 where s1.First == "Hugo" from s3 in source3 select s1;
    }

    public static IQueryable<Student> CreateWhereFromWhereQuery (IQueryable<Student> source1, IQueryable<Student> source2)
    {
      return from s1 in source1 where s1.First == "Hugo" from s2 in source2 where s1.Last == "Garcia" select s1;
    }

    public static IQueryable<string> CreateSimpleSelectWithNonDbProjection(IQueryable<Student> source1)
    {
      return from s1 in source1 select s1.NonDBProperty;
    }

    public static IQueryable<int> CreateSimpleSelectWithNonEntityMemberAccess(IQueryable<Student> source1)
    {
      DateTime now = DateTime.Now;
      return from s1 in source1 select now.Day;
    }

    public static IQueryable<Tuple<string,string ,int>> CreateMultiFromQueryWithProjection(IQueryable<Student> source1,IQueryable<Student> source2,IQueryable<Student> source3 )
    {
      return from s1 in source1 from s2 in source2 from s3 in source3 select Tuple.NewTuple (s1.First, s2.Last, s3.ID);
    }

    public static IQueryable<string> CreateUnaryBinaryLambdaInvocationConvertNewArrayExpressionQuery (IQueryable<Student> source1)
    {
      return from s1 in source1 select ((Func<string, string>) ((string s) => s1.First)) (s1.Last) + new string[] { s1.ToString () }[s1.ID];
    }
    
   

    public static MethodCallExpression CreateSimpleQuery_SelectExpression (IQueryable<Student> source)
    {
      IQueryable<Student> query = CreateSimpleQuery (source);
      return (MethodCallExpression) query.Expression;
    }

    public static MethodCallExpression CreateSimpleWhereQuery_WhereExpression (IQueryable<Student> source)
    {
      IQueryable<Student> query = CreateSimpleWhereQuery (source);
      return (MethodCallExpression) query.Expression;
    }

    public static MethodCallExpression CreateMultiWhereQuery_WhereExpression (IQueryable<Student> source)
    {
      IQueryable<Student> query = CreateMultiWhereQuery (source);
      return (MethodCallExpression) query.Expression;
    }

    public static MethodCallExpression CreateMultiFromWhere_WhereExpression(IQueryable<Student> source1,IQueryable<Student> source2)
    {
      IQueryable<Student> query = CreateMultiFromWhereQuery (source1, source2);
      return (MethodCallExpression) query.Expression;
    }

    public static MethodCallExpression CreateReverseFromWhere_WhereExpression (IQueryable<Student> source1, IQueryable<Student> source2)
    {
      IQueryable<Student> query = CreateReverseFromWhereQuery (source1, source2);
      return (MethodCallExpression) query.Expression;
    }

    public static MethodCallExpression CreateReverseFromWhereWithProjection_SelectExpression (IQueryable<Student> source1, IQueryable<Student> source2)
    {
      IQueryable<string> query = CreateReverseFromWhereQueryWithProjection(source1, source2);
      return (MethodCallExpression) query.Expression;
    }

    public static MethodCallExpression CreateSelectWhereQuery_SelectExpression (IQueryable<Student> source)
    {
      IQueryable<string> query = CreateSelectWhereQuery (source);
      return (MethodCallExpression) query.Expression;
    }

    public static MethodCallExpression CreateMultiFromQuery_SelectManyExpression (IQueryable<Student> source1,IQueryable<Student> source2)
    {
      IQueryable<Student> query = CreateMultiFromQuery (source1, source2);
      return (MethodCallExpression) query.Expression;
    }

    public static MethodCallExpression CreateThreeFromQuery_SelectManyExpression (IQueryable<Student> source1, IQueryable<Student> source2, IQueryable<Student> source3)
    {
      IQueryable<Student> query = CreateThreeFromQuery (source1, source2,source3);
      return (MethodCallExpression) query.Expression;
    }

    public static MethodCallExpression CreateWhereFromWhere_WhereExpression (IQueryable<Student> source1, IQueryable<Student> source2)
    {
      IQueryable<Student> query = CreateWhereFromWhereQuery (source1, source2);
      return (MethodCallExpression) query.Expression;
    }

    public static MethodCallExpression CreateThreeFromWhereQuery_SelectManyExpression (IQueryable<Student> source1, IQueryable<Student> source2, IQueryable<Student> source3)
    {
      IQueryable<Student> query = CreateThreeFromWhereQuery (source1, source2, source3);
      return (MethodCallExpression) query.Expression;
    }



  }
}
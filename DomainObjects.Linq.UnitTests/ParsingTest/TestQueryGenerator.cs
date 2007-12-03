using System.Linq;
using System.Linq.Expressions;
using Rubicon.Data.DomainObjects.Linq.UnitTests.Parsing;

namespace Rubicon.Data.DomainObjects.Linq.UnitTests.ParsingTest
{
  public static class TestQueryGenerator
  {
    public static IQueryable<Student> CreateSimpleQuery(IQueryable<Student> source)
    {
      return from s in source select s;
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

    public static IQueryable<Student> CreateThreeFromWhereQuery (IQueryable<Student> source1, IQueryable<Student> source2, IQueryable<Student> source3)
    {
      return from s1 in source1 from s2 in source2 where s1.First == "Hugo" from s3 in source3 select s1;
    }

    public static IQueryable<Student> CreateWhereFromWhereQuery (IQueryable<Student> source1, IQueryable<Student> source2)
    {
      return from s1 in source1 where s1.First == "Hugo" from s2 in source2 where s1.Last == "Garcia" select s1;
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
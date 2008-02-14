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

    public static IQueryable<Student> CreateWhereQueryWithEvaluatableSubExpression (IQueryable<Student> source)
    {
      string cia = "cia";
      return from s in source where s.Last == ("Gar" + cia) select s;
    }

    public static IQueryable<Student> CreateMultiFromWhereQuery (IQueryable<Student> source1,IQueryable<Student> source2)
    {
      return from s1 in source1 from s2 in source2 where s1.Last == "Garcia" select s1;
    }

    
    public static IQueryable<Student> CreateMultiFromWhereOrderByQuery (IQueryable<Student> source1, IQueryable<Student> source2)
    {
      return from s1 in source1 from s2 in source2 where s1.Last == "Garcia" orderby s1.First ascending,s2.Last descending select s1;
    }

    public static IQueryable<Student> CreateSimpleOrderByQuery (IQueryable<Student> source)
    {
      return from s1 in source orderby s1.First select s1 ;
    }

    public static IQueryable<Student> CreateOrderByNonDBPropertyQuery (IQueryable<Student> source)
    {
      return from s1 in source orderby s1.NonDBProperty select s1;
    }

    public static IQueryable<Student> CreateTwoOrderByQuery (IQueryable<Student> source)
    {
      return from s1 in source orderby s1.First orderby s1.Last descending select s1;
    }

    public static IQueryable<Student> CreateThreeOrderByQuery (IQueryable<Student> source)
    {
      return from s1 in source orderby s1.First,s1.Last orderby s1.Last descending select s1;
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

    public static IQueryable<Student> CreateWhereQueryWithDifferentComparisons (IQueryable<Student> source)
    {
      return from s in source where s.First != "Garcia" &&  s.ID > 5 && s.ID >= 6 && s.ID < 7 && s.ID <=6 && s.ID == 6 select s;
    }

    public static IQueryable<Student> CreateWhereQueryWithOrAndNot (IQueryable<Student> source)
    {
      return from s in source where (!(s.First == "Garcia") || s.First == "Garcia") && s.First == "Garcia" select s;
    }

    
    public static IQueryable<Student> CreateWhereQueryWithStartsWith (IQueryable<Student> source)
    {
      return from s in source where s.First.StartsWith("Garcia") select s;
    }

    public static IQueryable<Student> CreateWhereQueryWithEndsWith (IQueryable<Student> source)
    {
      return from s in source where s.First.EndsWith("Garcia") select s;
    }

    public static IQueryable<Student> CreateWhereQueryNullChecks (IQueryable<Student> source)
    {
      return from s in source where s.First == null || null != s.Last select s;
    }

    public static IQueryable<Student> CreateWhereQueryBooleanConstantTrue (IQueryable<Student> source)
    {
      return from s in source where true select s;
    }

    public static IQueryable<Student> CreateWhereQueryBooleanConstantFalse (IQueryable<Student> source)
    {
      return from s in source where false select s;
    }

    public static IQueryable<Student> CreateOrderByQueryWithOrderByAndThenBy(IQueryable<Student> source)
    {
     return from s in source orderby s.First,s.Last descending,s.Scores select s;
    }

    public static IQueryable<Student> CreateOrderByQueryWithMultipleOrderBys (IQueryable<Student> source)
    {
      return from s in source orderby s.First, s.Last descending, s.Scores orderby s.Last select s;
    }

    public static IQueryable<Student> CreateOrderByWithWhereCondition( IQueryable<Student> source)
    {
      return from s in source where s.First == "Garcia" orderby s.First select s;
    }

    public static IQueryable<Student> CreateOrderByWithWhereConditionAndMultiFrom (IQueryable<Student> source1, IQueryable<Student> source2)
    {
      return from s1 in source1 where s1.First == "Garcia" orderby s1.First from s2 in source2 where s2.Last == "Garcia" orderby s2.First, s2.Last orderby s2.First select s2;
    }

    public static IQueryable<Student_Detail> CreateSimpleImplicitWhereJoin (IQueryable<Student_Detail> source)
    {
      return from s in source where s.Student.First == "Garcia" select s;
    }

    public static IQueryable<Student> CreateSimpleExplicitJoin (IQueryable<Student_Detail> source1,IQueryable<Student> source2)
    {
      return from s1 in source2 join s2 in source1 on s1.ID equals s2.StudentID select s1;
    }

    public static IQueryable<Student_Detail> CreateSimpleImplicitOrderByJoin (IQueryable<Student_Detail> source)
    {
      return from s in source orderby s.Student.First select s;
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
    public static MethodCallExpression CreateWhereQueryWithEvaluatableSubExpression_WhereExpression (IQueryable<Student> source)
    {
      IQueryable<Student> query = CreateWhereQueryWithEvaluatableSubExpression (source);
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

    public static MethodCallExpression CreateOrderByQueryWithOrderByAndThenBy_OrderByExpression(IQueryable<Student> source)
    {
      IQueryable<Student> query = CreateOrderByQueryWithOrderByAndThenBy (source);
      return (MethodCallExpression) query.Expression;
    }

    public static MethodCallExpression CreateOrderByQueryWithMultipleOrderBys_OrderByExpression (IQueryable<Student> source)
    {
      IQueryable<Student> query = CreateOrderByQueryWithMultipleOrderBys (source);
      return (MethodCallExpression) query.Expression;
    }

    public static MethodCallExpression CreateOrderByQueryWithWhere_OrderByExpression (IQueryable<Student> source)
    {
      IQueryable<Student> query = CreateOrderByWithWhereCondition (source);
      return (MethodCallExpression) query.Expression;
    }
    



  }
}
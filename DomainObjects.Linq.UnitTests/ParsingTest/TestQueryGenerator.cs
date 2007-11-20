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

    public static IQueryable<Student> CreateMultiWhereQuery (IQueryable<Student> source)
    {
      return from s in source where s.Last == "Garcia" where s.First == "Hugo" where s.ID > 100 select s;
    }

    public static MethodCallExpression CreateSimpleQuerySelectExpression (IQueryable<Student> source)
    {
      IQueryable<Student> simpleQuery = CreateSimpleQuery (source);
      return (MethodCallExpression) simpleQuery.Expression;
    }

    public static MethodCallExpression CreateSimpleWhereQueryWhereExpression (IQueryable<Student> source)
    {
      IQueryable<Student> simpleWhereQuery = CreateSimpleWhereQuery (source);
      return (MethodCallExpression) simpleWhereQuery.Expression;
    }

    public static MethodCallExpression CreateMultiWhereQueryWhereExpression (IQueryable<Student> source)
    {
      IQueryable<Student> multiWhereQuery = CreateMultiWhereQuery (source);
      return (MethodCallExpression) multiWhereQuery.Expression;
    }
  }
}
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Rhino.Mocks;
using Rubicon.Data.DomainObjects.Linq.Clauses;
using Rubicon.Data.DomainObjects.Linq.QueryProviderImplementation;
using Rubicon.Data.DomainObjects.Linq.UnitTests.Parsing;

namespace Rubicon.Data.DomainObjects.Linq.UnitTests
{
  public static class ExpressionHelper
  {
    public static IQueryExecutor s_executor = CreateExecutor();

    public static Expression CreateExpression ()
    {
      return CreateNewIntArrayExpression();
    }

    public static LambdaExpression CreateLambdaExpression ()
    {
      return Expression.Lambda (Expression.Constant (0));
    }

    public static Expression CreateNewIntArrayExpression ()
    {
      return Expression.NewArrayInit (typeof (int));
    }

    public static ParameterExpression CreateParameterExpression ()
    {
      return Expression.Parameter (typeof (int), "i");
    }

    public static JoinClause CreateJoinClause ()
    {
      ParameterExpression identifier = ExpressionHelper.CreateParameterExpression ();
      Expression inExpression = ExpressionHelper.CreateExpression ();
      Expression onExpression = ExpressionHelper.CreateExpression ();
      Expression equalityExpression = ExpressionHelper.CreateExpression ();

      return new JoinClause (identifier, inExpression, onExpression, equalityExpression);
    }

    public static FromClause CreateFromClause ()
    {
      ParameterExpression id = ExpressionHelper.CreateParameterExpression ();
      IQueryable querySource = ExpressionHelper.CreateQuerySource ();
      
      
      return new FromClause (id, querySource);
    }

    public static GroupClause CreateGroupClause ()
    {
      Expression groupExpression = ExpressionHelper.CreateExpression ();
      Expression byExpression = ExpressionHelper.CreateExpression ();

      return new GroupClause (groupExpression, byExpression);
    }


    public static LetClause CreateLetClause ()
    {
      ParameterExpression identifier = ExpressionHelper.CreateParameterExpression ();
      Expression expression = ExpressionHelper.CreateExpression ();

      return new LetClause (identifier, expression);
    }

    public static OrderingClause CreateOrderingClause()
    {
      Expression expression = ExpressionHelper.CreateExpression ();
      return new OrderingClause (expression, OrderDirection.Asc);
    }

    public static OrderByClause CreateOrderByClause()
    {
      OrderingClause ordering = ExpressionHelper.CreateOrderingClause ();
      return new OrderByClause (ordering);
    }

    public static QueryBody CreateQueryBody()
    {
      ISelectGroupClause iSelectOrGroupClause = CreateSelectClause();

      return new QueryBody (iSelectOrGroupClause);

    }

    public static SelectClause CreateSelectClause ()
    {
      LambdaExpression expression = ExpressionHelper.CreateLambdaExpression ();

      return new SelectClause (new LambdaExpression[] {expression});
    }

    public static WhereClause CreateWhereClause ()
    {
      Expression boolExpression = ExpressionHelper.CreateExpression ();
      return new WhereClause (boolExpression);
    }

    public static IQueryable<Student> CreateQuerySource()
    {
      return new StandardQueryable<Student> (s_executor);
    }

    private static IQueryExecutor CreateExecutor()
    {
      MockRepository repository = new MockRepository();
      IQueryExecutor executor = repository.CreateMock<IQueryExecutor>();
      // Expect.Call (executor.Execute<IEnumerable<Student>>(null)).IgnoreArguments().Return (CreateStudents());
      return executor;
    }

    public static MethodCallExpression CreateSimpleQuerySelectExpression (IQueryable<Student> source)
    {
      IQueryable<Student> simpleQuery = TestQueryGenerator.CreateSimpleQuery (source);
      return (MethodCallExpression) simpleQuery.Expression;
    }

    public static MethodCallExpression CreateSimpleWhereQueryWhereExpression ()
    {
      IQueryable<Student> simpleWhereQuery = TestQueryGenerator.CreateSimpleWhereQuery (CreateQuerySource ());
      return (MethodCallExpression) simpleWhereQuery.Expression;
    }

    private static List<Student> CreateStudents ()
    {
      List<Student> students = new List<Student>
      {
        new Student {First="Svetlana", Last="Omelchenko", ID=111, Scores= new List<int> {97, 92, 81, 60}},
        new Student {First="Claire", Last="O’Donnell", ID=112, Scores= new List<int> {75, 84, 91, 39}},
        new Student {First="Sven", Last="Mortensen", ID=113, Scores= new List<int> {88, 94, 65, 91}},
        new Student {First="Cesar", Last="Garcia", ID=114, Scores= new List<int> {97, 89, 85, 82}},
        new Student {First="Debra", Last="Garcia", ID=115, Scores= new List<int> {35, 72, 91, 70}},
        new Student {First="Fadi", Last="Fakhouri", ID=116, Scores= new List<int> {99, 86, 90, 94}},
        new Student {First="Hanying", Last="Feng", ID=117, Scores= new List<int> {93, 92, 80, 87}},
        new Student {First="Hugo", Last="Garcia", ID=118, Scores= new List<int> {92, 90, 83, 78}},
        new Student {First="Lance", Last="Tucker", ID=119, Scores= new List<int> {68, 79, 88, 92}},
        new Student {First="Terry", Last="Adams", ID=120, Scores= new List<int> {99, 82, 81, 79}},
        new Student {First="Eugene", Last="Zabokritski", ID=121, Scores= new List<int> {96, 85, 91, 60}},
        new Student {First="Michael", Last="Tucker", ID=122, Scores= new List<int> {94, 92, 91, 91} }
      };
      return students;
    }
  }
}
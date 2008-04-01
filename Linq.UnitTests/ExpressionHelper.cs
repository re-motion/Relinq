using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Rhino.Mocks;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Data.Linq.Parsing.Structure;
using Rubicon.Data.Linq.QueryProviderImplementation;

namespace Rubicon.Data.Linq.UnitTests
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

      return new JoinClause (CreateMainFromClause(), identifier, inExpression, onExpression, equalityExpression);
    }

    public static QueryModel CreateQueryModel (MainFromClause mainFromClause)
    {
      return new QueryModel (typeof (IQueryable<Student>), mainFromClause, CreateSelectClause());
    }

    public static QueryModel CreateQueryModel ()
    {
      return CreateQueryModel (CreateMainFromClause(Expression.Parameter (typeof (Student), "s"), CreateQuerySource()));
    }


    public static MainFromClause CreateMainFromClause ()
    {
      ParameterExpression id = ExpressionHelper.CreateParameterExpression ();
      IQueryable querySource = ExpressionHelper.CreateQuerySource (); 
      return CreateMainFromClause(id, querySource);
    }

    public static AdditionalFromClause CreateAdditionalFromClause ()
    {
      return new AdditionalFromClause (CreateClause (), CreateParameterExpression (), CreateLambdaExpression (), CreateLambdaExpression ());
    }

    public static GroupClause CreateGroupClause ()
    {
      Expression groupExpression = ExpressionHelper.CreateExpression ();
      Expression byExpression = ExpressionHelper.CreateExpression ();

      return new GroupClause (CreateClause (), groupExpression, byExpression);
    }


    public static LetClause CreateLetClause ()
    {
      ParameterExpression identifier = ExpressionHelper.CreateParameterExpression ();
      Expression expression = ExpressionHelper.CreateExpression ();

      return new LetClause (CreateClause (), identifier, expression);
    }

    public static OrderingClause CreateOrderingClause()
    {
      LambdaExpression expression = ExpressionHelper.CreateLambdaExpression ();
      return new OrderingClause (CreateClause (), expression, OrderDirection.Asc);
    }

    public static OrderByClause CreateOrderByClause()
    {
      OrderingClause ordering = ExpressionHelper.CreateOrderingClause ();
      return new OrderByClause (ordering);
    }

    public static SelectClause CreateSelectClause ()
    {
      LambdaExpression expression = Expression.Lambda (Expression.Constant (0), Expression.Parameter (typeof (Student), "s1"));
      return new SelectClause (CreateClause (), expression,false);
    }



    public static WhereClause CreateWhereClause ()
    {
      LambdaExpression boolExpression = 
          Expression.Lambda (Expression.MakeBinary (ExpressionType.Equal, Expression.Constant (1), Expression.Constant (2)));
      return new WhereClause (CreateClause (), boolExpression);
    }

    public static IClause CreateClause()
    {
      return CreateMainFromClause();
    }

    public static IQueryable<Student> CreateQuerySource()
    {
      return CreateQuerySource (s_executor);
    }
        
    public static IQueryable<Student> CreateQuerySource (IQueryExecutor executor)
    {
      return new TestQueryable<Student> (executor);
    }

    public static IQueryable<Student_Detail> CreateQuerySource_Detail()
    {
      return CreateQuerySource_Detail (s_executor);
    }

    public static IQueryable<Student_Detail> CreateQuerySource_Detail (IQueryExecutor executor)
    {
      return new TestQueryable<Student_Detail> (executor);
    }

    public static IQueryable<Student_Detail_Detail> CreateQuerySource_Detail_Detail ()
    {
      return CreateQuerySource_Detail_Detail (s_executor);
    }

    public static IQueryable<Student_Detail_Detail> CreateQuerySource_Detail_Detail (IQueryExecutor executor)
    {
      return new TestQueryable<Student_Detail_Detail> (executor);
    }

    public static  IQueryable<IndustrialSector> CreateQuerySource_IndustrialSector ()
    {
      return CreateQuerySource_IndustrialSector (s_executor);
    }

    public static IQueryable<IndustrialSector> CreateQuerySource_IndustrialSector( IQueryExecutor executor)
    {
      return new TestQueryable<IndustrialSector> (executor);
    }

    public static IQueryExecutor CreateExecutor()
    {
      MockRepository repository = new MockRepository();
      IQueryExecutor executor = repository.CreateMock<IQueryExecutor>();
      // Expect.Call (executor.Execute<IEnumerable<Student>>(null)).IgnoreArguments().Return (CreateStudents());
      return executor;
    }

    //public static List<Student> CreateStudents ()
    //{
    //  List<Student> students = new List<Student>
    //  {
    //    new Student {First="Svetlana", Last="Omelchenko", ID=111, Scores= new List<int> {97, 92, 81, 60}},
    //    new Student {First="Claire", Last="O’Donnell", ID=112, Scores= new List<int> {75, 84, 91, 39}},
    //    new Student {First="Sven", Last="Mortensen", ID=113, Scores= new List<int> {88, 94, 65, 91}},
    //    new Student {First="Cesar", Last="Garcia", ID=114, Scores= new List<int> {97, 89, 85, 82}},
    //    new Student {First="Debra", Last="Garcia", ID=115, Scores= new List<int> {35, 72, 91, 70}},
    //    new Student {First="Fadi", Last="Fakhouri", ID=116, Scores= new List<int> {99, 86, 90, 94}},
    //    new Student {First="Hanying", Last="Feng", ID=117, Scores= new List<int> {93, 92, 80, 87}},
    //    new Student {First="Hugo", Last="Garcia", ID=118, Scores= new List<int> {92, 90, 83, 78}},
    //    new Student {First="Lance", Last="Tucker", ID=119, Scores= new List<int> {68, 79, 88, 92}},
    //    new Student {First="Terry", Last="Adams", ID=120, Scores= new List<int> {99, 82, 81, 79}},
    //    new Student {First="Eugene", Last="Zabokritski", ID=121, Scores= new List<int> {96, 85, 91, 60}},
    //    new Student {First="Michael", Last="Tucker", ID=122, Scores= new List<int> {94, 92, 91, 91} }
    //  };
    //  return students;
    //}



    

    public static object ExecuteLambda (LambdaExpression lambdaExpression, params object[] args)
    {
      return lambdaExpression.Compile().DynamicInvoke (args);
    }

    public static QueryModel ParseQuery<T> (IQueryable<T> query)
    {
      Expression expression = query.Expression;
      QueryParser parser = new QueryParser (expression);
      return parser.GetParsedQuery ();
    }

    public static FieldDescriptor CreateFieldDescriptor (FromClauseBase fromClause, MemberInfo member)
    {
      MemberInfo originalMember = member;
      return CreateFieldDescriptor(fromClause, member, originalMember);
    }

    public static FieldDescriptor CreateFieldDescriptor (FromClauseBase fromClause, MemberInfo member, MemberInfo originalMember)
    {
      IFromSource table = fromClause.GetFromSource (StubDatabaseInfo.Instance);
      Column? column = DatabaseInfoUtility.GetColumn (StubDatabaseInfo.Instance, table, member);
      FieldSourcePath sourcePath = new FieldSourcePath (table, new SingleJoin[0]);
      FieldDescriptor fieldDescriptor = new FieldDescriptor (originalMember, fromClause, sourcePath, column);
      return fieldDescriptor;
    }

    public static FieldSourcePath GetPathForNewTable ()
    {
      return new FieldSourcePath (new Table (), new SingleJoin[0]);
    }

    public static FieldSourcePath GetPathForNewTable (string name, string alias)
    {
      Table table = new Table (name, alias);
      return GetPathForTable(table);
    }

    public static FieldSourcePath GetPathForTable (Table table)
    {
      return new FieldSourcePath (table, new SingleJoin[0]);
    }

    public static MainFromClause CreateMainFromClause (ParameterExpression identifier, IQueryable querySource)
    {
      return new MainFromClause (identifier, Expression.Constant (querySource));
    }

    public static SubQueryFromClause CreateSubQueryFromClause ()
    {
      return CreateSubQueryFromClause (CreateParameterExpression());
    }

    public static SubQueryFromClause CreateSubQueryFromClause (ParameterExpression identifier)
    {
      return new SubQueryFromClause (CreateClause (), identifier, CreateQueryModel (), CreateLambdaExpression ());
    }

    public static MethodInfo GetMethod<T> (Expression<Func<T>> wrappedCall)
    {
      return ((MethodCallExpression) wrappedCall.Body).Method;
    }
  }
}
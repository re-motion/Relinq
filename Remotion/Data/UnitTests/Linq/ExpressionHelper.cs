// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2008 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// version 3.0 as published by the Free Software Foundation.
// 
// re-motion is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-motion; if not, see http://www.gnu.org/licenses.
// 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Data.Linq;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.UnitTests.Linq.TestQueryGenerators;
using Rhino.Mocks;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing.Structure;

namespace Remotion.Data.UnitTests.Linq
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
      ParameterExpression identifier = CreateParameterExpression ();
      Expression inExpression = CreateExpression ();
      Expression onExpression = CreateExpression ();
      Expression equalityExpression = CreateExpression ();

      return new JoinClause (CreateClause(), CreateMainFromClause(), identifier, inExpression, onExpression, equalityExpression);
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
      ParameterExpression id = CreateParameterExpression ();
      IQueryable querySource = CreateQuerySource (); 
      return CreateMainFromClause(id, querySource);
    }

    public static AdditionalFromClause CreateAdditionalFromClause ()
    {
      ParameterExpression identifier = CreateParameterExpression ();
      return CreateAdditionalFromClause(identifier);
    }

    public static AdditionalFromClause CreateAdditionalFromClause (ParameterExpression identifier)
    {
      return new AdditionalFromClause (CreateClause (), identifier, CreateLambdaExpression (), CreateLambdaExpression ());
    }

    public static GroupClause CreateGroupClause ()
    {
      Expression groupExpression = CreateExpression ();
      Expression byExpression = CreateExpression ();

      return new GroupClause (CreateClause (), groupExpression, byExpression);
    }
    
    public static LetClause CreateLetClause ()
    {
      ParameterExpression identifier = CreateParameterExpression ();
      return CreateLetClause(identifier);
    }

    public static LetClause CreateLetClause (ParameterExpression identifier)
    {
      return new LetClause (CreateClause (), identifier, CreateExpression (), CreateLambdaExpression ());
    }

    public static LetClause CreateLetClause (Expression expression)
    {
      ParameterExpression identifier = CreateParameterExpression ();
      return new LetClause (CreateLetClause (), identifier, expression, CreateLambdaExpression ());
    }


    public static OrderingClause CreateOrderingClause()
    {
      LambdaExpression expression = CreateLambdaExpression ();
      return new OrderingClause (CreateClause (), expression, OrderDirection.Asc);
    }

    public static OrderByClause CreateOrderByClause()
    {
      OrderingClause ordering = CreateOrderingClause ();
      return new OrderByClause (ordering);
    }

    public static SelectClause CreateSelectClause ()
    {
      LambdaExpression expression = Expression.Lambda (Expression.Constant (0), Expression.Parameter (typeof (Student), "s1"));
      //return new SelectClause (CreateClause (), expression,false);
      return new SelectClause (CreateClause (), expression, null);
    }

    public static SelectClause CreateNewSelectClause ()
    {
      LambdaExpression expression = Expression.Lambda (Expression.Constant (0), Expression.Parameter (typeof (Student), "s1"));
      var query = SelectTestQueryGenerator.CreateSimpleQuery (CreateQuerySource());
      MethodCallExpression methodCallExpression = CreateMethodCallExpression(query);
      List<MethodCallExpression> methodCallExpressions = new List<MethodCallExpression> ();
      methodCallExpressions.Add (methodCallExpression);

      return new SelectClause (CreateClause (), expression, methodCallExpressions);
    }

    public static MethodCallExpression CreateMethodCallExpression (IQueryable<Student> query)
    {
      var methodInfo = ParserUtility.GetMethod (() => query.Count ());
      return Expression.Call (methodInfo, query.Expression);
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
      IQueryExecutor executor = repository.StrictMock<IQueryExecutor>();
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
      IColumnSource table = fromClause.GetFromSource (StubDatabaseInfo.Instance);
      Column? column = DatabaseInfoUtility.GetColumn (StubDatabaseInfo.Instance, table, member);
      FieldSourcePath sourcePath = new FieldSourcePath (table, new SingleJoin[0]);
      FieldDescriptor fieldDescriptor = new FieldDescriptor (originalMember, sourcePath, column);
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

    public static Expression MakeExpression<T, R> (Expression<Func<T, R>> memberAccess)
    {
      return memberAccess.Body;
    }

    public static MemberInfo GetMember<T> (Expression<Func<T, object>> memberAccess)
    {
      Expression expression = memberAccess.Body;
      while (expression is UnaryExpression)
        expression = ((UnaryExpression) expression).Operand; // strip casts
      return ((MemberExpression) expression).Member;
    }

    public static MemberFromClause CreateMemberFromClause ()
    {
      var previousClause = CreateClause();
      var identifier = CreateParameterExpression();
      var bodyExpression = Expression.MakeMemberAccess (Expression.Constant (null, typeof (IndustrialSector)), typeof (IndustrialSector).GetProperty ("Students"));
      var fromExpression = Expression.Lambda (bodyExpression);
      var projectionExpression = CreateLambdaExpression();
      return new MemberFromClause (previousClause, identifier, fromExpression, projectionExpression);
    }

    public static ResultModifierClause CreateResultModifierClause (IClause previousClause, SelectClause selectClause)
    {
      var resultModifier = CreateMethodCallExpression (CreateQuerySource ());
      return new ResultModifierClause (previousClause, selectClause, resultModifier);
    }

    public static ResultModifierClause CreateResultModifierClause ()
    {
      return CreateResultModifierClause (CreateClause(), CreateSelectClause ());
    }
  }
}

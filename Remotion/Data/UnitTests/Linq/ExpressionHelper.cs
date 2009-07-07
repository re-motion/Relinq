// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Data.Linq;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Clauses.ResultOperators;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.Parsing.ExpressionTreeVisitors;
using Remotion.Data.Linq.Parsing.FieldResolving;
using Remotion.Data.Linq.Parsing.Structure;
using Rhino.Mocks;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Backend.DataObjectModel;

namespace Remotion.Data.UnitTests.Linq
{
  public static class ExpressionHelper
  {
    private static readonly IQueryExecutor s_executor = CreateExecutor();

    public static Expression CreateExpression ()
    {
      return CreateNewIntArrayExpression();
    }

    public static LambdaExpression CreateLambdaExpression ()
    {
      return Expression.Lambda (Expression.Constant (0));
    }

    public static Expression<Func<TSource, TResult>> CreateLambdaExpression<TSource, TResult> (Expression<Func<TSource, TResult>> expression)
    {
      return expression;
    }

    public static Expression<Func<TSource1, TSource2, TResult>> CreateLambdaExpression<TSource1, TSource2, TResult> (Expression<Func<TSource1, TSource2, TResult>> expression)
    {
      return expression;
    }

    public static Expression CreateNewIntArrayExpression ()
    {
      return Expression.NewArrayInit (typeof (int));
    }

    public static ParameterExpression CreateParameterExpression ()
    {
      return CreateParameterExpression ("i");
    }

    public static ParameterExpression CreateParameterExpression (string identifier)
    {
      return Expression.Parameter (typeof (int), identifier);
    }

    public static JoinClause CreateJoinClause ()
    {
      Expression inExpression = CreateExpression ();
      Expression onExpression = CreateExpression ();
      Expression equalityExpression = CreateExpression ();

      return new JoinClause ("x", typeof(Student), inExpression, onExpression, equalityExpression);
    }

    public static QueryModel CreateQueryModel (MainFromClause mainFromClause)
    {
      var selectClause = CreateSelectClause ();
      return new QueryModel (typeof (IQueryable<Student>), mainFromClause, selectClause);
    }

    public static QueryModel CreateQueryModel ()
    {
      return CreateQueryModel (CreateMainFromClause("s", typeof (Student), CreateQuerySource()));
    }


    public static MainFromClause CreateMainFromClause ()
    {
      IQueryable querySource = CreateQuerySource (); 
      return CreateMainFromClause("main", typeof (int), querySource);
    }

    public static MainFromClause CreateMainFromClause_Student ()
    {
      return CreateMainFromClause ("s", typeof (Student), CreateQuerySource());
    }

    public static MainFromClause CreateMainFromClause_Detail ()
    {
      return CreateMainFromClause ("sd", typeof (Student_Detail), CreateQuerySource_Detail ());
    }

    public static MainFromClause CreateMainFromClause_Detail_Detail ()
    {
      return CreateMainFromClause ("sdd", typeof (Student_Detail_Detail), CreateQuerySource_Detail_Detail());
    }

    public static AdditionalFromClause CreateAdditionalFromClause ()
    {
      return CreateAdditionalFromClause ("additional", typeof (int));
    }

    public static AdditionalFromClause CreateAdditionalFromClause (string itemName, Type itemType)
    {
      return new AdditionalFromClause (itemName, itemType, CreateExpression ());
    }

    public static GroupClause CreateGroupClause ()
    {
      Expression groupExpression = CreateExpression ();
      Expression byExpression = CreateExpression ();

      return new GroupClause (groupExpression, byExpression);
    }
    
    public static Ordering CreateOrdering ()
    {
      return new Ordering (CreateExpression (), OrderingDirection.Asc);
    }

    public static OrderByClause CreateOrderByClause()
    {
      return new OrderByClause ();
    }

    public static SelectClause CreateSelectClause ()
    {
      var selector = Expression.Constant (0);
      return new SelectClause (selector);
    }

    public static MethodCallExpression CreateMethodCallExpression (IQueryable<Student> query)
    {
      var methodInfo = ParserUtility.GetMethod (() => query.Count ());
      return Expression.Call (methodInfo, query.Expression);
    }

    public static MethodCallExpression CreateMethodCallExpression ()
    {
      return CreateMethodCallExpression (CreateQuerySource ());
    }

    public static WhereClause CreateWhereClause ()
    {
      var predicate = Expression.MakeBinary (ExpressionType.Equal, Expression.Constant (1), Expression.Constant (2));
      return new WhereClause (predicate);
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
      var repository = new MockRepository();
      var executor = repository.StrictMock<IQueryExecutor>();
      return executor;
    }

    public static object ExecuteLambda (LambdaExpression lambdaExpression, params object[] args)
    {
      return lambdaExpression.Compile().DynamicInvoke (args);
    }

    public static QueryModel ParseQuery<T> (IQueryable<T> query)
    {
      return ParseQuery (query.Expression);
    }

    public static QueryModel ParseQuery (Expression queryExpression)
    {
      var parser = new QueryParser ();
      return parser.GetParsedQuery (queryExpression);
    }

    public static FieldDescriptor CreateFieldDescriptor (JoinedTableContext joinedTableContext, FromClauseBase fromClause, MemberInfo member)
    {
      return CreateFieldDescriptor(joinedTableContext, fromClause, member, member);
    }

    public static FieldDescriptor CreateFieldDescriptor (JoinedTableContext joinedTableContext, FromClauseBase fromClause, MemberInfo member, MemberInfo originalMember)
    {
      IColumnSource table = joinedTableContext.GetColumnSource (fromClause);
      Column? column = DatabaseInfoUtility.GetColumn (StubDatabaseInfo.Instance, table, member);
      var sourcePath = new FieldSourcePath (table, new SingleJoin[0]);
      var fieldDescriptor = new FieldDescriptor (originalMember, sourcePath, column);
      return fieldDescriptor;
    }

    public static FieldSourcePath GetPathForNewTable ()
    {
      return new FieldSourcePath (new Table (), new SingleJoin[0]);
    }

    public static FieldSourcePath GetPathForNewTable (string name, string alias)
    {
      var table = new Table (name, alias);
      return GetPathForTable(table);
    }

    public static FieldSourcePath GetPathForTable (Table table)
    {
      return new FieldSourcePath (table, new SingleJoin[0]);
    }

    public static MainFromClause CreateMainFromClause (string itemName, Type itemType, IQueryable querySource)
    {
      return new MainFromClause (itemName, itemType, Expression.Constant (querySource));
    }

    public static Expression MakeExpression<TRet> (Expression<Func<TRet>> expression)
    {
      return expression.Body;
    }

    public static Expression MakeExpression<TArg, TRet> (Expression<Func<TArg, TRet>> expression)
    {
      return expression.Body;
    }

    public static MemberInfo GetMember<T> (Expression<Func<T, object>> memberAccess)
    {
      Expression expression = memberAccess.Body;
      while (expression is UnaryExpression)
        expression = ((UnaryExpression) expression).Operand; // strip casts
      return ((MemberExpression) expression).Member;
    }

    public static ResultOperatorBase CreateResultOperator ()
    {
      return new DistinctResultOperator ();
    }

    public static Expression Resolve<TParameter, TResult> (FromClauseBase fromClauseToReference, Expression<Func<TParameter, TResult>> expressionToBeResolved)
    {
      return ReplacingVisitor.Replace (expressionToBeResolved.Parameters[0], new QuerySourceReferenceExpression (fromClauseToReference), expressionToBeResolved.Body);
    }

    public static Expression Resolve<TParameter1, TParameter2, TResult> (FromClauseBase fromClauseToReference1, FromClauseBase fromClauseToReference2, Expression<Func<TParameter1, TParameter2, TResult>> expressionToBeResolved)
    {
      var result1 = ReplacingVisitor.Replace (expressionToBeResolved.Parameters[0], new QuerySourceReferenceExpression (fromClauseToReference1), expressionToBeResolved.Body);
      return ReplacingVisitor.Replace (expressionToBeResolved.Parameters[1], new QuerySourceReferenceExpression (fromClauseToReference2), result1);
    }

    public static Expression Resolve<TParameter1, TParameter2, TParameter3, TResult> (FromClauseBase fromClauseToReference1, FromClauseBase fromClauseToReference2, FromClauseBase fromClauseToReference3, Expression<Func<TParameter1, TParameter2, TParameter3, TResult>> expressionToBeResolved)
    {
      var result1 = ReplacingVisitor.Replace (expressionToBeResolved.Parameters[0], new QuerySourceReferenceExpression (fromClauseToReference1), expressionToBeResolved.Body);
      var result2 = ReplacingVisitor.Replace (expressionToBeResolved.Parameters[1], new QuerySourceReferenceExpression (fromClauseToReference2), result1);
      return ReplacingVisitor.Replace (expressionToBeResolved.Parameters[2], new QuerySourceReferenceExpression (fromClauseToReference3), result2);
    }
  }
}

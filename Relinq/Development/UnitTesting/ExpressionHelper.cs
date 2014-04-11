// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (c) rubicon IT GmbH, www.rubicon.eu
// 
// re-linq is free software; you can redistribute it and/or modify it under 
// the terms of the GNU Lesser General Public License as published by the 
// Free Software Foundation; either version 2.1 of the License, 
// or (at your option) any later version.
// 
// re-linq is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-linq; if not, see http://www.gnu.org/licenses.
// 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Parsing.ExpressionTreeVisitors;
using Remotion.Linq.Parsing.Structure;
using Remotion.Utilities;

namespace Remotion.Linq.Development.UnitTesting
{
  public static class ExpressionHelper
  {
    private static readonly IQueryExecutor s_executor = CreateExecutor();

    public static Expression CreateExpression ()
    {
      return CreateNewIntArrayExpression();
    }

    public static Expression CreateExpression (Type type)
    {
      ArgumentUtility.CheckNotNull ("type", type);

      object value = null;
      if (type.IsValueType)
        value = Activator.CreateInstance (type);

      return Expression.Constant (value, type);
    }

    public static LambdaExpression CreateLambdaExpression ()
    {
      return Expression.Lambda (Expression.Constant (0));
    }

    public static Expression<Func<TSource, TResult>> CreateLambdaExpression<TSource, TResult> (Expression<Func<TSource, TResult>> expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      return expression;
    }

    public static Expression<Func<TSource1, TSource2, TResult>> CreateLambdaExpression<TSource1, TSource2, TResult> (
        Expression<Func<TSource1, TSource2, TResult>> expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      return expression;
    }

    public static Expression<Func<TSource1, TSource2, TSource3, TResult>> CreateLambdaExpression<TSource1, TSource2, TSource3, TResult> (
        Expression<Func<TSource1, TSource2, TSource3, TResult>> expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      return expression;
    }

    public static Expression CreateNewIntArrayExpression ()
    {
      return Expression.NewArrayInit (typeof (int));
    }

    public static ParameterExpression CreateParameterExpression (string identifier = "i")
    {
      return Expression.Parameter (typeof (int), identifier);
    }

    public static JoinClause CreateJoinClause<T> ()
    {
      Expression innerSequence = CreateExpression();
      Expression outerKeySelector = CreateExpression();
      Expression innerKeySelector = CreateExpression();

      return new JoinClause ("x", typeof (T), innerSequence, outerKeySelector, innerKeySelector);
    }

    public static GroupJoinClause CreateGroupJoinClause<T> ()
    {
      return CreateGroupJoinClause<T> (CreateJoinClause<T>());
    }

    public static GroupJoinClause CreateGroupJoinClause<T> (JoinClause joinClause)
    {
      ArgumentUtility.CheckNotNull ("joinClause", joinClause);

      return new GroupJoinClause ("xs", typeof (IEnumerable<T>), joinClause);
    }

    public static QueryModel CreateQueryModel (MainFromClause mainFromClause)
    {
      ArgumentUtility.CheckNotNull ("mainFromClause", mainFromClause);

      var selectClause = new SelectClause (new QuerySourceReferenceExpression (mainFromClause));
      return new QueryModel (mainFromClause, selectClause);
    }

    public static QueryModel CreateQueryModel<T> ()
    {
      return CreateQueryModel (CreateMainFromClause_Int ("s", typeof (T), CreateQueryable<T>()));
    }

    public static QueryModel CreateQueryModel_Int ()
    {
      return CreateQueryModel (CreateMainFromClause_Int ("i", typeof (int), CreateIntQueryable()));
    }

    public static MainFromClause CreateMainFromClause_Int ()
    {
      IQueryable querySource = CreateIntQueryable();
      return CreateMainFromClause_Int ("main", typeof (int), querySource);
    }

    public static MainFromClause CreateMainFromClause<T> ()
    {
      return CreateMainFromClause_Int ("s", typeof (T), CreateQueryable<T>());
    }

    public static AdditionalFromClause CreateAdditionalFromClause ()
    {
      return CreateAdditionalFromClause ("additional", typeof (int));
    }

    public static AdditionalFromClause CreateAdditionalFromClause (string itemName, Type itemType)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("itemName", itemName);
      ArgumentUtility.CheckNotNull ("itemType", itemType);

      return new AdditionalFromClause (itemName, itemType, CreateExpression());
    }

    public static GroupResultOperator CreateGroupResultOperator ()
    {
      MainFromClause fromClause1 = CreateMainFromClause_Int ("i", typeof (int), CreateIntQueryable());
      MainFromClause fromClause2 = CreateMainFromClause_Int ("j", typeof (int), CreateIntQueryable());

      var keySelector = Resolve<int, string> (fromClause2, j => (j % 3).ToString());
      var elementSelector = Resolve<int, string> (fromClause1, i => i.ToString());

      return new GroupResultOperator ("groupings", keySelector, elementSelector);
    }

    public static UnionResultOperator CreateUnionResultOperator ()
    {
      return new UnionResultOperator ("union", typeof (int), Expression.Constant (new[] { 1, 2 }));
    }

    public static Ordering CreateOrdering ()
    {
      return new Ordering (CreateExpression(), OrderingDirection.Asc);
    }

    public static OrderByClause CreateOrderByClause ()
    {
      return new OrderByClause();
    }

    public static SelectClause CreateSelectClause ()
    {
      var selector = Expression.Constant (0);
      return new SelectClause (selector);
    }

    public static SelectClause CreateSelectClause (MainFromClause referencedClause)
    {
      ArgumentUtility.CheckNotNull ("referencedClause", referencedClause);

      return new SelectClause (new QuerySourceReferenceExpression (referencedClause));
    }

    public static MethodCallExpression CreateMethodCallExpression<T> (IQueryable<T> query)
    {
      ArgumentUtility.CheckNotNull ("query", query);

      var methodInfo = ReflectionUtility.GetMethod (() => query.Count());
      return Expression.Call (methodInfo, query.Expression);
    }

    public static MethodCallExpression CreateMethodCallExpression<T> ()
    {
      return CreateMethodCallExpression (CreateQueryable<T>());
    }

    public static WhereClause CreateWhereClause ()
    {
      var predicate = Expression.MakeBinary (ExpressionType.Equal, Expression.Constant (1), Expression.Constant (2));
      return new WhereClause (predicate);
    }

    public static IClause CreateClause ()
    {
      return CreateMainFromClause_Int();
    }

    public static IQueryable<int> CreateIntQueryable ()
    {
      return new TestQueryable<int> (QueryParser.CreateDefault(), s_executor);
    }

    public static IQueryable<T> CreateQueryable<T> ()
    {
      return CreateQueryable<T> (s_executor);
    }

    public static IQueryable<T> CreateQueryable<T> (IQueryExecutor executor)
    {
      ArgumentUtility.CheckNotNull ("executor", executor);

      return new TestQueryable<T> (QueryParser.CreateDefault(), executor);
    }

    public static IQueryExecutor CreateExecutor ()
    {
      return new StubQueryExecutor();
    }

    public static object ExecuteLambda (LambdaExpression lambdaExpression, params object[] args)
    {
      ArgumentUtility.CheckNotNull ("args", args);

      return lambdaExpression.Compile().DynamicInvoke (args);
    }

    public static QueryModel ParseQuery<T> (IQueryable<T> query)
    {
      ArgumentUtility.CheckNotNull ("query", query);

      return ParseQuery (query.Expression);
    }

    public static QueryModel ParseQuery (Expression queryExpression)
    {
      var parser = QueryParser.CreateDefault();
      return parser.GetParsedQuery (queryExpression);
    }

    public static MainFromClause CreateMainFromClause_Int (string itemName, Type itemType, IQueryable querySource)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("itemName", itemName);
      ArgumentUtility.CheckNotNull ("querySource", querySource);
      ArgumentUtility.CheckNotNull ("querySource", querySource);

      return new MainFromClause (itemName, itemType, Expression.Constant (querySource));
    }

    public static Expression MakeExpression<TRet> (Expression<Func<TRet>> expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      return expression.Body;
    }

    public static Expression MakeExpression<TArg, TRet> (Expression<Func<TArg, TRet>> expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      return expression.Body;
    }

    public static MemberInfo GetMember<T> (Expression<Func<T, object>> memberAccess)
    {
      ArgumentUtility.CheckNotNull ("memberAccess", memberAccess);

      Expression expression = memberAccess.Body;
      while (expression is UnaryExpression)
        expression = ((UnaryExpression) expression).Operand; // strip casts
      return ((MemberExpression) expression).Member;
    }

    public static ResultOperatorBase CreateResultOperator ()
    {
      return new DistinctResultOperator();
    }

    public static Expression Resolve<TParameter, TResult> (
        IQuerySource sourceToReference,
        Expression<Func<TParameter, TResult>> expressionToBeResolved)
    {
      ArgumentUtility.CheckNotNull ("sourceToReference", sourceToReference);
      ArgumentUtility.CheckNotNull ("expressionToBeResolved", expressionToBeResolved);

      return ReplacingExpressionTreeVisitor.Replace (
          expressionToBeResolved.Parameters[0],
          new QuerySourceReferenceExpression (sourceToReference),
          expressionToBeResolved.Body);
    }

    public static Expression Resolve<TParameter1, TParameter2, TResult> (
        IQuerySource sourceToReference1,
        IQuerySource sourceToReference2,
        Expression<Func<TParameter1, TParameter2, TResult>> expressionToBeResolved)
    {
      ArgumentUtility.CheckNotNull ("sourceToReference1", sourceToReference1);
      ArgumentUtility.CheckNotNull ("sourceToReference2", sourceToReference2);
      ArgumentUtility.CheckNotNull ("expressionToBeResolved", expressionToBeResolved);

      var expressionMapping = new Dictionary<Expression, Expression> (2)
                              {
                                  { expressionToBeResolved.Parameters[0], new QuerySourceReferenceExpression (sourceToReference1) },
                                  { expressionToBeResolved.Parameters[1], new QuerySourceReferenceExpression (sourceToReference2) }
                              };
      var result = MultiReplacingExpressionTreeVisitor.Replace (expressionMapping, expressionToBeResolved.Body);
      return result;
    }

    public static Expression Resolve<TParameter1, TParameter2, TParameter3, TResult> (
        IQuerySource sourceToReference1,
        IQuerySource sourceToReference2,
        IQuerySource sourceToReference3,
        Expression<Func<TParameter1, TParameter2, TParameter3, TResult>> expressionToBeResolved)
    {
      ArgumentUtility.CheckNotNull ("sourceToReference1", sourceToReference1);
      ArgumentUtility.CheckNotNull ("sourceToReference2", sourceToReference2);
      ArgumentUtility.CheckNotNull ("sourceToReference3", sourceToReference3);
      ArgumentUtility.CheckNotNull ("expressionToBeResolved", expressionToBeResolved);
      
      var expressionMapping = new Dictionary<Expression, Expression> (3)
                              {
                                  { expressionToBeResolved.Parameters[0], new QuerySourceReferenceExpression (sourceToReference1) },
                                  { expressionToBeResolved.Parameters[1], new QuerySourceReferenceExpression (sourceToReference2) },
                                  { expressionToBeResolved.Parameters[2], new QuerySourceReferenceExpression (sourceToReference3) },
                              };
      var result = MultiReplacingExpressionTreeVisitor.Replace (expressionMapping, expressionToBeResolved.Body);
      return result;
    }

    public static Expression ResolveLambdaParameter<TParameter1, TParameter2, TResult> (
        int parameterToResolveIndex,
        IQuerySource source,
        Expression<Func<TParameter1, TParameter2, TResult>> expressionToBeResolved)
    {
      ArgumentUtility.CheckNotNull ("source", source);
      ArgumentUtility.CheckNotNull ("expressionToBeResolved", expressionToBeResolved);
      
      var parameterToResolve = expressionToBeResolved.Parameters[parameterToResolveIndex];

      var resolvedBody = ReplacingExpressionTreeVisitor.Replace (
          parameterToResolve,
          new QuerySourceReferenceExpression (source),
          expressionToBeResolved.Body);

      var remainingParameters = new List<ParameterExpression> (expressionToBeResolved.Parameters);
      remainingParameters.Remove (parameterToResolve);

      return Expression.Lambda (resolvedBody, remainingParameters.ToArray());
    }
  }
}
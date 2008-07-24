/* Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh
 *
 * This program is free software: you can redistribute it and/or modify it under 
 * the terms of the re:motion license agreement in license.txt. If you did not 
 * receive it, please visit http://www.re-motion.org/licensing.
 * 
 * Unless otherwise provided, this software is distributed on an "AS IS" basis, 
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 */

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.Parsing.Structure;
using Remotion.Data.UnitTests.Linq.TestQueryGenerators;

namespace Remotion.Data.UnitTests.Linq.ParsingTest.StructureTest.SelectExpressionParserTest
{
  [TestFixture]
  public class GeneralSelectExpressionParserTest
  {

    [Test]
    public void Initialize ()
    {
      IQueryable<Student> querySource = ExpressionHelper.CreateQuerySource();
      MethodCallExpression expression = SelectTestQueryGenerator.CreateSimpleQuery_SelectExpression (querySource);
      new SelectExpressionParser ().Parse (new ParseResultCollector(expression), expression);
    }

    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "Expected one of 'Select', but found 'Where' at TestQueryable<Student>()"
        + ".Where(s => (s.Last = \"Garcia\")) in tree TestQueryable<Student>().Where(s => (s.Last = \"Garcia\")).")]
    public void Initialize_FromWrongExpression ()
    {
      MethodCallExpression expression = WhereTestQueryGenerator.CreateSimpleWhereQuery_WhereExpression (ExpressionHelper.CreateQuerySource ());
      new SelectExpressionParser ().Parse (new ParseResultCollector (expression), expression);
    }

    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "Expected Constant, MemberAccess, or Call expression for first argument of Select "
        + "expression, found Convert(null) (UnaryExpression).")]
    public void Initialize_FromWrongExpressionInWhereExpression ()
    {
      Expression nonCallExpression = Expression.Convert (Expression.Constant (null), typeof (IQueryable<Student>));
      // Get method Queryable.Select whose second argument is of type Expression<Func<TSource, TResult>>
      // (rather than Expression<Func<TSource, int, TResult>>)
      MethodInfo method = (from m in typeof (Queryable).GetMethods () where m.Name == "Select"
          && m.GetParameters()[1].ParameterType.GetGenericArguments()[0].GetGenericArguments().Length == 2 select m).First ();
      method = method.MakeGenericMethod (typeof (Student), typeof (Student));
      MethodCallExpression selectExpression = Expression.Call (method, nonCallExpression, Expression.Lambda (Expression.Constant (null, typeof (Student)), Expression.Parameter (typeof (Student), "student")));
      new SelectExpressionParser ().Parse (new ParseResultCollector (selectExpression), selectExpression);
    }

    [ExpectedException (typeof (ParserException), ExpectedMessage = "Expected no subqueries for Select expressions, found TestQueryable<Student>()"
        + ".Select(s => value(Remotion.Data.UnitTests.Linq.TestQueryGenerators.SelectTestQueryGenerator+<>c__DisplayClass4).source.Select(o => o)) "
        + "(MethodCallExpression).")]
    [Test]
    public void CheckSubQueryInSelect ()
    {
      MethodCallExpression selectExpression = SelectTestQueryGenerator.CreateSubQueryInSelct_SelectExpression (ExpressionHelper.CreateQuerySource ());
      new SelectExpressionParser().Parse (new ParseResultCollector (selectExpression), selectExpression);
    }

    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "Expected no subqueries for Select expressions, found "
        + "TestQueryable<Student>().Select(s => value(Remotion.Data.UnitTests.Linq.TestQueryGenerators.SelectTestQueryGenerator+<>c__DisplayClass6)"
        + ".source.Where(o => (o != null))) (MethodCallExpression).")]
    public void CheckSubQueryInSelect_WithoutExplicitSelect ()
    {
      MethodCallExpression selectExpression = 
          (MethodCallExpression) SelectTestQueryGenerator.CreateSubQueryInSelect_WithoutExplicitSelect (ExpressionHelper.CreateQuerySource ()).Expression;
      new SelectExpressionParser ().Parse (new ParseResultCollector (selectExpression), selectExpression);
      Assert.Fail();
    }
  }
}

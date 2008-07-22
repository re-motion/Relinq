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
using NUnit.Framework;
using Remotion.Data.Linq.Parsing;
using System.Reflection;
using Remotion.Data.Linq.Parsing.Structure;
using Remotion.Data.UnitTests.Linq.TestQueryGenerators;

namespace Remotion.Data.UnitTests.Linq.ParsingTest.StructureTest.WhereExpressionParserTest
{
  [TestFixture]
  public class GeneralWhereExpressionParserTest
  {
    [Test]
    public void Initialize ()
    {
      MethodCallExpression expression = WhereTestQueryGenerator.CreateSimpleWhereQuery_WhereExpression(ExpressionHelper.CreateQuerySource());
      new WhereExpressionParser ( true);
    }

    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "Expected one of 'Where', but found 'Select' at TestQueryable<Student>()"
        + ".Select(s => s) in tree TestQueryable<Student>().Select(s => s).")]
    public void Initialize_FromWrongExpression ()
    {
      MethodCallExpression expression = SelectTestQueryGenerator.CreateSimpleQuery_SelectExpression (ExpressionHelper.CreateQuerySource ());
      new WhereExpressionParser (true).Parse(new ParseResultCollector (expression), expression);
    }

    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "Expected Constant or Call expression for first argument of Where expression,"
        + " found Convert(null) (UnaryExpression).")]
    public void Initialize_FromWrongExpressionInWhereExpression ()
    {
      Expression nonCallExpression = Expression.Convert (Expression.Constant (null), typeof (IQueryable<Student>));
      // Get method Queryable.Where which has two arguments (rather than three)
      MethodInfo method = (from m in typeof (Queryable).GetMethods () where m.Name == "Where" && m.GetParameters ().Length == 2 select m).First();
      method = method.MakeGenericMethod (typeof (Student));
      MethodCallExpression whereExpression = Expression.Call (method, nonCallExpression, Expression.Lambda (Expression.Constant(true), Expression.Parameter(typeof (Student), "student")));
      new WhereExpressionParser (true).Parse(new ParseResultCollector (whereExpression), whereExpression);
    }
  }
}

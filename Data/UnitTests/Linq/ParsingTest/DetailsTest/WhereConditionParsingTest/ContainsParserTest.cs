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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using Remotion.Data.Linq;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Expressions;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.Parsing.Details;
using Remotion.Data.Linq.Parsing.Details.WhereConditionParsing;
using Remotion.Data.Linq.Parsing.FieldResolving;

namespace Remotion.Data.UnitTests.Linq.ParsingTest.DetailsTest.WhereConditionParsingTest
{
  [TestFixture]
  public class ContainsParserTest : DetailParserTestBase
  {
    [Test]
    public void ParseContainsWithSubQuery ()
    {
      IQueryable<Student> querySource = ExpressionHelper.CreateQuerySource ();
      Data.Linq.QueryModel queryModel = ExpressionHelper.CreateQueryModel ();
      SubQueryExpression subQueryExpression = new SubQueryExpression (queryModel);
      Student item = new Student ();
      ConstantExpression checkedExpression = Expression.Constant (item);
      ParameterExpression parameter = Expression.Parameter (typeof (Student), "s");
      MemberExpression memberAccess = Expression.MakeMemberAccess (parameter, typeof (Student).GetProperty ("First"));

      MethodInfo containsMethod = ParserUtility.GetMethod (() => querySource.Contains (item));
      MethodCallExpression methodCallExpression = Expression.Call (
          memberAccess,
          containsMethod,
          subQueryExpression,
          checkedExpression
          );

      ClauseFieldResolver resolver =
          new ClauseFieldResolver (StubDatabaseInfo.Instance, new WhereFieldAccessPolicy (StubDatabaseInfo.Instance));
      WhereConditionParserRegistry parserRegistry = new WhereConditionParserRegistry (StubDatabaseInfo.Instance);
      parserRegistry.RegisterParser (typeof (ConstantExpression), new ConstantExpressionParser (StubDatabaseInfo.Instance));
      parserRegistry.RegisterParser (typeof (ParameterExpression), new ParameterExpressionParser (resolver));
      parserRegistry.RegisterParser (typeof (MemberExpression), new MemberExpressionParser (resolver));

      ContainsParser parser = new ContainsParser (parserRegistry);

      ICriterion actualCriterion = parser.Parse (methodCallExpression, ParseContext);
      SubQuery expectedSubQuery = new SubQuery (queryModel, null);
      IValue expectedCheckedItem = new Constant (0);
      ICriterion expectedCriterion = new BinaryCondition (expectedSubQuery, expectedCheckedItem, BinaryCondition.ConditionKind.Contains);

      Assert.AreEqual (expectedCriterion, actualCriterion);
    }

    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "Expected SubQueryExpression for argument 0 of Contains method call, found "
        + "ConstantExpression (null).")]
    public void ParseContains_NoSubQueryExpression ()
    {
      IQueryable<Student> querySource = ExpressionHelper.CreateQuerySource ();
      Student item = new Student ();
      ConstantExpression checkedExpression = Expression.Constant (item);
      Data.Linq.QueryModel queryModel = ExpressionHelper.CreateQueryModel ();
      ClauseFieldResolver resolver =
          new ClauseFieldResolver (StubDatabaseInfo.Instance, new WhereFieldAccessPolicy (StubDatabaseInfo.Instance));

      MethodInfo containsMethod = ParserUtility.GetMethod (() => querySource.Contains (item));
      MethodCallExpression methodCallExpression = Expression.Call (
          null,
          containsMethod,
          Expression.Constant (null, typeof (IQueryable<Student>)),
          checkedExpression
          );

      WhereConditionParserRegistry parserRegistry = new WhereConditionParserRegistry (StubDatabaseInfo.Instance);
      parserRegistry.RegisterParser (typeof (ConstantExpression), new ConstantExpressionParser (StubDatabaseInfo.Instance));
      parserRegistry.RegisterParser (typeof (ParameterExpression), new ParameterExpressionParser (resolver));
      parserRegistry.RegisterParser (typeof (MemberExpression), new MemberExpressionParser (resolver));

      ContainsParser parser = new ContainsParser (parserRegistry);
      parser.Parse (methodCallExpression, ParseContext);
    }

    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "Expected Contains with expression for method call expression in where "
      + "condition, found Contains.")]
    public void ParseContains_NoArguments ()
    {
      MethodInfo containsMethod = typeof (ContainsParserTest).GetMethod ("Contains");
      MethodCallExpression methodCallExpression = Expression.Call (
          null,
          containsMethod
          );
      QueryModel queryModel = ExpressionHelper.CreateQueryModel ();
      ClauseFieldResolver resolver =
          new ClauseFieldResolver (StubDatabaseInfo.Instance, new WhereFieldAccessPolicy (StubDatabaseInfo.Instance));

      WhereConditionParserRegistry parserRegistry = new WhereConditionParserRegistry (StubDatabaseInfo.Instance);
      parserRegistry.RegisterParser (typeof (ConstantExpression), new ConstantExpressionParser (StubDatabaseInfo.Instance));
      parserRegistry.RegisterParser (typeof (ParameterExpression), new ParameterExpressionParser (resolver));
      parserRegistry.RegisterParser (typeof (MemberExpression), new MemberExpressionParser (resolver));

      ContainsParser parser = new ContainsParser (parserRegistry);
      
      parser.Parse (methodCallExpression, ParseContext);
    }

    public static bool Contains () { return true; }
  }
}

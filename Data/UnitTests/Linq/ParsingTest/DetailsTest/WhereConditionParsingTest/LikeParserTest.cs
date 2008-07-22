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
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.Parsing.Details;
using Remotion.Data.Linq.Parsing.Details.WhereConditionParsing;
using Remotion.Data.Linq.Parsing.FieldResolving;
using System.Collections.Generic;

namespace Remotion.Data.UnitTests.Linq.ParsingTest.DetailsTest.WhereConditionParsingTest
{
  [TestFixture]
  public class LikeParserTest : DetailParserTestBase
  {
    [Test]
    public void ParseStartsWith ()
    {
      var methodName = "StartsWith";
      var pattern = "Test%";
      CheckParsingOfLikeVariant (methodName, pattern);
    }

    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "Expected ConstantExpression for argument 0 of StartsWith method call, "
        + "found ParameterExpression (Test).")]
    public void ParseStartsWith_NoConstantExpression ()
    {
      var methodName = "StartsWith";
      CheckParsingOfLikeVariant_NoConstantExpression (methodName);
    }

    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "Expected at least 1 argument for StartsWith method call, found 0 arguments.")]
    public void ParseStartsWith_NoArguments ()
    {
      var methodName = "StartsWith";
      CheckParsingOfLikeVariant_NoArguments (methodName);
    }

    [Test]
    public void ParseEndsWith ()
    {
      var methodName = "EndsWith";
      var pattern = "%Test";
      CheckParsingOfLikeVariant (methodName, pattern);
    }

    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "Expected ConstantExpression for argument 0 of EndsWith method call, "
        + "found ParameterExpression (Test).")]
    public void ParseEndsWith_NoConstantExpression ()
    {
      var methodName = "EndsWith";
      CheckParsingOfLikeVariant_NoConstantExpression (methodName);
    }

    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "Expected at least 1 argument for EndsWith method call, found 0 arguments.")]
    public void ParseEndsWith_NoArguments ()
    {
      var methodName = "EndsWith";
      CheckParsingOfLikeVariant_NoArguments (methodName);
    }

    [Test]
    [ExpectedException (typeof (Remotion.Data.Linq.Parsing.ParserException), ExpectedMessage = "Expected StartsWith, EndsWith, Contains with no expression "
        + "for method call expression in where condition, found Equals.")]
    public void Parse_WithException ()
    {
      ParameterExpression parameter = Expression.Parameter (typeof (Student), "s");
      MemberExpression memberAccess = Expression.MakeMemberAccess (parameter, typeof (Student).GetProperty ("First"));
      MethodCallExpression methodCallExpression = Expression.Call (
          memberAccess,
          typeof (string).GetMethod ("Equals", new Type[] { typeof (object) }),
          Expression.Constant ("Test")
          );

      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause (parameter, ExpressionHelper.CreateQuerySource ());
      QueryModel queryModel = ExpressionHelper.CreateQueryModel (fromClause);
      ClauseFieldResolver resolver =
          new ClauseFieldResolver (StubDatabaseInfo.Instance, new WhereFieldAccessPolicy (StubDatabaseInfo.Instance));
      WhereConditionParserRegistry parserRegistry = new WhereConditionParserRegistry (StubDatabaseInfo.Instance);
      parserRegistry.RegisterParser (typeof (ConstantExpression), new ConstantExpressionParser (StubDatabaseInfo.Instance));
      parserRegistry.RegisterParser (typeof (ParameterExpression), new ParameterExpressionParser (resolver));
      parserRegistry.RegisterParser (typeof (MemberExpression), new MemberExpressionParser (resolver));

      LikeParser parser = new LikeParser (parserRegistry);
      parser.Parse (methodCallExpression, ParseContext);
    }

    private void CheckParsingOfLikeVariant (string methodName, string pattern)
    {
      WhereClause whereClause = ExpressionHelper.CreateWhereClause ();

      ParameterExpression parameter = Expression.Parameter (typeof (Student), "s");
      MemberExpression memberAccess = Expression.MakeMemberAccess (parameter, typeof (Student).GetProperty ("First"));

      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause (parameter, ExpressionHelper.CreateQuerySource ());
      Data.Linq.QueryModel queryModel = ExpressionHelper.CreateQueryModel (fromClause);
      ClauseFieldResolver resolver =
          new ClauseFieldResolver (StubDatabaseInfo.Instance, new WhereFieldAccessPolicy (StubDatabaseInfo.Instance));


      WhereConditionParserRegistry parserRegistry = new WhereConditionParserRegistry (StubDatabaseInfo.Instance);
      parserRegistry.RegisterParser (typeof (ConstantExpression), new ConstantExpressionParser (StubDatabaseInfo.Instance));
      parserRegistry.RegisterParser (typeof (ParameterExpression), new ParameterExpressionParser (resolver));
      parserRegistry.RegisterParser (typeof (MemberExpression), new MemberExpressionParser (resolver));

      MethodCallExpression methodCallExpression = Expression.Call (
          memberAccess,
          typeof (string).GetMethod (methodName, new Type[] { typeof (string) }),
          Expression.Constant ("Test")
          );

      LikeParser parser = new LikeParser (parserRegistry);

      ICriterion actualCriterion = parser.Parse (methodCallExpression, ParseContext);
      ICriterion expectedCriterion = new BinaryCondition (new Column (new Table ("studentTable", "s"), "FirstColumn"), new Constant (pattern), BinaryCondition.ConditionKind.Like);
      Assert.AreEqual (expectedCriterion, actualCriterion);
    }

    private void CheckParsingOfLikeVariant_NoConstantExpression (string methodName)
    {
      ParameterExpression parameter = Expression.Parameter (typeof (Student), "s");
      MemberExpression memberAccess = Expression.MakeMemberAccess (parameter, typeof (Student).GetProperty ("First"));

      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause (parameter, ExpressionHelper.CreateQuerySource ());
      QueryModel queryModel = ExpressionHelper.CreateQueryModel (fromClause);
      ClauseFieldResolver resolver =
          new ClauseFieldResolver (StubDatabaseInfo.Instance, new WhereFieldAccessPolicy (StubDatabaseInfo.Instance));


      WhereConditionParserRegistry parserRegistry = new WhereConditionParserRegistry (StubDatabaseInfo.Instance);
      parserRegistry.RegisterParser (typeof (ConstantExpression), new ConstantExpressionParser (StubDatabaseInfo.Instance));
      parserRegistry.RegisterParser (typeof (ParameterExpression), new ParameterExpressionParser (resolver));
      parserRegistry.RegisterParser (typeof (MemberExpression), new MemberExpressionParser (resolver));

      MethodCallExpression methodCallExpression = Expression.Call (
          memberAccess,
          typeof (string).GetMethod (methodName, new Type[] { typeof (string) }),
          Expression.Parameter (typeof (string), "Test")
          );

      LikeParser parser = new LikeParser (parserRegistry);
      parser.Parse (methodCallExpression, ParseContext);
    }

    private void CheckParsingOfLikeVariant_NoArguments (string methodName)
    {
      ParameterExpression parameter = Expression.Parameter (typeof (Student), "s");
      MemberExpression memberAccess = Expression.MakeMemberAccess (parameter, typeof (Student).GetProperty ("First"));

      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause (parameter, ExpressionHelper.CreateQuerySource ());
      QueryModel queryModel = ExpressionHelper.CreateQueryModel (fromClause);
      ClauseFieldResolver resolver =
          new ClauseFieldResolver (StubDatabaseInfo.Instance, new WhereFieldAccessPolicy (StubDatabaseInfo.Instance));


      WhereConditionParserRegistry parserRegistry = new WhereConditionParserRegistry (StubDatabaseInfo.Instance);
      parserRegistry.RegisterParser (typeof (ConstantExpression), new ConstantExpressionParser (StubDatabaseInfo.Instance));
      parserRegistry.RegisterParser (typeof (ParameterExpression), new ParameterExpressionParser (resolver));
      parserRegistry.RegisterParser (typeof (MemberExpression), new MemberExpressionParser (resolver));

      MethodCallExpression methodCallExpression = Expression.Call (
          memberAccess,
          typeof (LikeParserTest).GetMethod (methodName)
          );

      LikeParser parser = new LikeParser (parserRegistry);
      parser.Parse (methodCallExpression, ParseContext);
    }

    public static bool StartsWith () { return true; }
    public static bool EndsWith () { return true; }
  }
}

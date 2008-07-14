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
using System.Linq.Expressions;
using System.Reflection;
using System.Collections.Generic;
using NUnit.Framework;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing.Details;
using Remotion.Data.Linq.Parsing.Details.WhereConditionParsing;
using Remotion.Data.Linq.Parsing.FieldResolving;

namespace Remotion.Data.Linq.UnitTests.ParsingTest.DetailsTest.WhereConditionParsingTest
{
  [TestFixture]
  public class MethodCallExpressionParserTest : DetailParserTestBase
  {

    private ParameterExpression _parameter;
    private IColumnSource _fromSource;
    private MainFromClause _fromClause;
    private WhereConditionParserRegistry _parserRegistry;
    private ClauseFieldResolver _resolver;

    public override void SetUp ()
    {
      base.SetUp();

      _parameter = Expression.Parameter (typeof (Student), "s");
      _fromClause = ExpressionHelper.CreateMainFromClause (_parameter, ExpressionHelper.CreateQuerySource ());
      _fromSource = _fromClause.GetFromSource (StubDatabaseInfo.Instance);
      QueryModel = ExpressionHelper.CreateQueryModel (_fromClause);
      _resolver = new ClauseFieldResolver (StubDatabaseInfo.Instance, new WhereFieldAccessPolicy(StubDatabaseInfo.Instance));
      _parserRegistry = new WhereConditionParserRegistry (StubDatabaseInfo.Instance);
      _parserRegistry.RegisterParser (typeof(BinaryExpression), new BinaryExpressionParser (_parserRegistry));
      _parserRegistry.RegisterParser (typeof (MemberExpression), new MemberExpressionParser (_resolver));
      _parserRegistry.RegisterParser (typeof (ParameterExpression), new ParameterExpressionParser (_resolver));
      _parserRegistry.RegisterParser (typeof (ConstantExpression), new ConstantExpressionParser (StubDatabaseInfo.Instance));
    }

    [Test]
    public void CreateMethodCall_ToUpper_NoArguments ()
    {
      MemberExpression memberExpression = Expression.MakeMemberAccess (_parameter, typeof (Student).GetProperty ("First"));
      MethodInfo methodInfo = typeof (string).GetMethod ("ToUpper", new Type[] { });
      MethodCallExpression methodCallExpression = Expression.Call (memberExpression, methodInfo);

      //expected Result
      Column column = new Column (_fromSource, "FirstColumn");
      List<IEvaluation> c1 = new List<IEvaluation> { column }; //should be criterions
      MethodCall expected = new MethodCall (methodInfo, column, null);

      MethodCallExpressionParser methodCallExpressionParser = new MethodCallExpressionParser (_parserRegistry);

      //result
      ICriterion result = methodCallExpressionParser.Parse (methodCallExpression, ParseContext);

      //asserts
      Assert.IsEmpty (((MethodCall) result).EvaluationArguments);
      Assert.AreEqual (expected.EvaluationMethodInfo, ((MethodCall) result).EvaluationMethodInfo);
      Assert.AreEqual (expected.EvaluationParameter, ((MethodCall) result).EvaluationParameter);
    }

    [Test]
    public void CreateMethodCall_Remove_WithArguments ()
    {
      MemberExpression memberExpression = Expression.MakeMemberAccess (_parameter, typeof (Student).GetProperty ("First"));
      MethodInfo methodInfo = typeof (string).GetMethod ("Remove", new Type[] { typeof (int) });
      MethodCallExpression methodCallExpression = Expression.Call (memberExpression, methodInfo, Expression.Constant (5));

      //expected result
      Column column = new Column (_fromSource, "FirstColumn");
      List<IEvaluation> c1 = new List<IEvaluation> { column };
      Constant item = new Constant (5);
      List<IEvaluation> item1 = new List<IEvaluation> { item };
      List<IEvaluation> arguments = new List<IEvaluation> { item };
      MethodCall expected = new MethodCall (methodInfo, column, arguments);

      MethodCallExpressionParser methodCallExpressionParser =
        new MethodCallExpressionParser (_parserRegistry);

      //result
      ICriterion result = methodCallExpressionParser.Parse (methodCallExpression, ParseContext);

      //asserts
      Assert.IsNotEmpty (((MethodCall) result).EvaluationArguments);
      Assert.AreEqual (((MethodCall) result).EvaluationArguments, expected.EvaluationArguments);
      Assert.AreEqual (expected.EvaluationMethodInfo, ((MethodCall) result).EvaluationMethodInfo);
      Assert.AreEqual (expected.EvaluationParameter, ((MethodCall) result).EvaluationParameter);
    }
  }

  
}
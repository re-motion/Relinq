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
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.Parsing.Details;
using Remotion.Data.Linq.Parsing.Details.SelectProjectionParsing;
using Remotion.Data.Linq.Parsing.FieldResolving;

namespace Remotion.Data.UnitTests.Linq.ParsingTest.DetailsTest.SelectProjectionParsingTest
{
  [TestFixture]
  public class BinaryExpressionParserTest : DetailParserTestBase
  {
    private ClauseFieldResolver _resolver;
    private IColumnSource _fromSource;
    private ParameterExpression _parameter;
    private MainFromClause _fromClause;
    private MemberExpression _memberExpression1;
    private MemberExpression _memberExpression2;
    private MemberExpression _memberExpression3;
    private SelectProjectionParserRegistry _parserRegistry;

    public MemberExpression MemberExpression3
    {
      get { return _memberExpression3; }
    }

    public override void SetUp ()
    {
      base.SetUp();
      _parameter = Expression.Parameter (typeof (Student), "s");
      _fromClause = ExpressionHelper.CreateMainFromClause (_parameter, ExpressionHelper.CreateQuerySource ());
      QueryModel = ExpressionHelper.CreateQueryModel (_fromClause);
      _resolver = new ClauseFieldResolver (StubDatabaseInfo.Instance, new SelectFieldAccessPolicy ());
      _fromSource = _fromClause.GetFromSource (StubDatabaseInfo.Instance);
      _memberExpression1 = Expression.MakeMemberAccess (_parameter, typeof (Student).GetProperty ("First"));
      _memberExpression2 = Expression.MakeMemberAccess (_parameter, typeof (Student).GetProperty ("Last"));
      _memberExpression3 = Expression.MakeMemberAccess (_parameter, typeof (Student).GetProperty ("Last"));
      _parserRegistry = new SelectProjectionParserRegistry (StubDatabaseInfo.Instance, new ParseMode());
      _parserRegistry.RegisterParser (typeof(ConstantExpression), new ConstantExpressionParser (StubDatabaseInfo.Instance));
      _parserRegistry.RegisterParser (typeof(ParameterExpression), new ParameterExpressionParser (_resolver));
      _parserRegistry.RegisterParser (typeof(MemberExpression), new MemberExpressionParser (_resolver));
      _parserRegistry.RegisterParser (typeof(MethodCallExpression), new MethodCallExpressionParser (_parserRegistry));
    }

    [Test]
    public void ParseWithAdd ()
    {
      MethodInfo methodInfo = typeof(string).GetMethod("Concat",new[] { typeof(string), typeof(string) });
      BinaryExpression binaryExpression = Expression.Add (_memberExpression1, _memberExpression2, methodInfo);
      
      //expectedResult
      Column column1 = new Column (_fromSource, "FirstColumn");
      Column column2 = new Column (_fromSource, "LastColumn");
      BinaryEvaluation expectedResult = new BinaryEvaluation (column1, column2, BinaryEvaluation.EvaluationKind.Add);

      BinaryExpressionParser binaryExpressionParser = 
        new BinaryExpressionParser (_parserRegistry);

      //result
     IEvaluation result = binaryExpressionParser.Parse (binaryExpression, ParseContext);

     Assert.AreEqual (expectedResult, result);
    }

    [Test]
    public void CheckNodeTypeMap ()
    {
      MemberExpression memberExpression = Expression.MakeMemberAccess (_parameter, typeof (Student).GetProperty ("ID"));
      BinaryExpression binaryExpression1 = Expression.Add (memberExpression, memberExpression);
      BinaryExpression binaryExpression2 = Expression.Divide (memberExpression, memberExpression);
      BinaryExpression binaryExpression3 = Expression.Modulo (memberExpression, memberExpression);
      BinaryExpression binaryExpression4 = Expression.Multiply (memberExpression, memberExpression);
      BinaryExpression binaryExpression5 = Expression.Subtract (memberExpression, memberExpression);

      BinaryExpressionParser binaryExpressionParser = 
        new BinaryExpressionParser (_parserRegistry);

      BinaryEvaluation.EvaluationKind evaluationKind;
      Assert.IsTrue (binaryExpressionParser.NodeTypeMap.TryGetValue (binaryExpression1.NodeType, out evaluationKind));
      Assert.IsTrue (binaryExpressionParser.NodeTypeMap.TryGetValue (binaryExpression2.NodeType, out evaluationKind));
      Assert.IsTrue (binaryExpressionParser.NodeTypeMap.TryGetValue (binaryExpression3.NodeType, out evaluationKind));
      Assert.IsTrue (binaryExpressionParser.NodeTypeMap.TryGetValue (binaryExpression4.NodeType, out evaluationKind));
      Assert.IsTrue (binaryExpressionParser.NodeTypeMap.TryGetValue (binaryExpression5.NodeType, out evaluationKind));
    }

    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "Expected Add, Divide, Modulo, Multiply, Subtract for binary expression "
      +"in select projection, found AddChecked.")]
    public void CheckExceptionHandling ()
    {
      MemberExpression memberExpression = Expression.MakeMemberAccess (_parameter, typeof (Student).GetProperty ("ID"));
      BinaryExpression binaryExpression = Expression.AddChecked (memberExpression, memberExpression);
      BinaryExpressionParser binaryExpressionParser = new BinaryExpressionParser (_parserRegistry);
      binaryExpressionParser.Parse (binaryExpression, ParseContext);
    }
  }
}

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
using Remotion.Data.Linq;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.Parsing.Details;
using Remotion.Data.Linq.Parsing.Details.SelectProjectionParsing;
using Remotion.Data.UnitTests.Linq.TestQueryGenerators;
using System.Collections.Generic;

namespace Remotion.Data.UnitTests.Linq.ParsingTest.DetailsTest.SelectProjectionParsingTest
{
  [TestFixture]
  public class ResultModifierParserTest : DetailParserTestBase
  {
    [Test]
    public void MethodCallEvaluation_Count ()
    {
      var query = SelectTestQueryGenerator.CreateSimpleQuery (ExpressionHelper.CreateQuerySource ());
      QueryModel parsedQuery = ExpressionHelper.ParseQuery (query);
      var methodInfo = ParserUtility.GetMethod (() => Enumerable.Count (query));
      MethodCallExpression methodCallExpression = Expression.Call (methodInfo, query.Expression);

      SelectProjectionParserRegistry registry = new SelectProjectionParserRegistry (StubDatabaseInfo.Instance, new ParseMode ());
      
      ResultModifierParser parser = new ResultModifierParser (registry);
      IEvaluation result = parser.Parse (methodCallExpression, ParseContext);
      
      SourceMarkerEvaluation sourceMarkerEvaluation = new SourceMarkerEvaluation();
      var evaluationArguments = new List<IEvaluation> { sourceMarkerEvaluation };

      Assert.IsNull (((MethodCall) result).EvaluationParameter);
      Assert.AreEqual (methodInfo, ((MethodCall) result).EvaluationMethodInfo);
    }
  }
}
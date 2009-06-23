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
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.Parsing.Details;
using Remotion.Data.Linq.Parsing.Details.WhereConditionParsing;
using Remotion.Data.Linq.Parsing.FieldResolving;

namespace Remotion.Data.UnitTests.Linq.Parsing.Details.WhereConditionParsing
{
  [TestFixture]
  public class LikeParserTest : DetailParserTestBase
  {
    [Test]
    public void ParseStartsWith ()
    {
      string methodName = "StartsWith";
      string pattern = "Test%";
      CheckParsingOfLikeVariant (methodName, pattern);
    }

    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "Expected ConstantExpression for argument 0 of StartsWith method call, "
                                                                    + "found 'ParameterExpression (Test)'.")]
    public void ParseStartsWith_NoConstantExpression ()
    {
      string methodName = "StartsWith";
      CheckParsingOfLikeVariant_NoConstantExpression (methodName);
    }

    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "Expected at least 1 argument for StartsWith method call, found '0 arguments'.")]
    public void ParseStartsWith_NoArguments ()
    {
      string methodName = "StartsWith";
      CheckParsingOfLikeVariant_NoArguments (methodName);
    }

    [Test]
    public void ParseEndsWith ()
    {
      string methodName = "EndsWith";
      string pattern = "%Test";
      CheckParsingOfLikeVariant (methodName, pattern);
    }

    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "Expected ConstantExpression for argument 0 of EndsWith method call, "
                                                                    + "found 'ParameterExpression (Test)'.")]
    public void ParseEndsWith_NoConstantExpression ()
    {
      string methodName = "EndsWith";
      CheckParsingOfLikeVariant_NoConstantExpression (methodName);
    }

    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "Expected at least 1 argument for EndsWith method call, found '0 arguments'.")]
    public void ParseEndsWith_NoArguments ()
    {
      string methodName = "EndsWith";
      CheckParsingOfLikeVariant_NoArguments (methodName);
    }

    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "Expected StartsWith, EndsWith, Contains with no expression "
                                                                    + "for method call expression in where condition, found 'Equals'.")]
    public void Parse_WithException ()
    {
      MethodCallExpression methodCallExpression = Expression.Call (
          Student_First_Expression,
          typeof (string).GetMethod ("Equals", new[] { typeof (object) }),
          Expression.Constant ("Test")
          );

      var resolver =
          new ClauseFieldResolver (StubDatabaseInfo.Instance, new WhereFieldAccessPolicy (StubDatabaseInfo.Instance));
      var parserRegistry = new WhereConditionParserRegistry (StubDatabaseInfo.Instance);
      parserRegistry.RegisterParser (typeof (ConstantExpression), new ConstantExpressionParser (StubDatabaseInfo.Instance));
      parserRegistry.RegisterParser (typeof (MemberExpression), new MemberExpressionParser (resolver));

      var parser = new LikeParser (parserRegistry);
      parser.Parse (methodCallExpression, ParseContext);
    }

    private void CheckParsingOfLikeVariant (string methodName, string pattern)
    {
      var resolver =
          new ClauseFieldResolver (StubDatabaseInfo.Instance, new WhereFieldAccessPolicy (StubDatabaseInfo.Instance));

      var parserRegistry = new WhereConditionParserRegistry (StubDatabaseInfo.Instance);
      parserRegistry.RegisterParser (typeof (ConstantExpression), new ConstantExpressionParser (StubDatabaseInfo.Instance));
      parserRegistry.RegisterParser (typeof (MemberExpression), new MemberExpressionParser (resolver));

      MethodCallExpression methodCallExpression = Expression.Call (
          Student_First_Expression,
          typeof (string).GetMethod (methodName, new[] { typeof (string) }),
          Expression.Constant ("Test")
          );

      var parser = new LikeParser (parserRegistry);

      ICriterion actualCriterion = parser.Parse (methodCallExpression, ParseContext);
      ICriterion expectedCriterion = new BinaryCondition (
          new Column (new Table ("studentTable", "s"), "FirstColumn"), new Constant (pattern), BinaryCondition.ConditionKind.Like);
      Assert.That (actualCriterion, Is.EqualTo (expectedCriterion));
    }

    private void CheckParsingOfLikeVariant_NoConstantExpression (string methodName)
    {
      var resolver =
          new ClauseFieldResolver (StubDatabaseInfo.Instance, new WhereFieldAccessPolicy (StubDatabaseInfo.Instance));


      var parserRegistry = new WhereConditionParserRegistry (StubDatabaseInfo.Instance);
      parserRegistry.RegisterParser (typeof (ConstantExpression), new ConstantExpressionParser (StubDatabaseInfo.Instance));
      parserRegistry.RegisterParser (typeof (MemberExpression), new MemberExpressionParser (resolver));

      MethodCallExpression methodCallExpression = Expression.Call (
          Student_First_Expression,
          typeof (string).GetMethod (methodName, new[] { typeof (string) }),
          Expression.Parameter (typeof (string), "Test")
          );

      var parser = new LikeParser (parserRegistry);
      parser.Parse (methodCallExpression, ParseContext);
    }

    private void CheckParsingOfLikeVariant_NoArguments (string methodName)
    {
      var resolver =
          new ClauseFieldResolver (StubDatabaseInfo.Instance, new WhereFieldAccessPolicy (StubDatabaseInfo.Instance));

      var parserRegistry = new WhereConditionParserRegistry (StubDatabaseInfo.Instance);
      parserRegistry.RegisterParser (typeof (ConstantExpression), new ConstantExpressionParser (StubDatabaseInfo.Instance));
      parserRegistry.RegisterParser (typeof (MemberExpression), new MemberExpressionParser (resolver));

      MethodCallExpression methodCallExpression = Expression.Call (
          Student_First_Expression,
          typeof (LikeParserTest).GetMethod (methodName)
          );

      var parser = new LikeParser (parserRegistry);
      parser.Parse (methodCallExpression, ParseContext);
    }

    public static bool StartsWith ()
    {
      return true;
    }

    public static bool EndsWith ()
    {
      return true;
    }
  }
}

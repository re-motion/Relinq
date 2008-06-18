using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.Parsing.Details;
using Remotion.Data.Linq.Parsing.Details.WhereConditionParsing;
using Remotion.Data.Linq.Parsing.FieldResolving;
using System.Collections.Generic;

namespace Remotion.Data.Linq.UnitTests.ParsingTest.DetailsTest.WhereConditionParsingTest
{
  [TestFixture]
  public class LikeParserTest
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
      WhereClause whereClause = ExpressionHelper.CreateWhereClause ();

      ParameterExpression parameter = Expression.Parameter (typeof (Student), "s");
      MemberExpression memberAccess = Expression.MakeMemberAccess (parameter, typeof (Student).GetProperty ("First"));
      MethodCallExpression methodCallExpression = Expression.Call (
          memberAccess,
          typeof (string).GetMethod ("Equals", new Type[] { typeof (object) }),
          Expression.Constant ("Test")
          );

      ICriterion criterion = new Constant ("Test");

      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause (parameter, ExpressionHelper.CreateQuerySource ());
      QueryModel queryModel = ExpressionHelper.CreateQueryModel (fromClause);
      ClauseFieldResolver resolver =
          new ClauseFieldResolver (StubDatabaseInfo.Instance, new JoinedTableContext (), new WhereFieldAccessPolicy (StubDatabaseInfo.Instance));
      WhereConditionParserRegistry parserRegistry = new WhereConditionParserRegistry (queryModel, StubDatabaseInfo.Instance, new JoinedTableContext ());
      parserRegistry.RegisterParser (typeof (ConstantExpression), new ConstantExpressionParser (StubDatabaseInfo.Instance));
      parserRegistry.RegisterParser (typeof (ParameterExpression), new ParameterExpressionParser (queryModel, resolver));
      parserRegistry.RegisterParser (typeof (MemberExpression), new MemberExpressionParser (queryModel, resolver));

      LikeParser parser = new LikeParser (queryModel.GetExpressionTree (), parserRegistry);

      List<FieldDescriptor> fieldCollection = new List<FieldDescriptor> ();
      parser.Parse (methodCallExpression, fieldCollection);
    }

    private static void CheckParsingOfLikeVariant (string methodName, string pattern)
    {
      WhereClause whereClause = ExpressionHelper.CreateWhereClause ();

      ParameterExpression parameter = Expression.Parameter (typeof (Student), "s");
      MemberExpression memberAccess = Expression.MakeMemberAccess (parameter, typeof (Student).GetProperty ("First"));

      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause (parameter, ExpressionHelper.CreateQuerySource ());
      QueryModel queryModel = ExpressionHelper.CreateQueryModel (fromClause);
      ClauseFieldResolver resolver =
          new ClauseFieldResolver (StubDatabaseInfo.Instance, new JoinedTableContext (), new WhereFieldAccessPolicy (StubDatabaseInfo.Instance));


      WhereConditionParserRegistry parserRegistry = new WhereConditionParserRegistry (queryModel, StubDatabaseInfo.Instance, new JoinedTableContext ());
      parserRegistry.RegisterParser (typeof (ConstantExpression), new ConstantExpressionParser (StubDatabaseInfo.Instance));
      parserRegistry.RegisterParser (typeof (ParameterExpression), new ParameterExpressionParser (queryModel, resolver));
      parserRegistry.RegisterParser (typeof (MemberExpression), new MemberExpressionParser (queryModel, resolver));

      MethodCallExpression methodCallExpression = Expression.Call (
          memberAccess,
          typeof (string).GetMethod (methodName, new Type[] { typeof (string) }),
          Expression.Constant ("Test")
          );

      ICriterion criterion = new Constant ("Test");
      LikeParser parser = new LikeParser (queryModel.GetExpressionTree(), parserRegistry);

      List<FieldDescriptor> fieldCollection = new List<FieldDescriptor> ();
      ICriterion actualCriterion = parser.Parse (methodCallExpression, fieldCollection);
      ICriterion expectedCriterion = new BinaryCondition (new Column (new Table ("studentTable", "s"), "FirstColumn"), new Constant (pattern), BinaryCondition.ConditionKind.Like);
      Assert.AreEqual (expectedCriterion, actualCriterion);
    }

    private static void CheckParsingOfLikeVariant_NoConstantExpression (string methodName)
    {
      WhereClause whereClause = ExpressionHelper.CreateWhereClause ();

      ParameterExpression parameter = Expression.Parameter (typeof (Student), "s");
      MemberExpression memberAccess = Expression.MakeMemberAccess (parameter, typeof (Student).GetProperty ("First"));

      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause (parameter, ExpressionHelper.CreateQuerySource ());
      QueryModel queryModel = ExpressionHelper.CreateQueryModel (fromClause);
      ClauseFieldResolver resolver =
          new ClauseFieldResolver (StubDatabaseInfo.Instance, new JoinedTableContext (), new WhereFieldAccessPolicy (StubDatabaseInfo.Instance));


      WhereConditionParserRegistry parserRegistry = new WhereConditionParserRegistry (queryModel, StubDatabaseInfo.Instance, new JoinedTableContext ());
      parserRegistry.RegisterParser (typeof (ConstantExpression), new ConstantExpressionParser (StubDatabaseInfo.Instance));
      parserRegistry.RegisterParser (typeof (ParameterExpression), new ParameterExpressionParser (queryModel, resolver));
      parserRegistry.RegisterParser (typeof (MemberExpression), new MemberExpressionParser (queryModel, resolver));

      MethodCallExpression methodCallExpression = Expression.Call (
          memberAccess,
          typeof (string).GetMethod (methodName, new Type[] { typeof (string) }),
          Expression.Parameter (typeof (string), "Test")
          );

      LikeParser parser = new LikeParser (queryModel.GetExpressionTree (), parserRegistry);
      List<FieldDescriptor> fieldCollection = new List<FieldDescriptor> ();
      parser.Parse (methodCallExpression, fieldCollection);
    }

    private static void CheckParsingOfLikeVariant_NoArguments (string methodName)
    {
      WhereClause whereClause = ExpressionHelper.CreateWhereClause ();

      ParameterExpression parameter = Expression.Parameter (typeof (Student), "s");
      MemberExpression memberAccess = Expression.MakeMemberAccess (parameter, typeof (Student).GetProperty ("First"));

      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause (parameter, ExpressionHelper.CreateQuerySource ());
      QueryModel queryModel = ExpressionHelper.CreateQueryModel (fromClause);
      ClauseFieldResolver resolver =
          new ClauseFieldResolver (StubDatabaseInfo.Instance, new JoinedTableContext (), new WhereFieldAccessPolicy (StubDatabaseInfo.Instance));


      WhereConditionParserRegistry parserRegistry = new WhereConditionParserRegistry (queryModel, StubDatabaseInfo.Instance, new JoinedTableContext ());
      parserRegistry.RegisterParser (typeof (ConstantExpression), new ConstantExpressionParser (StubDatabaseInfo.Instance));
      parserRegistry.RegisterParser (typeof (ParameterExpression), new ParameterExpressionParser (queryModel, resolver));
      parserRegistry.RegisterParser (typeof (MemberExpression), new MemberExpressionParser (queryModel, resolver));

      MethodCallExpression methodCallExpression = Expression.Call (
          memberAccess,
          typeof (LikeParserTest).GetMethod (methodName)
          );

      LikeParser parser = new LikeParser (queryModel.GetExpressionTree (), parserRegistry);
      List<FieldDescriptor> fieldCollection = new List<FieldDescriptor> ();
      parser.Parse (methodCallExpression, fieldCollection);
    }

    public static bool StartsWith () { return true; }
    public static bool EndsWith () { return true; }
  }
}
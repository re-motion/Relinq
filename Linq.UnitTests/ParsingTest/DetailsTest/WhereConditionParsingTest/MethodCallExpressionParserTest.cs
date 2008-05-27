using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.Parsing.Details.WhereConditionParsing;
using System.Reflection;
using NUnit.Framework.SyntaxHelpers;

namespace Remotion.Data.Linq.UnitTests.ParsingTest.DetailsTest.WhereConditionParsingTest
{
  [TestFixture]
  public class MethodCallExpressionParserTest
  {
    [Test]
    public void ParseStartsWith()
    {
      var methodName = "StartsWith";
      var pattern = "Test%";
      CheckParsingOfLikeVariant(methodName, pattern);
    }

    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "Expected ConstantExpression for argument 0 of StartsWith method call, "
        + "found ParameterExpression (Test).")]
    public void ParseStartsWith_NoConstantExpression ()
    {
      var methodName = "StartsWith";
      CheckParsingOfLikeVariant_NoConstantExpression(methodName);
    }

    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "Expected 1 arguments for StartsWith method call, found 0 arguments.")]
    public void ParseStartsWith_NoArguments ()
    {
      var methodName = "StartsWith";
      CheckParsingOfLikeVariant_NoArguments(methodName);
    }

    [Test]
    public void ParseEndsWith ()
    {
      var methodName = "EndsWith";
      var pattern = "%Test";
      CheckParsingOfLikeVariant (methodName, pattern);
    }

    [Test]
    [Ignore]
    public void ParseContainsWithConstantExpression()
    {
      var methodName = "Contains";
      var pattern = "%Test";
      CheckParsingOfLikeVariant(methodName,pattern);
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
    [ExpectedException (typeof (ParserException), ExpectedMessage = "Expected 1 arguments for EndsWith method call, found 0 arguments.")]
    public void ParseEndsWith_NoArguments ()
    {
      var methodName = "EndsWith";
      CheckParsingOfLikeVariant_NoArguments (methodName);
    }

    public static bool StartsWith () { return true; }
    public static bool EndsWith () { return true; }


    [Test]
    [ExpectedException (typeof (Remotion.Data.Linq.Parsing.ParserException), ExpectedMessage = "Expected StartsWith, EndsWith, Contains for method " 
        + "call expression in where condition, found Equals.")]
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

      MethodCallExpressionParser parser = new MethodCallExpressionParser (whereClause, delegate (Expression expression)
      {
        Expression.Constant (5);
        return criterion;
      });

      parser.Parse (methodCallExpression);
    }

    [Test]
    public void ParseContainsWithSubQuery ()
    {
      WhereClause whereClause = ExpressionHelper.CreateWhereClause ();

      IQueryable<Student> querySource = ExpressionHelper.CreateQuerySource ();
      QueryModel queryModel = ExpressionHelper.CreateQueryModel ();
      SubQueryExpression subQueryExpression = new SubQueryExpression (queryModel);
      Student item = new Student();
      ConstantExpression checkedExpression = Expression.Constant (item);

      MethodInfo containsMethod = ExpressionHelper.GetMethod (() => querySource.Contains (item));
      MethodCallExpression methodCallExpression = Expression.Call (
          null,
          containsMethod,
          subQueryExpression,
          checkedExpression
          );

      int callCount = 0;

      MethodCallExpressionParser parser = new MethodCallExpressionParser (whereClause, delegate (Expression expression)
      {
        if (callCount == 0)
        {
          Assert.That (expression, Is.SameAs (checkedExpression));
          return new Constant ("Test");
        }
        else
          Assert.Fail ("Too many calls.");
        
        ++callCount;
        return null;
      });

      ICriterion actualCriterion = parser.Parse (methodCallExpression);
      SubQuery expectedSubQuery = new SubQuery (queryModel, null);
      IValue expectedCheckedItem = new Constant ("Test");
      ICriterion expectedCriterion = new BinaryCondition (expectedSubQuery, expectedCheckedItem, BinaryCondition.ConditionKind.Contains);

      Assert.AreEqual (expectedCriterion, actualCriterion);
    }

    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "Expected SubQueryExpression for argument 0 of Contains method call, found " 
        + "ConstantExpression (null).")]
    public void ParseContains_NoSubQueryExpression ()
    {
      WhereClause whereClause = ExpressionHelper.CreateWhereClause ();

      IQueryable<Student> querySource = ExpressionHelper.CreateQuerySource ();
      Student item = new Student ();
      ConstantExpression checkedExpression = Expression.Constant (item);

      MethodInfo containsMethod = ExpressionHelper.GetMethod (() => querySource.Contains (item));
      MethodCallExpression methodCallExpression = Expression.Call (
          null,
          containsMethod,
          Expression.Constant (null, typeof (IQueryable<Student>)),
          checkedExpression
          );

      MethodCallExpressionParser parser = new MethodCallExpressionParser (whereClause, delegate { return null; });
      parser.Parse (methodCallExpression);
    }

    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "Expected 2 arguments for Contains method call, found 0 arguments.")]
    public void ParseContains_NoArguments ()
    {
      WhereClause whereClause = ExpressionHelper.CreateWhereClause ();

      MethodInfo containsMethod = typeof (MethodCallExpressionParserTest).GetMethod ("Contains");
      MethodCallExpression methodCallExpression = Expression.Call (
          null,
          containsMethod
          );

      MethodCallExpressionParser parser = new MethodCallExpressionParser (whereClause, delegate { return null; });
      parser.Parse (methodCallExpression);
    }

    public static bool Contains () { return true; }

    private void CheckParsingOfLikeVariant (string methodName, string pattern)
    {
      WhereClause whereClause = ExpressionHelper.CreateWhereClause ();

      ParameterExpression parameter = Expression.Parameter (typeof (Student), "s");
      MemberExpression memberAccess = Expression.MakeMemberAccess (parameter, typeof (Student).GetProperty ("First"));
      MethodCallExpression methodCallExpression = Expression.Call (
          memberAccess,
          typeof (string).GetMethod (methodName, new Type[] { typeof (string) }),
          Expression.Constant ("Test")
          );

      ICriterion criterion = new Constant ("Test");
      MethodCallExpressionParser parser = new MethodCallExpressionParser (whereClause, delegate (Expression expression)
      {
        Expression.Constant (5);
        return criterion;
      });

      ICriterion actualCriterion = parser.Parse (methodCallExpression);
      ICriterion expectedCriterion = new BinaryCondition (new Constant ("Test"), new Constant (pattern), BinaryCondition.ConditionKind.Like);
      Assert.AreEqual (expectedCriterion, actualCriterion);
    }

    private void CheckParsingOfLikeVariant_NoConstantExpression (string methodName)
    {
      WhereClause whereClause = ExpressionHelper.CreateWhereClause ();

      ParameterExpression parameter = Expression.Parameter (typeof (Student), "s");
      MemberExpression memberAccess = Expression.MakeMemberAccess (parameter, typeof (Student).GetProperty ("First"));
      MethodCallExpression methodCallExpression = Expression.Call (
          memberAccess,
          typeof (string).GetMethod (methodName, new Type[] { typeof (string) }),
          Expression.Parameter (typeof (string), "Test")
          );

      MethodCallExpressionParser parser = new MethodCallExpressionParser (whereClause, delegate { return null; });
      parser.Parse (methodCallExpression);
    }

    private void CheckParsingOfLikeVariant_NoArguments (string methodName)
    {
      WhereClause whereClause = ExpressionHelper.CreateWhereClause ();

      ParameterExpression parameter = Expression.Parameter (typeof (Student), "s");
      MemberExpression memberAccess = Expression.MakeMemberAccess (parameter, typeof (Student).GetProperty ("First"));
      MethodCallExpression methodCallExpression = Expression.Call (
          memberAccess,
          typeof (MethodCallExpressionParserTest).GetMethod (methodName)
          );

      MethodCallExpressionParser parser = new MethodCallExpressionParser (whereClause, delegate { return null; });
      parser.Parse (methodCallExpression);
    }

  }
}
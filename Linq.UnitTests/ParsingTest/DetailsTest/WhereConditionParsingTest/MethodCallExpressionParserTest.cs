using System;
using System.Linq.Expressions;
using NUnit.Framework;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Data.Linq.Parsing.Details.WhereConditionParsing;

namespace Rubicon.Data.Linq.UnitTests.ParsingTest.DetailsTest.WhereConditionParsingTest
{
  [TestFixture]
  public class MethodCallExpressionParserTest
  {
    [Test]
    public void ParseStartsWith()
    {
      WhereClause whereClause = ExpressionHelper.CreateWhereClause();
      
      ParameterExpression parameter = Expression.Parameter (typeof (Student), "s");
      MemberExpression memberAccess = Expression.MakeMemberAccess (parameter, typeof (Student).GetProperty ("First"));
      MethodCallExpression methodCallExpression = Expression.Call (
          memberAccess,
          typeof (string).GetMethod ("StartsWith", new Type[] { typeof (string) }),
          Expression.Constant ("Test")
          );

      ICriterion criterion = new Constant ("Test");

      MethodCallExpressionParser parser = new MethodCallExpressionParser (whereClause,delegate (Expression expression)
      {
        Expression.Constant (5);
        return criterion;
      });

      ICriterion actualCriterion = parser.Parse (methodCallExpression);

      ICriterion expectedCriterion = new BinaryCondition (new Constant ("Test"),new Constant ("Test%"), BinaryCondition.ConditionKind.Like);

      Assert.AreEqual (expectedCriterion, actualCriterion);
    }

    [Test]
    public void ParseEndsWith ()
    {
      WhereClause whereClause = ExpressionHelper.CreateWhereClause ();

      ParameterExpression parameter = Expression.Parameter (typeof (Student), "s");
      MemberExpression memberAccess = Expression.MakeMemberAccess (parameter, typeof (Student).GetProperty ("First"));
      MethodCallExpression methodCallExpression = Expression.Call (
          memberAccess,
          typeof (string).GetMethod ("EndsWith", new Type[] { typeof (string) }),
          Expression.Constant ("Test")
          );

      ICriterion criterion = new Constant ("Test");

      MethodCallExpressionParser parser = new MethodCallExpressionParser (whereClause, delegate (Expression expression)
      {
        Expression.Constant (5);
        return criterion;
      });

      ICriterion actualCriterion = parser.Parse (methodCallExpression);

      ICriterion expectedCriterion = new BinaryCondition (new Constant ("Test"), new Constant ("%Test"), BinaryCondition.ConditionKind.Like);

      Assert.AreEqual (expectedCriterion, actualCriterion);
    }

    [Test]
    [ExpectedException (typeof (Rubicon.Data.Linq.Parsing.ParserException), ExpectedMessage = "Expected StartsWith, EndsWith for method call "
      +"expression in where condition, found Equals.")]
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

  }
}
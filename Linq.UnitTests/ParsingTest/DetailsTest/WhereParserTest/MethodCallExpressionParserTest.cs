using System;
using System.Linq.Expressions;
using NUnit.Framework;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Data.Linq.Parsing.Details.WhereParser;

namespace Rubicon.Data.Linq.UnitTests.ParsingTest.DetailsTest.WhereParserTest
{
  [TestFixture]
  public class MethodCallExpressionParserTest
  {
    [Test]
    public void Parse()
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
  }
}
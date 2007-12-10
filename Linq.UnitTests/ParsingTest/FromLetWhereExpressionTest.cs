using System;
using System.Linq.Expressions;
using NUnit.Framework;
using Rubicon.Data.Linq.Parsing;

namespace Rubicon.Data.Linq.UnitTests.ParsingTest
{
  [TestFixture]
  public class FromLetWhereExpressionTest
  {
    [Test]
    public void FromExpression_Initialize()
    {
      Expression expression = ExpressionHelper.CreateExpression();
      ParameterExpression identifier = ExpressionHelper.CreateParameterExpression();
      FromExpression fromExpression = new FromExpression(expression,identifier);
      Assert.AreSame (expression, fromExpression.Expression);
      Assert.AreSame (identifier, fromExpression.Identifier);
    }

    [Test]
    public void WhereExpression_Initialize ()
    {
      LambdaExpression expression = ExpressionHelper.CreateLambdaExpression ();
      WhereExpression whereExpression = new WhereExpression (expression);
      Assert.AreSame (expression, whereExpression.Expression);

    }
    
  }
}
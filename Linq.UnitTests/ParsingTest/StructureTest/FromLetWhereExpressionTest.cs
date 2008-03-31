using System;
using System.Linq.Expressions;
using NUnit.Framework;
using Rubicon.Data.Linq.Parsing.Structure;

namespace Rubicon.Data.Linq.UnitTests.ParsingTest.StructureTest
{
  [TestFixture]
  public class FromLetWhereExpressionTest
  {
    [Test]
    public void FromExpression_Initialize()
    {
      Expression expression = ExpressionHelper.CreateExpression();
      ParameterExpression identifier = ExpressionHelper.CreateParameterExpression();
      FromExpressionData fromExpressionData = new FromExpressionData(expression,identifier);
      Assert.AreSame (expression, fromExpressionData.Expression);
      Assert.AreSame (identifier, fromExpressionData.Identifier);
    }

    [Test]
    public void WhereExpression_Initialize ()
    {
      LambdaExpression expression = ExpressionHelper.CreateLambdaExpression ();
      WhereExpressionData whereExpressionData = new WhereExpressionData (expression);
      Assert.AreSame (expression, whereExpressionData.Expression);

    }
    
  }
}
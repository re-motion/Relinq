using System;
using System.Linq.Expressions;
using NUnit.Framework;

namespace Rubicon.Data.DomainObjects.Linq.UnitTests
{
  [TestFixture]
  public class JoinClauseTest
  {
    [Test]
    public void Intialize_WithIDAndInExprAndOnExprAndEqExpr()
    {
      ParameterExpression identifier = ExpressionHelper.CreateParameterExpression ();
      Expression inExpression = ExpressionHelper.CreateExpression ();
      Expression onExpression = ExpressionHelper.CreateExpression ();
      Expression equalityExpression = ExpressionHelper.CreateExpression ();

      JoinClause joinClause = new JoinClause (identifier, inExpression, onExpression, equalityExpression);
      
      Assert.AreSame (identifier, joinClause.Identifier);
      Assert.AreSame (inExpression, joinClause.InExpression);
      Assert.AreSame (onExpression, joinClause.OnExpression);
      Assert.AreSame (equalityExpression, joinClause.EqualityExpression);
      
      Assert.IsNull (joinClause.IntoIdentifier);
    }

    [Test]
    public void Intialize_WithIDAndInExprAndOnExprAndEqExprAndIntoID ()
    {
      ParameterExpression identifier = ExpressionHelper.CreateParameterExpression ();
      Expression inExpression = ExpressionHelper.CreateExpression ();
      Expression onExpression = ExpressionHelper.CreateExpression ();
      Expression equalityExpression = ExpressionHelper.CreateExpression ();
      ParameterExpression intoIdentifier = ExpressionHelper.CreateParameterExpression ();

      JoinClause joinClause = new JoinClause (identifier, inExpression, onExpression, equalityExpression, intoIdentifier);

      Assert.AreSame (identifier, joinClause.Identifier);
      Assert.AreSame (inExpression, joinClause.InExpression);
      Assert.AreSame (onExpression, joinClause.OnExpression);
      Assert.AreSame (equalityExpression, joinClause.EqualityExpression);
      Assert.AreSame (equalityExpression, joinClause.EqualityExpression);
      Assert.AreSame (intoIdentifier, joinClause.IntoIdentifier);

    }
  }
}
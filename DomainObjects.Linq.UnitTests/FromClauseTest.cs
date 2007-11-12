using System;
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

namespace Rubicon.Data.DomainObjects.Linq.UnitTests
{
  [TestFixture]
  public class FromClauseTest
  {
    [Test]
    public void Initialize_WithIDAndExpression()
    {
      ParameterExpression id = ExpressionHelper.CreateParameterExpression ();
      Expression expression = ExpressionHelper.CreateExpression();
      
      FromClause fromClause = new FromClause (id, expression);
      
      Assert.AreSame (id, fromClause.Identifier);
      Assert.AreSame (expression, fromClause.Expression);
      
      Assert.That (fromClause.JoinClauses, Is.Empty);
      Assert.AreEqual (0, fromClause.JoinClauseCount);
    }

    [Test]
    [ExpectedException (typeof (ArgumentNullException))]
    public void Initialize_ThrowsOnNullID ()
    {
      Expression expression = ExpressionHelper.CreateExpression();
      new FromClause (null, expression);
    }

    [Test]
    [ExpectedException (typeof (ArgumentNullException))]
    public void Initialize_ThrowsOnNullExpression ()
    {
      ParameterExpression id = ExpressionHelper.CreateParameterExpression();
      new FromClause (id, null);
    }

    [Test]
    public void AddJoinClause()
    {
      ParameterExpression id = ExpressionHelper.CreateParameterExpression ();
      Expression expression = ExpressionHelper.CreateExpression ();

      FromClause fromClause = new FromClause (id, expression);

      JoinClause joinClause1 = CreateJoinClause();
      JoinClause joinClause2 = CreateJoinClause();

      fromClause.Add (joinClause1);
      fromClause.Add (joinClause2);

      Assert.That (fromClause.JoinClauses, Is.EqualTo (new object[] { joinClause1, joinClause2 }));
      Assert.AreEqual (2, fromClause.JoinClauseCount);
    }

    [Test]
    public void ImplementInterface()
    {
      ParameterExpression id = ExpressionHelper.CreateParameterExpression ();
      Expression expression = ExpressionHelper.CreateExpression ();

      FromClause fromClause = new FromClause (id, expression);

      Assert.IsInstanceOfType (typeof (IFromLetWhereClause), fromClause);
    }


    private JoinClause CreateJoinClause ()
    {
      ParameterExpression identifier = ExpressionHelper.CreateParameterExpression ();
      Expression inExpression = ExpressionHelper.CreateExpression ();
      Expression onExpression = ExpressionHelper.CreateExpression ();
      Expression equalityExpression = ExpressionHelper.CreateExpression ();

      return new JoinClause (identifier, inExpression, onExpression, equalityExpression);
    }

  }
}
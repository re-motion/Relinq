using System;
using System.Linq.Expressions;
using NUnit.Framework;

namespace Rubicon.Data.DomainObjects.Linq.UnitTests
{
  [TestFixture]
  public class GroupClauseTest
  {
    [Test]
    public void InitializeWithGroupExpressionAndByExpression()
    {
      Expression groupExpression = ExpressionHelper.CreateExpression();
      Expression byExpression = ExpressionHelper.CreateExpression();

      GroupClause groupClause = new GroupClause (groupExpression, byExpression);

      Assert.AreSame (groupExpression, groupClause.GroupExpression);
      Assert.AreSame (byExpression, groupClause.ByExpression);
    }

    [Test]
    public void ImplementInterface ()
    {
      Expression groupExpression = ExpressionHelper.CreateExpression ();
      Expression byExpression = ExpressionHelper.CreateExpression ();

      GroupClause groupClause = new GroupClause (groupExpression, byExpression);

      Assert.IsInstanceOfType (typeof (ISelectGroupClause), groupClause);
    }
  }
}
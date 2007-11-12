using System;
using System.Linq.Expressions;
using NUnit.Framework;

namespace Rubicon.Data.DomainObjects.Linq.UnitTests
{
  [TestFixture]
  public class LetClauseTest
  {
    [Test]
    public void IntitalizeWithIDAndExpression()
    {
      ParameterExpression identifier = ExpressionHelper.CreateParameterExpression();
      Expression expression = ExpressionHelper.CreateExpression();

      LetClause letClause = new LetClause(identifier,expression);
    }

    [Test]
    public void ImplementInterface()
    {
      ParameterExpression identifier = ExpressionHelper.CreateParameterExpression ();
      Expression expression = ExpressionHelper.CreateExpression ();

      LetClause letClause = new LetClause (identifier, expression);

      Assert.IsInstanceOfType (typeof (IFromLetWhereClause), letClause);
    }
  }
}
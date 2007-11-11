using System;
using System.Linq.Expressions;
using NUnit.Framework;

namespace Rubicon.Data.DomainObjects.Linq.UnitTests
{
  [TestFixture]
  public class SelectClauseTest
  {
    [Test]
    public void InitializeWithExpression ()
    {
      Expression expression = ExpressionHelper.CreateExpression ();

      SelectClause selectClause = new SelectClause (expression);

      Assert.AreSame (expression, selectClause.Expression);

    }
  }
}
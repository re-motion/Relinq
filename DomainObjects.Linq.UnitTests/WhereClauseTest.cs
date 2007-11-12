using System;
using System.Linq.Expressions;
using NUnit.Framework;

namespace Rubicon.Data.DomainObjects.Linq.UnitTests
{
  [TestFixture]
  public class WhereClauseTest
  {
   [Test] 
   public void InitializeWithboolExpression()
   {
     Expression boolExpression = ExpressionHelper.CreateExpression();
     WhereClause whereClause = new WhereClause(boolExpression);
   }

    [Test]
    public void ImplementInterface()
    {
      Expression boolExpression = ExpressionHelper.CreateExpression ();
      WhereClause whereClause = new WhereClause (boolExpression);
      Assert.IsInstanceOfType (typeof (IFromLetWhereClause), whereClause);
    }
  }
}
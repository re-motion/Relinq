using System;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Rubicon.Data.Linq.Clauses;

namespace Rubicon.Data.Linq.UnitTests.ClausesTest
{
  [TestFixture]
  public class FromClauseBaseTest
  {
    [Test]
    public void AddJoinClause()
    {
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause();

      JoinClause joinClause1 = ExpressionHelper.CreateJoinClause();
      JoinClause joinClause2 = ExpressionHelper.CreateJoinClause();

      fromClause.Add (joinClause1);
      fromClause.Add (joinClause2);

      Assert.That (fromClause.JoinClauses, Is.EqualTo (new object[] { joinClause1, joinClause2 }));
      Assert.AreEqual (2, fromClause.JoinClauses.Count);
    }


    [Test]
    public void FromClause_ImplementsIQueryElement()
    {
      Assert.IsTrue (typeof (IQueryElement).IsAssignableFrom (typeof (FromClauseBase)));
    }

    
  }
}
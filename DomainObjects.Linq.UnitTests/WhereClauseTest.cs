using System;
using System.Linq.Expressions;
using NUnit.Framework;
using Rhino.Mocks;

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

     Assert.AreSame (boolExpression, whereClause.BoolExpression);
   }

    [Test]
    public void ImplementInterface()
    {
      WhereClause whereClause = CreateWhereClause();
      Assert.IsInstanceOfType (typeof (IFromLetWhereClause), whereClause);
    }
    
    [Test]
    public void WhereClause_ImplementIQueryElement()
    {
      WhereClause whereClause = CreateWhereClause();
      Assert.IsInstanceOfType (typeof (IQueryElement), whereClause);
    }

    [Test]
    public void Accept()
    {
      WhereClause whereClause = CreateWhereClause();

      MockRepository repository = new MockRepository ();
      IQueryVisitor visitorMock = repository.CreateMock<IQueryVisitor> ();

      visitorMock.VisitWhereClause (whereClause);

      repository.ReplayAll();

      whereClause.Accept (visitorMock);

      repository.VerifyAll();
    }

    private WhereClause CreateWhereClause ()
    {
      Expression boolExpression = ExpressionHelper.CreateExpression ();
      return new WhereClause (boolExpression);
    }
  }
}
using System;
using System.Linq.Expressions;
using NUnit.Framework;
using Rhino.Mocks;

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

    [Test]
    public void SelectClause_ImplementISelectGroupClause()
    {
      SelectClause selectClause = CreateSelectClause();

      Assert.IsInstanceOfType (typeof(ISelectGroupClause),selectClause);
    }
        
    [Test]
    public void SelectClause_ImplementIQueryElement()
    {
      SelectClause selectClause = CreateSelectClause();
      Assert.IsInstanceOfType (typeof (IQueryElement), selectClause);
    }


    [Test]
    public void Accept()
    {
      SelectClause selectClause = CreateSelectClause();

      MockRepository repository = new MockRepository();
      IQueryVisitor visitorMock = repository.CreateMock<IQueryVisitor>();

      visitorMock.VisitSelectClause (selectClause);

      repository.ReplayAll();

      selectClause.Accept (visitorMock);

      repository.VerifyAll();

    }

    private SelectClause CreateSelectClause ()
    {
      Expression expression = ExpressionHelper.CreateExpression ();

      return new SelectClause (expression);
    }


  }
}
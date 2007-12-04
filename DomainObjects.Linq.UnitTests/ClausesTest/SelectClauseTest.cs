using System;
using System.Linq.Expressions;
using NUnit.Framework;
using Rhino.Mocks;
using Rubicon.Data.DomainObjects.Linq.Clauses;
using System.Linq;
using Rubicon.Data.DomainObjects.Linq.UnitTests.Parsing;
using Rubicon.Data.DomainObjects.Linq.Parsing;

namespace Rubicon.Data.DomainObjects.Linq.UnitTests.ClausesTest
{
  [TestFixture]
  public class SelectClauseTest
  {
    [Test]
    public void InitializeWithExpression ()
    {
      LambdaExpression expression = ExpressionHelper.CreateLambdaExpression ();
      IClause clause = ExpressionHelper.CreateClause();

      SelectClause selectClause = new SelectClause (clause, expression);
      Assert.AreSame (clause, selectClause.PreviousClause);
      Assert.AreEqual (expression, selectClause.ProjectionExpression);
    }

    [Test]
    public void InitializeWithoutExpression ()
    {
      SelectClause selectClause = new SelectClause (ExpressionHelper.CreateClause(),null);
      Assert.IsNull (selectClause.ProjectionExpression);
    }


    [Test]
    public void SelectClause_ImplementISelectGroupClause()
    {
      SelectClause selectClause = ExpressionHelper.CreateSelectClause();

      Assert.IsInstanceOfType (typeof(ISelectGroupClause),selectClause);
    }
        
    [Test]
    public void SelectClause_ImplementIQueryElement()
    {
      SelectClause selectClause = ExpressionHelper.CreateSelectClause();
      Assert.IsInstanceOfType (typeof (IQueryElement), selectClause);
    }

    [Test]
    public void Accept()
    {
      SelectClause selectClause = ExpressionHelper.CreateSelectClause();

      MockRepository repository = new MockRepository();
      IQueryVisitor visitorMock = repository.CreateMock<IQueryVisitor>();

      visitorMock.VisitSelectClause (selectClause);

      repository.ReplayAll();

      selectClause.Accept (visitorMock);

      repository.VerifyAll();

    }
  }
}
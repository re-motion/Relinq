using System;
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Rhino.Mocks;
using Rubicon.Data.DomainObjects.Linq.Clauses;

namespace Rubicon.Data.DomainObjects.Linq.UnitTests.ClausesTest
{
  [TestFixture]
  public class SelectClauseTest
  {
    [Test]
    public void InitializeWithExpression ()
    {
      LambdaExpression expression = ExpressionHelper.CreateLambdaExpression ();

      SelectClause selectClause = new SelectClause (new LambdaExpression[] {expression});

      Assert.That (selectClause.ProjectionExpressions, Is.EqualTo (new object[] {expression}));
    }

    [Test]
    public void InitializeWithMultipleExpression ()
    {
      LambdaExpression expression1 = ExpressionHelper.CreateLambdaExpression ();
      LambdaExpression expression2 = ExpressionHelper.CreateLambdaExpression ();

      SelectClause selectClause = new SelectClause (new LambdaExpression[] { expression1, expression2 });

      Assert.That (selectClause.ProjectionExpressions, Is.EqualTo (new object[] { expression1, expression2 }));
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
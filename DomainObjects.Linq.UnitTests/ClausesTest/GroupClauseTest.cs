using System;
using System.Linq.Expressions;
using NUnit.Framework;
using Rhino.Mocks;
using Rubicon.Data.DomainObjects.Linq.Clauses;

namespace Rubicon.Data.DomainObjects.Linq.UnitTests.ClausesTest
{
  [TestFixture]
  public class GroupClauseTest
  {
    [Test]
    public void InitializeWithGroupExpressionAndByExpression()
    {
      Expression groupExpression = ExpressionHelper.CreateExpression();
      Expression byExpression = ExpressionHelper.CreateExpression();

      IClause clause = ExpressionHelper.CreateClause();
      GroupClause groupClause = new GroupClause (clause, groupExpression, byExpression);

      Assert.AreSame (clause, groupClause.PreviousClause);
      Assert.AreSame (groupExpression, groupClause.GroupExpression);
      Assert.AreSame (byExpression, groupClause.ByExpression);
    }

    [Test]
    public void GroupClause_ImplementISelectGroupClause ()
    {
      GroupClause groupClause = ExpressionHelper.CreateGroupClause();

      Assert.IsInstanceOfType (typeof (ISelectGroupClause), groupClause);
    }

    [Test]
    public void GroupClause_ImplementIQueryElement()
    {
      GroupClause groupClause = ExpressionHelper.CreateGroupClause ();
      Assert.IsInstanceOfType (typeof (IQueryElement), groupClause);
    }

    [Test]
    public void Accept()
    {
      GroupClause groupClause = ExpressionHelper.CreateGroupClause ();

      MockRepository repository = new MockRepository();
      IQueryVisitor visitorMock = repository.CreateMock<IQueryVisitor>();
      visitorMock.VisitGroupClause (groupClause);

      repository.ReplayAll();

      groupClause.Accept (visitorMock);

      repository.VerifyAll();
    }
  }
}
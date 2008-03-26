using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Rhino.Mocks;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Data.Linq.DataObjectModel;

namespace Rubicon.Data.Linq.UnitTests.ClausesTest
{
  [TestFixture]
  public class MainFromClauseTest
  {
    [Test]
    public void Initialize_WithIDAndConstant ()
    {
      ParameterExpression id = ExpressionHelper.CreateParameterExpression ();
      IQueryable querySource = ExpressionHelper.CreateQuerySource ();

      ConstantExpression constantExpression = Expression.Constant (querySource);
      MainFromClause fromClause = new MainFromClause (id, constantExpression);

      Assert.AreSame (id, fromClause.Identifier);
      Assert.AreSame (constantExpression, fromClause.QuerySource);

      Assert.That (fromClause.JoinClauses, Is.Empty);
      Assert.AreEqual (0, fromClause.JoinClauses.Count);

      Assert.IsNull (fromClause.PreviousClause);
    }

    [Test]
    public void Initialize_WithExpression ()
    {
      ParameterExpression id = ExpressionHelper.CreateParameterExpression ();
      IQueryable querySource = ExpressionHelper.CreateQuerySource ();
      var anonymous = new {source = querySource};
      MemberExpression sourceExpression = Expression.MakeMemberAccess (Expression.Constant (anonymous), anonymous.GetType().GetProperty ("source"));

      MainFromClause fromClause = new MainFromClause (id, sourceExpression);

      Assert.AreSame (sourceExpression, fromClause.QuerySource);
    }

    [Test]
    public void Accept ()
    {
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause ();

      MockRepository repository = new MockRepository ();
      IQueryVisitor visitorMock = repository.CreateMock<IQueryVisitor> ();

      visitorMock.VisitMainFromClause (fromClause);

      repository.ReplayAll ();

      fromClause.Accept (visitorMock);

      repository.VerifyAll ();

    }

    [Test]
    public void GetQueriedEntityType ()
    {
      IQueryable<Student> querySource = ExpressionHelper.CreateQuerySource();
      MainFromClause fromClause = new MainFromClause (ExpressionHelper.CreateParameterExpression(), querySource);
      Assert.AreSame (typeof (TestQueryable<Student>), fromClause.GetQueriedEntityType());
    }
  }
}
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
    public void Initialize_WithIDAndExpression ()
    {
      ParameterExpression id = ExpressionHelper.CreateParameterExpression ();
      IQueryable querySource = ExpressionHelper.CreateQuerySource ();

      MainFromClause fromClause = new MainFromClause (id, querySource);

      Assert.AreSame (id, fromClause.Identifier);
      Assert.AreSame (querySource, fromClause.QuerySource);

      Assert.That (fromClause.JoinClauses, Is.Empty);
      Assert.AreEqual (0, fromClause.JoinClauseCount);

      Assert.IsNull (fromClause.PreviousClause);
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
    public void GetQuerySourceType ()
    {
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause ();
      Assert.AreSame (fromClause.QuerySource.GetType(), fromClause.GetQuerySourceType());
    }

    [Test]
    public void GetTable ()
    {
      ParameterExpression id = Expression.Parameter (typeof (Student), "s1");
      IQueryable querySource = ExpressionHelper.CreateQuerySource ();

      MainFromClause fromClause = new MainFromClause (id, querySource);
      Assert.AreEqual (new Table ("sourceTable", "s1"), fromClause.GetTable (StubDatabaseInfo.Instance));
    }

    [Test]
    public void Resolve_ParameterAccess_Succeeds ()
    {
      ParameterExpression identifier = Expression.Parameter (typeof (Student), "fromIdentifier1");
      MainFromClause fromClause = new MainFromClause (identifier, ExpressionHelper.CreateQuerySource ());

      FieldDescriptor fieldDescriptor = fromClause.ResolveField (StubDatabaseInfo.Instance, identifier, identifier);
      Assert.AreEqual (new Column (new Table ("sourceTable", "fromIdentifier1"), "*"), fieldDescriptor.Column);
      Assert.AreSame (fromClause, fieldDescriptor.FromClause);
    }

    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "There is no from clause defining identifier 'fromIdentifier5', which is used in "
        + "expression 'fromIdentifier5'.")]
    public void Resolve_ParameterAccess_InvalidParameter ()
    {
      ParameterExpression identifier = Expression.Parameter (typeof (Student), "fromIdentifier1");
      MainFromClause fromClause = new MainFromClause (identifier, ExpressionHelper.CreateQuerySource ());

      ParameterExpression identifier2 = Expression.Parameter (typeof (Student), "fromIdentifier5");
      fromClause.ResolveField (StubDatabaseInfo.Instance, identifier2, identifier2);
    }

    [Test]
    public void Resolve_SimpleMemberAccess_Succeeds ()
    {
      ParameterExpression identifier = Expression.Parameter (typeof (Student), "fromIdentifier1");
      MainFromClause fromClause = new MainFromClause (identifier, ExpressionHelper.CreateQuerySource());

      Expression fieldExpression = Expression.MakeMemberAccess (Expression.Parameter (typeof (Student), "fromIdentifier1"),
          typeof (Student).GetProperty ("First"));
      FieldDescriptor fieldDescriptor = fromClause.ResolveField (StubDatabaseInfo.Instance, fieldExpression, fieldExpression);
      Assert.AreEqual (new Column (new Table ("sourceTable", "fromIdentifier1"), "FirstColumn"), fieldDescriptor.Column);
      Assert.AreSame (fromClause, fieldDescriptor.FromClause);
    }

    [Test]
    [Ignore ("TODO")]
    public void Resolve_JoinMemberAccess_Succeeds ()
    {
      ParameterExpression identifier = Expression.Parameter (typeof (Student_Detail), "fromIdentifier1");
      MainFromClause fromClause = new MainFromClause (identifier, ExpressionHelper.CreateQuerySourceDetail ());

      Expression studentExpression = Expression.MakeMemberAccess (
          Expression.Parameter (typeof (Student_Detail), "fromIdentifier1"),
          typeof (Student_Detail).GetProperty ("Student"));
      Expression fieldExpression = Expression.MakeMemberAccess (
          studentExpression,
          typeof (Student).GetProperty ("First"));

      FieldDescriptor fieldDescriptor = fromClause.ResolveField (StubDatabaseInfo.Instance, fieldExpression, fieldExpression);
      Assert.Fail ("Not implemented yet");
    }

    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "There is no from clause defining identifier 'fromIdentifier1', "
        + "which is used in expression 'fromIdentifier1.First'.")]
    public void Resolve_SimpleMemberAccess_InvalidName ()
    {
      ParameterExpression identifier = Expression.Parameter (typeof (Student), "fzlbf");
      MainFromClause fromClause = new MainFromClause (identifier, ExpressionHelper.CreateQuerySource ());

      Expression fieldExpression = Expression.MakeMemberAccess (Expression.Parameter (typeof (Student), "fromIdentifier1"),
          typeof (Student).GetProperty ("First"));
      fromClause.ResolveField (StubDatabaseInfo.Instance, fieldExpression, fieldExpression);
    }

    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "The from identifier 'fromIdentifier1' has a different type "
        + "(System.Int32) than expected in expression 'fromIdentifier1.First' (Rubicon.Data.Linq.UnitTests.Student).")]
    public void Resolve_SimpleMemberAccess_InvalidType ()
    {
      ParameterExpression identifier = Expression.Parameter (typeof (int), "fromIdentifier1");
      MainFromClause fromClause = new MainFromClause (identifier, ExpressionHelper.CreateQuerySource ());

      Expression fieldExpression = Expression.MakeMemberAccess (Expression.Parameter (typeof (Student), "fromIdentifier1"),
          typeof (Student).GetProperty ("First"));
      fromClause.ResolveField (StubDatabaseInfo.Instance, fieldExpression, fieldExpression);
    }

    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "Expected MemberExpression or ParameterExpression for resolving field access, "
        + "found ConstantExpression (null).")]
    public void Resolve_SimpleMemberAccess_InvalidOuterExpression ()
    {
      ParameterExpression identifier = Expression.Parameter (typeof (Student), "fromIdentifier1");
      MainFromClause fromClause = new MainFromClause (identifier, ExpressionHelper.CreateQuerySource ());

      Expression fieldExpression = Expression.Constant (null, typeof (Student));
      fromClause.ResolveField (StubDatabaseInfo.Instance, fieldExpression, fieldExpression);
    }

    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "Expected ParameterExpression for resolving field access, found "
        + "ConstantExpression (null).")]
    public void Resolve_SimpleMemberAccess_InvalidInnerExpression ()
    {
      ParameterExpression identifier = Expression.Parameter (typeof (Student), "fromIdentifier1");
      MainFromClause fromClause = new MainFromClause (identifier, ExpressionHelper.CreateQuerySource ());

      Expression fieldExpression = Expression.MakeMemberAccess (Expression.Constant (null, typeof (Student)),
          typeof (Student).GetProperty ("First"));
      fromClause.ResolveField (StubDatabaseInfo.Instance, fieldExpression, fieldExpression);
    }

    [Test]
    public void Resolve_SimpleMemberAccess_InvalidField ()
    {
      ParameterExpression identifier = Expression.Parameter (typeof (Student), "fromIdentifier1");
      MainFromClause fromClause = new MainFromClause (identifier, ExpressionHelper.CreateQuerySource ());

      Expression fieldExpression = Expression.MakeMemberAccess (Expression.Parameter (typeof (Student), "fromIdentifier1"),
          typeof (Student).GetProperty ("NonDBProperty"));
      FieldDescriptor fieldDescriptor = fromClause.ResolveField (StubDatabaseInfo.Instance, fieldExpression, fieldExpression);
      Table table = fromClause.GetTable (StubDatabaseInfo.Instance);
      Assert.AreEqual (new FieldDescriptor(typeof (Student).GetProperty ("NonDBProperty"), fromClause, table, null), fieldDescriptor);
    }
  }
}
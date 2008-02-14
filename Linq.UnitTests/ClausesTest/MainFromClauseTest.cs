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
      Assert.AreEqual (0, fromClause.JoinClauses.Count);

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
    [ExpectedException (typeof (FieldAccessResolveException), ExpectedMessage = "This from clause can only resolve field accesses for parameters "
        + "called 'fromIdentifier1', but a parameter called 'fromIdentifier5' was given.")]
    public void Resolve_ParameterAccess_InvalidParameterName ()
    {
      ParameterExpression identifier = Expression.Parameter (typeof (Student), "fromIdentifier1");
      MainFromClause fromClause = new MainFromClause (identifier, ExpressionHelper.CreateQuerySource ());

      ParameterExpression identifier2 = Expression.Parameter (typeof (Student), "fromIdentifier5");
      fromClause.ResolveField (StubDatabaseInfo.Instance, identifier2, identifier2);
    }

    [Test]
    [ExpectedException (typeof (FieldAccessResolveException), ExpectedMessage = "This from clause can only resolve field accesses for parameters of "
        + "type 'Rubicon.Data.Linq.UnitTests.Student', but a parameter of type 'System.String' was given.")]
    public void Resolve_ParameterAccess_InvalidParameterType ()
    {
      ParameterExpression identifier = Expression.Parameter (typeof (Student), "fromIdentifier1");
      MainFromClause fromClause = new MainFromClause (identifier, ExpressionHelper.CreateQuerySource ());

      ParameterExpression identifier2 = Expression.Parameter (typeof (string), "fromIdentifier1");
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
    [ExpectedException (typeof (FieldAccessResolveException), ExpectedMessage = "This from clause can only resolve field accesses for parameters "
        + "called 'fzlbf', but a parameter called 'fromIdentifier1' was given.")]
    public void Resolve_SimpleMemberAccess_InvalidName ()
    {
      ParameterExpression identifier = Expression.Parameter (typeof (Student), "fzlbf");
      MainFromClause fromClause = new MainFromClause (identifier, ExpressionHelper.CreateQuerySource ());

      Expression fieldExpression = Expression.MakeMemberAccess (Expression.Parameter (typeof (Student), "fromIdentifier1"),
          typeof (Student).GetProperty ("First"));
      fromClause.ResolveField (StubDatabaseInfo.Instance, fieldExpression, fieldExpression);
    }

    [Test]
    [ExpectedException (typeof (FieldAccessResolveException), ExpectedMessage = "This from clause can only resolve field accesses for parameters of "
        + "type 'Rubicon.Data.Linq.UnitTests.Student', but a parameter of type 'Rubicon.Data.Linq.UnitTests.Student_Detail' was given.")]
    public void Resolve_SimpleMemberAccess_InvalidType ()
    {
      ParameterExpression identifier = Expression.Parameter (typeof (Student), "fromIdentifier1");
      MainFromClause fromClause = new MainFromClause (identifier, ExpressionHelper.CreateQuerySource ());

      Expression fieldExpression = Expression.MakeMemberAccess (Expression.Parameter (typeof (Student_Detail), "fromIdentifier1"),
          typeof (Student_Detail).GetProperty ("Student"));
      fromClause.ResolveField (StubDatabaseInfo.Instance, fieldExpression, fieldExpression);
    }

    [Test]
    public void Resolve_Join ()
    {
      ParameterExpression identifier = Expression.Parameter (typeof (Student_Detail), "sd");
      MainFromClause fromClause = new MainFromClause (identifier, ExpressionHelper.CreateQuerySourceDetail());

      Expression fieldExpression = 
          Expression.MakeMemberAccess (
              Expression.MakeMemberAccess (Expression.Parameter (typeof (Student_Detail), "sd"),
              typeof (Student_Detail).GetProperty ("Student")),
          typeof (Student).GetProperty ("First"));
      
      FieldDescriptor fieldDescriptor = fromClause.ResolveField (StubDatabaseInfo.Instance, fieldExpression, fieldExpression);

      Assert.AreEqual (new Column (new Table ("sourceTable", null), "FirstColumn"), fieldDescriptor.Column);
      Assert.AreSame (fromClause, fieldDescriptor.FromClause);
      Assert.AreEqual (typeof (Student).GetProperty ("First"), fieldDescriptor.Member);

      Table expectedLeftSide = new Table("sourceTable", null);
      Table expectedRightSide = fromClause.GetTable (StubDatabaseInfo.Instance);
      Join expectedJoin = new Join (
          expectedLeftSide,
          expectedRightSide,
          new Column (expectedLeftSide, "Student_FK"),
          new Column (expectedRightSide, "Student_Detail_PK"));

      Assert.AreEqual (expectedJoin, fieldDescriptor.SourcePath);
    }

    [Test]
    [ExpectedException (typeof (FieldAccessResolveException), ExpectedMessage = "The member 'Rubicon.Data.Linq.UnitTests.Student.First' does not "
        + "identify a relation.")]
    public void Resolve_Join_InvalidMember ()
    {
      ParameterExpression identifier = Expression.Parameter (typeof (Student), "s");
      MainFromClause fromClause = new MainFromClause (identifier, ExpressionHelper.CreateQuerySourceDetail ());

      Expression fieldExpression =
          Expression.MakeMemberAccess (
              Expression.MakeMemberAccess (Expression.Parameter (typeof (Student), "s"),
              typeof (Student).GetProperty ("First")),
          typeof (string).GetProperty ("Length"));

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
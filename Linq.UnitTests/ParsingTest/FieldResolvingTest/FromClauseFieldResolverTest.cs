using System;
using System.Linq.Expressions;
using NUnit.Framework;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Data.Linq.Parsing.FieldResolving;

namespace Rubicon.Data.Linq.UnitTests.ParsingTest.FieldResolvingTest
{
  [TestFixture]
  public class FromClauseFieldResolverTest
  {
    private JoinedTableContext _context;

    [SetUp]
    public void SetUp ()
    {
      _context = new JoinedTableContext();
    }

    [Test]
    public void Resolve_ParameterAccess_Succeeds ()
    {
      ParameterExpression identifier = Expression.Parameter (typeof (Student), "fromIdentifier1");
      MainFromClause fromClause = new MainFromClause (identifier, ExpressionHelper.CreateQuerySource());

      FieldDescriptor fieldDescriptor =
          new FromClauseFieldResolver (fromClause).ResolveField (StubDatabaseInfo.Instance, _context, identifier, identifier);
      Assert.AreEqual (new Column (new Table ("studentTable", "fromIdentifier1"), "*"), fieldDescriptor.Column);
      Assert.AreSame (fromClause, fieldDescriptor.FromClause);
    }

    [Test]
    [ExpectedException (typeof (FieldAccessResolveException),
        ExpectedMessage = "This from clause can only resolve field accesses for parameters "
            + "called 'fromIdentifier1', but a parameter called 'fromIdentifier5' was given.")]
    public void Resolve_ParameterAccess_InvalidParameterName ()
    {
      ParameterExpression identifier = Expression.Parameter (typeof (Student), "fromIdentifier1");
      MainFromClause fromClause = new MainFromClause (identifier, ExpressionHelper.CreateQuerySource());

      ParameterExpression identifier2 = Expression.Parameter (typeof (Student), "fromIdentifier5");
      new FromClauseFieldResolver (fromClause).ResolveField (StubDatabaseInfo.Instance, _context, identifier2, identifier2);
    }

    [Test]
    [ExpectedException (typeof (FieldAccessResolveException),
        ExpectedMessage = "This from clause can only resolve field accesses for parameters of "
            + "type 'Rubicon.Data.Linq.UnitTests.Student', but a parameter of type 'System.String' was given.")]
    public void Resolve_ParameterAccess_InvalidParameterType ()
    {
      ParameterExpression identifier = Expression.Parameter (typeof (Student), "fromIdentifier1");
      MainFromClause fromClause = new MainFromClause (identifier, ExpressionHelper.CreateQuerySource());

      ParameterExpression identifier2 = Expression.Parameter (typeof (string), "fromIdentifier1");
      new FromClauseFieldResolver (fromClause).ResolveField (StubDatabaseInfo.Instance, _context, identifier2, identifier2);
    }

    [Test]
    public void Resolve_SimpleMemberAccess_Succeeds ()
    {
      ParameterExpression identifier = Expression.Parameter (typeof (Student), "fromIdentifier1");
      MainFromClause fromClause = new MainFromClause (identifier, ExpressionHelper.CreateQuerySource());

      Expression fieldExpression = Expression.MakeMemberAccess (Expression.Parameter (typeof (Student), "fromIdentifier1"),
          typeof (Student).GetProperty ("First"));
      FieldDescriptor fieldDescriptor =
          new FromClauseFieldResolver (fromClause).ResolveField (StubDatabaseInfo.Instance, _context, fieldExpression, fieldExpression);
      Assert.AreEqual (new Column (new Table ("studentTable", "fromIdentifier1"), "FirstColumn"), fieldDescriptor.Column);
      Assert.AreSame (fromClause, fieldDescriptor.FromClause);
    }

    [Test]
    [ExpectedException (typeof (FieldAccessResolveException), ExpectedMessage = "This from clause can only resolve field accesses for parameters "
        + "called 'fzlbf', but a parameter called 'fromIdentifier1' was given.")]
    public void Resolve_SimpleMemberAccess_InvalidName ()
    {
      ParameterExpression identifier = Expression.Parameter (typeof (Student), "fzlbf");
      MainFromClause fromClause = new MainFromClause (identifier, ExpressionHelper.CreateQuerySource());

      Expression fieldExpression = Expression.MakeMemberAccess (Expression.Parameter (typeof (Student), "fromIdentifier1"),
          typeof (Student).GetProperty ("First"));
      new FromClauseFieldResolver (fromClause).ResolveField (StubDatabaseInfo.Instance, _context, fieldExpression, fieldExpression);
    }

    [Test]
    [ExpectedException (typeof (FieldAccessResolveException), ExpectedMessage = "This from clause can only resolve field accesses for parameters of "
        + "type 'Rubicon.Data.Linq.UnitTests.Student', but a parameter of type 'Rubicon.Data.Linq.UnitTests.Student_Detail' was given.")]
    public void Resolve_SimpleMemberAccess_InvalidType ()
    {
      ParameterExpression identifier = Expression.Parameter (typeof (Student), "fromIdentifier1");
      MainFromClause fromClause = new MainFromClause (identifier, ExpressionHelper.CreateQuerySource());

      Expression fieldExpression = Expression.MakeMemberAccess (Expression.Parameter (typeof (Student_Detail), "fromIdentifier1"),
          typeof (Student_Detail).GetProperty ("Student"));
      new FromClauseFieldResolver (fromClause).ResolveField (StubDatabaseInfo.Instance, _context, fieldExpression, fieldExpression);
    }

    [Test]
    public void Resolve_Join ()
    {
      // sd.Student.First
      ParameterExpression identifier = Expression.Parameter (typeof (Student_Detail), "sd");
      MainFromClause fromClause = new MainFromClause (identifier, ExpressionHelper.CreateQuerySource_Detail());

      Expression fieldExpression =
          Expression.MakeMemberAccess (
              Expression.MakeMemberAccess (Expression.Parameter (typeof (Student_Detail), "sd"),
                  typeof (Student_Detail).GetProperty ("Student")),
              typeof (Student).GetProperty ("First"));

      FieldDescriptor fieldDescriptor =
          new FromClauseFieldResolver (fromClause).ResolveField (StubDatabaseInfo.Instance, _context, fieldExpression, fieldExpression);

      Assert.AreEqual (new Column (new Table ("studentTable", null), "FirstColumn"), fieldDescriptor.Column);
      Assert.AreSame (fromClause, fieldDescriptor.FromClause);
      Assert.AreEqual (typeof (Student).GetProperty ("First"), fieldDescriptor.Member);

      Table expectedLeftSide = new Table ("studentTable", null);
      Table expectedRightSide = fromClause.GetTable (StubDatabaseInfo.Instance);
      Join expectedJoin = new Join (
          expectedLeftSide,
          expectedRightSide,
          new Column (expectedLeftSide, "Student_FK"),
          new Column (expectedRightSide, "Student_Detail_PK"));

      Assert.AreEqual (expectedJoin, fieldDescriptor.SourcePath);
    }

    [Test]
    public void Resolve_DoubleJoin ()
    {
      // sdd.Student_Detail.Student.First
      ParameterExpression identifier = Expression.Parameter (typeof (Student_Detail_Detail), "sdd");
      MainFromClause fromClause = new MainFromClause (identifier, ExpressionHelper.CreateQuerySource_Detail_Detail());

      Expression fieldExpression =
          Expression.MakeMemberAccess (
              Expression.MakeMemberAccess (
                  Expression.MakeMemberAccess (Expression.Parameter (typeof (Student_Detail_Detail), "sdd"),
                      typeof (Student_Detail_Detail).GetProperty ("Student_Detail")),
                  typeof (Student_Detail).GetProperty ("Student")),
              typeof (Student).GetProperty ("First"));

      FieldDescriptor fieldDescriptor =
          new FromClauseFieldResolver (fromClause).ResolveField (StubDatabaseInfo.Instance, _context, fieldExpression, fieldExpression);

      Assert.AreEqual (new Column (new Table ("studentTable", null), "FirstColumn"), fieldDescriptor.Column);
      Assert.AreSame (fromClause, fieldDescriptor.FromClause);
      Assert.AreEqual (typeof (Student).GetProperty ("First"), fieldDescriptor.Member);

      Table expectedInnerLeftSide = new Table ("detailTable", null); // Student_Detail
      Table expectedInnerRightSide = fromClause.GetTable (StubDatabaseInfo.Instance); // Student_Detail_Detail
      Join expectedInnerJoin = new Join ( // sd inner join sdd on sd.FK = sdd.PK
          expectedInnerLeftSide,
          expectedInnerRightSide,
          new Column (expectedInnerLeftSide, "Student_Detail_FK"),
          new Column (expectedInnerRightSide, "Student_Detail_Detail_PK"));

      Table expectedOuterLeftSide = new Table ("studentTable", null); // Student
      Join expectedOuterJoin = new Join ( // s inner join (expectedInnerJoin) on s.FK = sd.PK
          expectedOuterLeftSide,
          expectedInnerJoin,
          new Column (expectedOuterLeftSide, "Student_FK"),
          new Column (expectedInnerLeftSide, "Student_Detail_PK")
          );

      Assert.AreEqual (expectedOuterJoin, fieldDescriptor.SourcePath);
    }

    [Test]
    [ExpectedException (typeof (FieldAccessResolveException), ExpectedMessage = "The member 'Rubicon.Data.Linq.UnitTests.Student.First' does not "
        + "identify a relation.")]
    public void Resolve_Join_InvalidMember ()
    {
      // s.First.Length
      ParameterExpression identifier = Expression.Parameter (typeof (Student), "s");
      MainFromClause fromClause = new MainFromClause (identifier, ExpressionHelper.CreateQuerySource_Detail());

      Expression fieldExpression =
          Expression.MakeMemberAccess (
              Expression.MakeMemberAccess (Expression.Parameter (typeof (Student), "s"),
                  typeof (Student).GetProperty ("First")),
              typeof (string).GetProperty ("Length"));

      new FromClauseFieldResolver (fromClause).ResolveField (StubDatabaseInfo.Instance, _context, fieldExpression, fieldExpression);
    }

    [Test]
    public void Resolve_SimpleMemberAccess_InvalidField ()
    {
      ParameterExpression identifier = Expression.Parameter (typeof (Student), "fromIdentifier1");
      MainFromClause fromClause = new MainFromClause (identifier, ExpressionHelper.CreateQuerySource());

      Expression fieldExpression = Expression.MakeMemberAccess (Expression.Parameter (typeof (Student), "fromIdentifier1"),
          typeof (Student).GetProperty ("NonDBProperty"));
      FieldDescriptor fieldDescriptor =
          new FromClauseFieldResolver (fromClause).ResolveField (StubDatabaseInfo.Instance, _context, fieldExpression, fieldExpression);
      Table table = fromClause.GetTable (StubDatabaseInfo.Instance);
      Assert.AreEqual (new FieldDescriptor (typeof (Student).GetProperty ("NonDBProperty"), fromClause, table, null), fieldDescriptor);
    }

    [Test]
    public void Resolve_UsesContext ()
    {
      // sd.Student.First
      ParameterExpression identifier = Expression.Parameter (typeof (Student_Detail), "sd");
      MainFromClause fromClause = new MainFromClause (identifier, ExpressionHelper.CreateQuerySource_Detail());

      Expression fieldExpression =
          Expression.MakeMemberAccess (
              Expression.MakeMemberAccess (Expression.Parameter (typeof (Student_Detail), "sd"),
                  typeof (Student_Detail).GetProperty ("Student")),
              typeof (Student).GetProperty ("First"));

      FieldDescriptor fieldDescriptor1 =
          new FromClauseFieldResolver (fromClause).ResolveField (StubDatabaseInfo.Instance, _context, fieldExpression, fieldExpression);
      FieldDescriptor fieldDescriptor2 =
          new FromClauseFieldResolver (fromClause).ResolveField (StubDatabaseInfo.Instance, _context, fieldExpression, fieldExpression);

      Table table1 = ((Join) fieldDescriptor1.SourcePath).LeftSide;
      Table table2 = ((Join) fieldDescriptor2.SourcePath).LeftSide;

      Assert.AreSame (table1, table2);
    }
  }
}
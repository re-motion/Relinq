using System;
using System.Linq.Expressions;
using NUnit.Framework;
using Rubicon.Collections;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Data.Linq.DataObjectModel;

namespace Rubicon.Data.Linq.UnitTests.DataObjectModelTest
{
  [TestFixture]
  public class DatabaseInfoUtilityTest
  {
    private IDatabaseInfo _databaseInfo;

    [SetUp]
    public void SetUp()
    {
      _databaseInfo = StubDatabaseInfo.Instance;
    }

    [Test]
    public void GetTableForFromClause()
    {
      MainFromClause fromClause = new MainFromClause (Expression.Parameter (typeof (Student), "s"), ExpressionHelper.CreateQuerySource());
      Table table = DatabaseInfoUtility.GetTableForFromClause (_databaseInfo, fromClause);
      Assert.AreEqual (new Table ("studentTable", "s"), table);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = "The from clause with identifier i and query source type "
        + "Rubicon.Data.Linq.UnitTests.TestQueryable`1[[System.Int32, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]] "
            + "does not identify a queryable table.", MatchType = MessageMatch.Contains)]
    public void GetTableForFromClause_InvalidSource ()
    {
      TestQueryable<int> ints = new TestQueryable<int> (ExpressionHelper.CreateExecutor());
      MainFromClause fromClause = new MainFromClause (Expression.Parameter (typeof (int), "i"), ints);
      DatabaseInfoUtility.GetTableForFromClause (_databaseInfo, fromClause);
    }

    [Test]
    public void GetColumnForFromClause()
    {
      MainFromClause fromClause = new MainFromClause (Expression.Parameter (typeof (Student), "s"), ExpressionHelper.CreateQuerySource ());
      Table table = DatabaseInfoUtility.GetTableForFromClause (_databaseInfo, fromClause);
      Column column = DatabaseInfoUtility.GetColumn (_databaseInfo, table, typeof (Student).GetProperty ("First")).Value;
      Assert.AreEqual (new Column (table, "FirstColumn"),column);
    }

    [Test]
    public void GetColumnForFromClause_InvalidMember ()
    {
      MainFromClause fromClause = new MainFromClause (Expression.Parameter (typeof (Student), "s"), ExpressionHelper.CreateQuerySource ());
      Table table = DatabaseInfoUtility.GetTableForFromClause (_databaseInfo, fromClause);
      Assert.IsNull (DatabaseInfoUtility.GetColumn (_databaseInfo, table, typeof (Student).GetProperty ("NonDBProperty")));
    }

    [Test]
    public void GetRelatedTable ()
    {
      Table table = DatabaseInfoUtility.GetRelatedTable (StubDatabaseInfo.Instance, typeof (Student_Detail).GetProperty ("Student"));
      Assert.AreEqual (new Table ("studentTable", null), table);
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "The member 'Rubicon.Data.Linq.UnitTests.Student.First' does not "
        + "identify a relation.")]
    public void GetRelatedTable_InvalidMember ()
    {
      DatabaseInfoUtility.GetRelatedTable (StubDatabaseInfo.Instance, typeof (Student).GetProperty ("First"));
    }

    [Test]
    public void GetJoinColumns()
    {
      Tuple<string, string> columns = DatabaseInfoUtility.GetJoinColumns (StubDatabaseInfo.Instance, typeof (Student_Detail).GetProperty ("Student"));
      Assert.AreEqual (Tuple.NewTuple("Student_Detail_PK", "Student_FK"), columns);
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "The member 'Rubicon.Data.Linq.UnitTests.Student.First' does not "
        + "identify a relation.")]
    public void GetJoinColumns_InvalidMember ()
    {
      DatabaseInfoUtility.GetJoinColumns (StubDatabaseInfo.Instance, typeof (Student).GetProperty ("First"));
    }
  }
}
using System;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using Rhino.Mocks;
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
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause(Expression.Parameter (typeof (Student), "s"), ExpressionHelper.CreateQuerySource());
      Table table = DatabaseInfoUtility.GetTableForFromClause (_databaseInfo, fromClause);
      Assert.AreEqual (new Table ("studentTable", "s"), table);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = "The from clause with identifier i and query source type <null> does not "
        + "identify a queryable table.\r\nParameter name: fromClause")]
    public void GetTableForFromClause_WithSubQueryFromClause ()
    {
      SubQueryFromClause fromClause = ExpressionHelper.CreateSubQueryFromClause ();
      DatabaseInfoUtility.GetTableForFromClause (_databaseInfo, fromClause);
    }

    [Test]
    public void GetTableForFromClause_CachesTable ()
    {
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause(Expression.Parameter (typeof (Student), "s"), ExpressionHelper.CreateQuerySource ());
      Table table1 = DatabaseInfoUtility.GetTableForFromClause (_databaseInfo, fromClause);
      Table table2 = DatabaseInfoUtility.GetTableForFromClause (_databaseInfo, fromClause);
      Assert.AreSame (table1, table2);
    }

    [Test]
    public void GetTableForFromClause_CachesTable_PerDatabaseInfo ()
    {
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause(Expression.Parameter (typeof (Student), "s"), ExpressionHelper.CreateQuerySource ());

      MockRepository repository = new MockRepository();
      IDatabaseInfo databaseInfoMock = repository.CreateMock<IDatabaseInfo>();

      Expect.Call (databaseInfoMock.GetTableName (fromClause)).Return ("studentTable");

      repository.ReplayAll();

      Table table1 = DatabaseInfoUtility.GetTableForFromClause (_databaseInfo, fromClause);
      Table table2 = DatabaseInfoUtility.GetTableForFromClause (databaseInfoMock, fromClause);
      Assert.AreNotSame (table1, table2);

      repository.VerifyAll();
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = "The from clause with identifier i and query source type "
        + "Rubicon.Data.Linq.UnitTests.TestQueryable`1[[System.Int32, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]] "
            + "does not identify a queryable table.", MatchType = MessageMatch.Contains)]
    public void GetTableForFromClause_InvalidSource ()
    {
      TestQueryable<int> ints = new TestQueryable<int> (ExpressionHelper.CreateExecutor());
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause(Expression.Parameter (typeof (int), "i"), ints);
      DatabaseInfoUtility.GetTableForFromClause (_databaseInfo, fromClause);
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
      Tuple<string, string> columns = DatabaseInfoUtility.GetJoinColumnNames (StubDatabaseInfo.Instance, typeof (Student_Detail).GetProperty ("Student"));
      Assert.AreEqual (Tuple.NewTuple("Student_Detail_PK", "Student_Detail_to_Student_FK"), columns);
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "The member 'Rubicon.Data.Linq.UnitTests.Student.First' does not "
        + "identify a relation.")]
    public void GetJoinColumns_InvalidMember ()
    {
      DatabaseInfoUtility.GetJoinColumnNames (StubDatabaseInfo.Instance, typeof (Student).GetProperty ("First"));
    }

    [Test]
    public void IsRelationMember_True ()
    {
      Assert.IsTrue (DatabaseInfoUtility.IsRelationMember (StubDatabaseInfo.Instance, typeof (Student_Detail_Detail).GetProperty ("Student_Detail")));
      Assert.IsTrue (DatabaseInfoUtility.IsRelationMember (StubDatabaseInfo.Instance, typeof (Student_Detail).GetProperty ("Student")));
    }

    [Test]
    public void IsRelationMember_False ()
    {
      Assert.IsFalse (DatabaseInfoUtility.IsRelationMember (StubDatabaseInfo.Instance, typeof (Student).GetProperty ("First")));
    }

    [Test]
    public void IsRelationMember_NonDBMember ()
    {
      Assert.IsFalse (DatabaseInfoUtility.IsRelationMember (StubDatabaseInfo.Instance, typeof (Student).GetProperty ("NonDBProperty")));
    }

    [Test]
    public void IsVirtualColumn_True()
    {
      Assert.IsTrue (DatabaseInfoUtility.IsVirtualColumn (_databaseInfo, typeof(IndustrialSector).GetProperty ("Student_Detail")));
    }

    [Test]
    public void IsVirtualColumn_False_RealSide ()
    {
      Assert.IsFalse (DatabaseInfoUtility.IsVirtualColumn (_databaseInfo, typeof (Student_Detail).GetProperty ("IndustrialSector")));
    }

    [Test]
    public void IsVirtualColumn_False_NonRelationMember ()
    {
      Assert.IsFalse (DatabaseInfoUtility.IsVirtualColumn (_databaseInfo, typeof (Student).GetProperty ("ID")));
    }

    [Test]
    public void IsVirtualColumn_False_NonDBMember ()
    {
      Assert.IsFalse (DatabaseInfoUtility.IsVirtualColumn (_databaseInfo, typeof (Student).GetProperty ("NonDBProperty")));
    }

    [Test]
    public void GetColumnForFromClause ()
    {
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause(Expression.Parameter (typeof (Student), "s"), ExpressionHelper.CreateQuerySource ());
      IColumnSource table = fromClause.GetFromSource (_databaseInfo);
      Column column = DatabaseInfoUtility.GetColumn (_databaseInfo, table, typeof (Student).GetProperty ("First")).Value;
      Assert.AreEqual (new Column (table, "FirstColumn"), column);
    }

    [Test]
    public void GetColumn_IsTableFalse_MemberNull ()
    {
      IColumnSource table = new LetColumnSource ("s", false);
      Column column = DatabaseInfoUtility.GetColumn (_databaseInfo, table, null).Value;
      Assert.AreEqual (new Column(table,null),column);
    }

    [Test]
    public void GetColumn_IsTableFalse_Member ()
    {
      IColumnSource table = new LetColumnSource ("s", false);
      Column column = DatabaseInfoUtility.GetColumn (_databaseInfo, table, typeof (Student).GetProperty ("First")).Value;
      Assert.AreEqual (new Column (table, "FirstColumn"), column);
    }

    [Test]
    public void GetColumnForFromClause_InvalidMember ()
    {
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause(Expression.Parameter (typeof (Student), "s"), ExpressionHelper.CreateQuerySource ());
      IColumnSource table = fromClause.GetFromSource (_databaseInfo);
      Assert.IsNull (DatabaseInfoUtility.GetColumn (_databaseInfo, table, typeof (Student).GetProperty ("NonDBProperty")));
    }

    [Test]
    public void GetPrimaryKeyMember ()
    {
      MemberInfo studentDetailPKMember = DatabaseInfoUtility.GetPrimaryKeyMember (_databaseInfo, typeof (Student_Detail));
      Assert.AreEqual (typeof (Student_Detail).GetProperty ("ID"), studentDetailPKMember);
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "The primary key member of type 'System.Object' cannot be determined "
        + "because it is no entity type.")]
    public void GetPrimaryKeyMember_NonEntityType ()
    {
      DatabaseInfoUtility.GetPrimaryKeyMember (_databaseInfo, typeof (object));
    }

    [Test]
    public void GetEntity ()
    {
      
    }
  }
}

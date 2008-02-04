using System;
using System.Linq.Expressions;
using NUnit.Framework;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Data.Linq.Parsing;

namespace Rubicon.Data.Linq.UnitTests.ParsingTest
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
      Assert.AreEqual (new Table ("sourceTable", "s"), table);
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
      Column column = DatabaseInfoUtility.GetColumn (_databaseInfo, table, typeof (Student).GetProperty ("First"));
      Assert.AreEqual (new Column (table, "FirstColumn"),column);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = "he member Rubicon.Data.Linq.UnitTests.Student.NonDBProperty does "+
      "not identify a queryable column in table sourceTable.", MatchType = MessageMatch.Contains)]
    public void GetColumnForFromClause_InvalidMember ()
    {
      MainFromClause fromClause = new MainFromClause (Expression.Parameter (typeof (Student), "s"), ExpressionHelper.CreateQuerySource ());
      Table table = DatabaseInfoUtility.GetTableForFromClause (_databaseInfo, fromClause);
      DatabaseInfoUtility.GetColumn (_databaseInfo, table, typeof (Student).GetProperty ("NonDBProperty"));
    }

  }
}
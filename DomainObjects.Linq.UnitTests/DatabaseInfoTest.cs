using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Rubicon.Collections;
using Rubicon.Data.Linq;
using Rubicon.Data.DomainObjects.UnitTests.TestDomain;
using Rubicon.Data.Linq.Clauses;
using System.Linq.Expressions;
using Rubicon.Data.Linq.DataObjectModel;

namespace Rubicon.Data.DomainObjects.Linq.UnitTests
{
  [TestFixture]
  public class DatabaseInfoTest
  {
    private IDatabaseInfo _databaseInfo;

    [SetUp]
    public void SetUp ()
    {
      _databaseInfo = DatabaseInfo.Instance;
    }

    [Test]
    public void GetTableName()
    {
      Assert.AreEqual (new Table ("Order", "source"), _databaseInfo.GetTable (CreateFromClause<Order>()));
      Assert.AreEqual (new Table ("Company", "source"), _databaseInfo.GetTable (CreateFromClause<Customer>()));
      Assert.AreEqual (new Table ("TableWithValidRelations", "source"), _databaseInfo.GetTable (CreateFromClause<ClassWithValidRelations> ()));
    }

    [Test]
    public void GetTableName_InvalidType ()
    {
      Assert.IsNull (_databaseInfo.GetTable(CreateFromClause<DomainObject>()));

      DummyQueryable<string> stringSource = new DummyQueryable<string>();
      MainFromClause stringClause = new MainFromClause (Expression.Parameter (typeof (string), "source"), stringSource);
      
      Assert.IsNull (_databaseInfo.GetTable (stringClause));
    }

    private FromClauseBase CreateFromClause<T> ()
        where T : DomainObject
    {
      IQueryable querySource = new DomainObjectQueryable<T> (null);
      return new MainFromClause (Expression.Parameter (querySource.ElementType, "source"), querySource);
    }

    [Test]
    public void GetColumnName()
    {
      Assert.AreEqual ("OrderNo", _databaseInfo.GetColumnName (typeof (Order).GetProperty ("OrderNumber")));
      Assert.AreEqual ("DeliveryDate", _databaseInfo.GetColumnName (typeof (Order).GetProperty ("DeliveryDate")));
      Assert.AreEqual ("OrderID", _databaseInfo.GetColumnName (typeof (OrderTicket).GetProperty ("Order")));
    }

    [Test]
    public void GetColumnName_Null ()
    {
      Assert.IsNull (_databaseInfo.GetColumnName (typeof (Order).GetProperty ("OrderTicket")));
      Assert.IsNull (_databaseInfo.GetColumnName (typeof (Order).GetProperty ("NotInMapping")));
      Assert.IsNull (_databaseInfo.GetColumnName (typeof (string).GetProperty ("Length")));
      Assert.IsNull (_databaseInfo.GetColumnName (typeof (DatabaseInfoTest).GetMethod ("SetUp")));
      Assert.IsNull (_databaseInfo.GetColumnName (typeof (DatabaseInfoTest).GetField ("_databaseInfo", BindingFlags.NonPublic | BindingFlags.Instance)));
    }

    [Test]
    public void GetJoinColumns_FK_Right()
    {
      Tuple<string, string> columns = _databaseInfo.GetJoinColumns (typeof (Order).GetProperty ("OrderItems"));
      Assert.AreEqual ("ID", columns.A);
      Assert.AreEqual ("OrderID", columns.B);
    }

    [Test]
    public void GetJoinColumns_FK_Left ()
    {
      Tuple<string, string> columns = _databaseInfo.GetJoinColumns (typeof (OrderItem).GetProperty ("Order"));
      Assert.AreEqual ("OrderID", columns.A);
      Assert.AreEqual ("ID", columns.B);
    }

    [Test]
    public void GetJoinColumns_NotInMapping ()
    {
      Tuple<string, string> columns = _databaseInfo.GetJoinColumns (typeof (Order).GetProperty ("NotInMapping"));
      Assert.IsNull (columns);
    }

    [Test]
    public void GetJoinColumns_NoRelationProperty ()
    {
      Tuple<string, string> columns = _databaseInfo.GetJoinColumns (typeof (Order).GetProperty ("OrderNumber"));
      Assert.IsNull (columns);
    }

    [Test]
    public void GetRelatedTable_FK_Right ()
    {
      string tableName = _databaseInfo.GetRelatedTableName (typeof (Order).GetProperty ("OrderItems"));
      Assert.AreEqual ("OrderItem", tableName);
    }

    [Test]
    public void GetRelatedTable_FK_Left ()
    {
      string tableName = _databaseInfo.GetRelatedTableName (typeof (OrderItem).GetProperty ("Order"));
      Assert.AreEqual ("Order", tableName);
    }

    [Test]
    public void GetRelatedTable_NotInMapping ()
    {
      string tableName = _databaseInfo.GetRelatedTableName (typeof (Order).GetProperty ("NotInMapping"));
      Assert.IsNull (tableName);
    }

    [Test]
    public void GetRelatedTable_NoRelationProperty ()
    {
      string tableName = _databaseInfo.GetRelatedTableName (typeof (Order).GetProperty ("OrderNumber"));
      Assert.IsNull (tableName);
    }

  }
}
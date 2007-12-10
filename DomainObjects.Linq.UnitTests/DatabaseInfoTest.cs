using System;
using System.Reflection;
using NUnit.Framework;
using Rubicon.Data.Linq;
using Rubicon.Data.DomainObjects.UnitTests.TestDomain;

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
      Assert.AreEqual ("Order", _databaseInfo.GetTableName (typeof(DomainObjectQueryable<Order>)));
      Assert.AreEqual ("Company", _databaseInfo.GetTableName (typeof (DomainObjectQueryable<Customer>)));
      Assert.AreEqual ("TableWithValidRelations", _databaseInfo.GetTableName (typeof (DomainObjectQueryable<ClassWithValidRelations>)));
    }

    [Test]
    public void GetTableName_InvalidType ()
    {
      Assert.IsNull (_databaseInfo.GetTableName(typeof (DomainObjectQueryable<DomainObject>)));
      Assert.IsNull (_databaseInfo.GetTableName (typeof (string)));
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
  }
}
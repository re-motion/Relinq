using System;
using NUnit.Framework;
using Rubicon.Data.Linq.DataObjectModel;

namespace Rubicon.Data.Linq.UnitTests.DataObjectModelTest
{
  [TestFixture]
  public class TableTest
  {
    [Test]
    public void GetStartingTable()
    {
      Table t = new Table("x", "y");
      Assert.AreEqual (t, t.GetStartingTable());
    }
  }
}
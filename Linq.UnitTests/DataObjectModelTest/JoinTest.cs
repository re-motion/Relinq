using System;
using NUnit.Framework;
using Rubicon.Data.Linq.DataObjectModel;

namespace Rubicon.Data.Linq.UnitTests.DataObjectModelTest
{
  [TestFixture]
  public class JoinTest
  {
    [Test]
    public void GetStartingTable_SingleJoin ()
    {
      Table t1 = new Table ("x", "y");
      Table t2 = new Table ("a", "b");
      Join j1 = new Join(t1, t2, new Column(), new Column());
      Assert.AreEqual (t2, j1.GetStartingTable ());
    }

    [Test]
    public void GetStartingTable_NestedJoin ()
    {
      Table t1 = new Table ("x", "y");
      Table t2 = new Table ("a", "b");
      Table t3 = new Table ("1", "2");
      Join j1 = new Join (t1, t2, new Column (), new Column ());
      Join j2 = new Join (t3, j1, new Column (), new Column ());
      Assert.AreEqual (t2, j2.GetStartingTable ());
    }
  }
}
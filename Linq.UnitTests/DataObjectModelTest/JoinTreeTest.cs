using System;
using System.Linq;
using NUnit.Framework;
using Rubicon.Data.Linq.DataObjectModel;
using NUnit.Framework.SyntaxHelpers;

namespace Rubicon.Data.Linq.UnitTests.DataObjectModelTest
{
  [TestFixture]
  public class JoinTreeTest
  {
    [Test]
    public void GetStartingTable_SingleJoin ()
    {
      Table t1 = new Table ("x", "y");
      Table t2 = new Table ("a", "b");
      JoinTree j1 = new JoinTree(t1, t2, new Column(), new Column());
      Assert.AreEqual (t2, j1.GetStartingTable ());
    }

    [Test]
    public void GetStartingTable_NestedJoin ()
    {
      Table t1 = new Table ("x", "y");
      Table t2 = new Table ("a", "b");
      Table t3 = new Table ("1", "2");
      JoinTree j1 = new JoinTree (t1, t2, new Column (), new Column ());
      JoinTree j2 = new JoinTree (t3, j1, new Column (), new Column ());
      Assert.AreEqual (t2, j2.GetStartingTable ());
    }

    [Test]
    public void GetSingleJoinForRoot ()
    {
      Table t1 = new Table ("x", "y");
      Table t2 = new Table ("a", "b");
      Table t3 = new Table ("1", "2");
      JoinTree j1 = new JoinTree (t1, t2, new Column (t1, "alpha"), new Column (t2, "beta"));
      JoinTree j2 = new JoinTree (t3, j1, new Column (t3, "gamma"), new Column (t1, "delta"));

      Assert.AreEqual (new SingleJoin (new Column (t1, "alpha"), new Column (t2, "beta")), j1.GetSingleJoinForRoot());
      Assert.AreEqual (new SingleJoin (new Column (t3, "gamma"), new Column (t1, "delta")), j2.GetSingleJoinForRoot ());
    }

    [Test]
    public void GetAllSingleJoins ()
    {
      Table t1 = new Table ("x", "y");
      Table t2 = new Table ("a", "b");
      Table t3 = new Table ("1", "2");
      JoinTree j1 = new JoinTree (t1, t2, new Column (t1, "alpha"), new Column (t2, "beta"));
      JoinTree j2 = new JoinTree (t3, j1, new Column (t3, "gamma"), new Column (t1, "delta"));

      SingleJoin sj1 = j1.GetSingleJoinForRoot ();
      SingleJoin sj2 = j2.GetSingleJoinForRoot ();

      Assert.That (j1.GetAllSingleJoins ().ToArray(), Is.EqualTo (new object[] { sj1 }));
      Assert.That (j2.GetAllSingleJoins ().ToArray(), Is.EqualTo (new object[] { sj2, sj1 }));
    }
  }
}
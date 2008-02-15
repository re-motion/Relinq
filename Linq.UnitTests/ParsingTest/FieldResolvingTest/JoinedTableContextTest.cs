using System;
using System.Reflection;
using NUnit.Framework;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Data.Linq.Parsing.FieldResolving;

namespace Rubicon.Data.Linq.UnitTests.ParsingTest.FieldResolvingTest
{
  [TestFixture]
  public class JoinedTableContextTest
  {
    [Test]
    public void GetJoinedTable()
    {
      JoinedTableContext context = new JoinedTableContext();
      IFieldSourcePath fieldSourcePath = new Table();
      MemberInfo member = typeof (Student_Detail).GetProperty ("Student");
      Table table = context.GetJoinedTable (StubDatabaseInfo.Instance, fieldSourcePath, member);
      Assert.IsNotNull (table);
      Assert.IsNull (table.Alias);
      Assert.AreEqual ("sourceTable", table.Name);
    }

    [Test]
    public void GetJoinedTable_Twice ()
    {
      JoinedTableContext context = new JoinedTableContext ();
      IFieldSourcePath fieldSourcePath1 = new Table ();
      IFieldSourcePath fieldSourcePath2 = new Table ();
      MemberInfo member = typeof (Student_Detail).GetProperty ("Student");
      Table table1 = context.GetJoinedTable (StubDatabaseInfo.Instance, fieldSourcePath1, member);
      Table table2 = context.GetJoinedTable (StubDatabaseInfo.Instance, fieldSourcePath2, member);
      Assert.AreSame (table1, table2);
    }

    [Test]
    public void GetJoinedTable_TwiceWithDifferentMembers ()
    {
      JoinedTableContext context = new JoinedTableContext ();
      IFieldSourcePath fieldSourcePath = new Table ();
      MemberInfo member1 = typeof (Student_Detail).GetProperty ("IndustrialSector");
      MemberInfo member2 = typeof (Student_Detail).GetProperty ("Student");
      
      Table table1 = context.GetJoinedTable (StubDatabaseInfo.Instance, fieldSourcePath, member1);
      Table table2 = context.GetJoinedTable (StubDatabaseInfo.Instance, fieldSourcePath, member2);
      Assert.AreNotSame (table1, table2);
      Assert.AreEqual ("industrialTable", table1.Name);    
      Assert.AreEqual ("sourceTable", table2.Name);
    }

    [Test]
    public void GetJoinedTable_TwiceWithDifferentFieldPath ()
    {
      JoinedTableContext context = new JoinedTableContext ();
      IFieldSourcePath fieldSourcePath1 = new Table ("x","y");
      IFieldSourcePath fieldSourcePath2 = new Table ("z","i");

      MemberInfo member = typeof (Student_Detail).GetProperty ("IndustrialSector");
      
      Table table1 = context.GetJoinedTable (StubDatabaseInfo.Instance, fieldSourcePath1, member);
      Table table2 = context.GetJoinedTable (StubDatabaseInfo.Instance, fieldSourcePath2, member);

      Assert.AreNotSame (table1, table2);
      Assert.AreEqual (table1, table2);
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "The member 'Rubicon.Data.Linq.UnitTests.Student.First' "
      +"does not identify a relation.")]
    public void GetJoinedTable_InvalidMember ()
    {
      JoinedTableContext context = new JoinedTableContext ();
      IFieldSourcePath fieldSourcePath = new Table ();
      MemberInfo member = typeof (Student).GetProperty ("First");
      context.GetJoinedTable (StubDatabaseInfo.Instance, fieldSourcePath, member);
    }
    
  }
}
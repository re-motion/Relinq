using System;
using System.Reflection;
using NUnit.Framework;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing.FieldResolving;
using NUnit.Framework.SyntaxHelpers;

namespace Remotion.Data.Linq.UnitTests.ParsingTest.FieldResolvingTest
{
  [TestFixture]
  public class JoinedTableContextTest
  {
    [Test]
    public void GetJoinedTable()
    {
      JoinedTableContext context = new JoinedTableContext();
      FieldSourcePath fieldSourcePath = ExpressionHelper.GetPathForNewTable();
      MemberInfo member = typeof (Student_Detail).GetProperty ("Student");
      Assert.AreEqual (0, context.Count);
      Table table = context.GetJoinedTable (StubDatabaseInfo.Instance, fieldSourcePath, member);
      Assert.AreEqual (1, context.Count);
      Assert.IsNotNull (table);
      Assert.IsNull (table.Alias);
      Assert.AreEqual ("studentTable", table.Name);
    }

    [Test]
    public void GetJoinedTable_Twice ()
    {
      JoinedTableContext context = new JoinedTableContext ();
      Table table = new Table();
      FieldSourcePath fieldSourcePath1 = ExpressionHelper.GetPathForTable (table);
      FieldSourcePath fieldSourcePath2 = ExpressionHelper.GetPathForTable (table);
      MemberInfo member = typeof (Student_Detail).GetProperty ("Student");
      Table table1 = context.GetJoinedTable (StubDatabaseInfo.Instance, fieldSourcePath1, member);
      Table table2 = context.GetJoinedTable (StubDatabaseInfo.Instance, fieldSourcePath2, member);
      Assert.AreEqual (1, context.Count);
      Assert.AreSame (table1, table2);
    }

    [Test]
    public void GetJoinedTable_TwiceWithDifferentMembers ()
    {
      JoinedTableContext context = new JoinedTableContext ();
      FieldSourcePath fieldSourcePath = ExpressionHelper.GetPathForNewTable ();
      MemberInfo member1 = typeof (Student_Detail).GetProperty ("IndustrialSector");
      MemberInfo member2 = typeof (Student_Detail).GetProperty ("Student");
      
      Table table1 = context.GetJoinedTable (StubDatabaseInfo.Instance, fieldSourcePath, member1);
      Table table2 = context.GetJoinedTable (StubDatabaseInfo.Instance, fieldSourcePath, member2);
      Assert.AreEqual (2, context.Count);
      Assert.AreNotSame (table1, table2);
      Assert.AreEqual ("industrialTable", table1.Name);    
      Assert.AreEqual ("studentTable", table2.Name);
    }

    [Test]
    public void GetJoinedTable_TwiceWithDifferentFieldPath ()
    {
      JoinedTableContext context = new JoinedTableContext ();
      FieldSourcePath fieldSourcePath1 = ExpressionHelper.GetPathForNewTable ("x", "y");
      FieldSourcePath fieldSourcePath2 = ExpressionHelper.GetPathForNewTable ("z", "i");

      MemberInfo member = typeof (Student_Detail).GetProperty ("IndustrialSector");
      
      Table table1 = context.GetJoinedTable (StubDatabaseInfo.Instance, fieldSourcePath1, member);
      Table table2 = context.GetJoinedTable (StubDatabaseInfo.Instance, fieldSourcePath2, member);

      Assert.AreNotSame (table1, table2);
      Assert.AreEqual (table1, table2);
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "The member 'Remotion.Data.Linq.UnitTests.Student.First' "
      +"does not identify a relation.")]
    public void GetJoinedTable_InvalidMember ()
    {
      JoinedTableContext context = new JoinedTableContext ();
      FieldSourcePath fieldSourcePath = ExpressionHelper.GetPathForNewTable ();
      MemberInfo member = typeof (Student).GetProperty ("First");
      context.GetJoinedTable (StubDatabaseInfo.Instance, fieldSourcePath, member);
    }

    [Test]
    public void CreateAliases()
    {
      JoinedTableContext context = new JoinedTableContext ();
      FieldSourcePath fieldSourcePath = ExpressionHelper.GetPathForNewTable ();
      
      MemberInfo member1 = typeof (Student_Detail).GetProperty ("Student");
      Table table1 = context.GetJoinedTable (StubDatabaseInfo.Instance, fieldSourcePath, member1);

      Assert.IsNull (table1.Alias);
      context.CreateAliases();
      Assert.AreEqual ("j0", table1.Alias);
    }

    [Test]
    public void CreateAliases_DoesntOverwriteAliases ()
    {
      JoinedTableContext context = new JoinedTableContext ();
      FieldSourcePath fieldSourcePath = ExpressionHelper.GetPathForNewTable ();

      MemberInfo member1 = typeof (Student_Detail).GetProperty ("Student");
      Table table1 = context.GetJoinedTable (StubDatabaseInfo.Instance, fieldSourcePath, member1);
      table1.SetAlias ("Franz");

      context.CreateAliases ();
      Assert.AreEqual ("Franz", table1.Alias);
    }

    [Test]
    public void CreateAliases_MultipleTimes ()
    {
      JoinedTableContext context = new JoinedTableContext ();
      FieldSourcePath fieldSourcePath = ExpressionHelper.GetPathForNewTable ();

      MemberInfo member1 = typeof (Student_Detail).GetProperty ("Student");
      Table table1 = context.GetJoinedTable (StubDatabaseInfo.Instance, fieldSourcePath, member1);

      Assert.IsNull (table1.Alias);
      context.CreateAliases ();
      Assert.AreEqual ("j0", table1.Alias);

      MemberInfo member2 = typeof (Student_Detail).GetProperty ("IndustrialSector");
      Table table2 = context.GetJoinedTable (StubDatabaseInfo.Instance, fieldSourcePath, member2);

      Assert.IsNull (table2.Alias);
      context.CreateAliases ();
      Assert.AreEqual ("j1", table2.Alias);
    }

    [Test]
    public void CreateAliases_MultipleTablesAndOrdering ()
    {
      JoinedTableContext context = new JoinedTableContext ();
      FieldSourcePath fieldSourcePath1 = ExpressionHelper.GetPathForNewTable ("1", null);
      FieldSourcePath fieldSourcePath2 = ExpressionHelper.GetPathForNewTable ("2", null);
      FieldSourcePath fieldSourcePath3 = ExpressionHelper.GetPathForNewTable ("3", null);

      MemberInfo member = typeof (Student_Detail).GetProperty ("Student");

      Table table1 = context.GetJoinedTable (StubDatabaseInfo.Instance, fieldSourcePath1, member);
      Table table2 = context.GetJoinedTable (StubDatabaseInfo.Instance, fieldSourcePath2, member);
      Table table3 = context.GetJoinedTable (StubDatabaseInfo.Instance, fieldSourcePath3, member);

      Assert.IsNull (table1.Alias);
      Assert.IsNull (table2.Alias);
      Assert.IsNull (table3.Alias);

      context.CreateAliases ();

      Assert.AreEqual ("j0", table1.Alias);
      Assert.AreEqual ("j1", table2.Alias);
      Assert.AreEqual ("j2", table3.Alias);
    }
  }
}
using System.Reflection;
using NUnit.Framework;
using Rubicon.Collections;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Data.Linq.Parsing.FieldResolving;

namespace Rubicon.Data.Linq.UnitTests.ParsingTest.FieldResolvingTest
{
  [TestFixture]
  public class FieldSourcePathBuilderTest
  {
    private JoinedTableContext _context;
    private Table _initialTable;
    private PropertyInfo _studentDetailMember;
    private PropertyInfo _studentMember;

    [SetUp]
    public void SetUp()
    {
      _context = new JoinedTableContext();
      _initialTable = new Table ("initial", "i");

      _studentDetailMember = typeof (Student_Detail_Detail).GetProperty ("Student_Detail");
      _studentMember = typeof (Student_Detail).GetProperty ("Student");
    }

    [Test]
    public void BuildFieldSourcePath_NoJoin ()
    {
      MemberInfo[] joinMembers = new MemberInfo[] { };
      Tuple<IFieldSourcePath, Table> result =
          new FieldSourcePathBuilder().BuildFieldSourcePath (StubDatabaseInfo.Instance, _context, _initialTable, joinMembers);

      Assert.AreSame (_initialTable, result.A);
      Assert.AreSame (_initialTable, result.B);
    }

    [Test]
    public void BuildFieldSourcePath_SimpleJoin ()
    {
      MemberInfo[] joinMembers = new MemberInfo[] { _studentMember };
      Tuple<IFieldSourcePath, Table> result =
          new FieldSourcePathBuilder ().BuildFieldSourcePath (StubDatabaseInfo.Instance, _context, _initialTable, joinMembers);

      Table relatedTable = DatabaseInfoUtility.GetRelatedTable (StubDatabaseInfo.Instance, _studentMember);
      Tuple<string, string> joinColumns = DatabaseInfoUtility.GetJoinColumns (StubDatabaseInfo.Instance, _studentMember);
      JoinTree expectedJoinTree =
          new JoinTree (relatedTable, _initialTable, new Column (relatedTable, joinColumns.B), new Column (_initialTable, joinColumns.A));

      Assert.AreEqual (expectedJoinTree, result.A);
      Assert.AreEqual (relatedTable, result.B);
    }

    [Test]
    public void BuildFieldSourcePath_NestedJoin ()
    {
      MemberInfo[] joinMembers = new MemberInfo[] { _studentDetailMember, _studentMember };
      Tuple<IFieldSourcePath, Table> result =
          new FieldSourcePathBuilder ().BuildFieldSourcePath (StubDatabaseInfo.Instance, _context, _initialTable, joinMembers);

      Table relatedTable1 = DatabaseInfoUtility.GetRelatedTable (StubDatabaseInfo.Instance, _studentDetailMember);
      Tuple<string, string> joinColumns1 = DatabaseInfoUtility.GetJoinColumns (StubDatabaseInfo.Instance, _studentDetailMember);
      JoinTree expectedInnerJoinTree =
          new JoinTree (relatedTable1, _initialTable, new Column (relatedTable1, joinColumns1.B), new Column (_initialTable, joinColumns1.A));

      Table relatedTable2 = DatabaseInfoUtility.GetRelatedTable (StubDatabaseInfo.Instance, _studentMember);
      Tuple<string, string> joinColumns2 = DatabaseInfoUtility.GetJoinColumns (StubDatabaseInfo.Instance, _studentMember);

      JoinTree expectedOuterJoinTree =
          new JoinTree(relatedTable2, expectedInnerJoinTree, new Column (relatedTable2, joinColumns2.B), new Column (relatedTable1, joinColumns2.A) );

      Assert.AreEqual (expectedOuterJoinTree, result.A);
      Assert.AreEqual (relatedTable2, result.B);
    }

    [Test]
    public void BuildFieldSourcePath_UsesContext ()
    {
      Assert.AreEqual (0, _context.Count);
      BuildFieldSourcePath_SimpleJoin();
      Assert.AreEqual (1, _context.Count);
    }
  }
}
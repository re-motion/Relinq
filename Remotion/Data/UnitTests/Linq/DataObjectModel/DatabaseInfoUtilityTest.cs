// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// version 3.0 as published by the Free Software Foundation.
// 
// re-motion is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-motion; if not, see http://www.gnu.org/licenses.
// 
using System;
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Collections;
using Remotion.Data.Linq.Backend;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Backend.DataObjectModel;

namespace Remotion.Data.UnitTests.Linq.DataObjectModel
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
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause_Student();
      Table table = DatabaseInfoUtility.GetTableForFromClause (_databaseInfo, fromClause);
      Assert.That (table, Is.EqualTo (new Table ("studentTable", "s")));
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = "The from clause with identifier i and item type "
        + "System.Int32 does not identify a queryable table.", MatchType = MessageMatch.Contains)]
    public void GetTableForFromClause_InvalidSource ()
    {
      var ints = new TestQueryable<int> (ExpressionHelper.CreateExecutor());
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause("i", typeof (int) , ints);
      DatabaseInfoUtility.GetTableForFromClause (_databaseInfo, fromClause);
    }

    [Test]
    public void GetRelatedTable ()
    {
      Table table = DatabaseInfoUtility.GetRelatedTable (StubDatabaseInfo.Instance, typeof (Student_Detail).GetProperty ("Student"));
      Assert.That (table, Is.EqualTo (new Table ("studentTable", null)));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "The member 'Remotion.Data.UnitTests.Linq.Student.First' does not "
        + "identify a relation.")]
    public void GetRelatedTable_InvalidMember ()
    {
      DatabaseInfoUtility.GetRelatedTable (StubDatabaseInfo.Instance, typeof (Student).GetProperty ("First"));
    }

    [Test]
    public void GetJoinColumns()
    {
      Tuple<string, string> columns = DatabaseInfoUtility.GetJoinColumnNames (StubDatabaseInfo.Instance, typeof (Student_Detail).GetProperty ("Student"));
      Assert.That (columns, Is.EqualTo (Tuple.NewTuple ("Student_Detail_PK", "Student_Detail_to_Student_FK")));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "The member 'Remotion.Data.UnitTests.Linq.Student.First' does not "
        + "identify a relation.")]
    public void GetJoinColumns_InvalidMember ()
    {
      DatabaseInfoUtility.GetJoinColumnNames (StubDatabaseInfo.Instance, typeof (Student).GetProperty ("First"));
    }

    [Test]
    public void IsRelationMember_True ()
    {
      Assert.That (
          DatabaseInfoUtility.IsRelationMember (StubDatabaseInfo.Instance, typeof (Student_Detail_Detail).GetProperty ("Student_Detail")), Is.True);
      Assert.That (DatabaseInfoUtility.IsRelationMember (StubDatabaseInfo.Instance, typeof (Student_Detail).GetProperty ("Student")), Is.True);
    }

    [Test]
    public void IsRelationMember_False ()
    {
      Assert.That (DatabaseInfoUtility.IsRelationMember (StubDatabaseInfo.Instance, typeof (Student).GetProperty ("First")), Is.False);
    }

    [Test]
    public void IsRelationMember_NonDBMember ()
    {
      Assert.That (DatabaseInfoUtility.IsRelationMember (StubDatabaseInfo.Instance, typeof (Student).GetProperty ("NonDBProperty")), Is.False);
    }

    [Test]
    public void IsVirtualColumn_True()
    {
      Assert.That (DatabaseInfoUtility.IsVirtualColumn (_databaseInfo, typeof (IndustrialSector).GetProperty ("Student_Detail")), Is.True);
    }

    [Test]
    public void IsVirtualColumn_False_RealSide ()
    {
      Assert.That (DatabaseInfoUtility.IsVirtualColumn (_databaseInfo, typeof (Student_Detail).GetProperty ("IndustrialSector")), Is.False);
    }

    [Test]
    public void IsVirtualColumn_False_NonRelationMember ()
    {
      Assert.That (DatabaseInfoUtility.IsVirtualColumn (_databaseInfo, typeof (Student).GetProperty ("ID")), Is.False);
    }

    [Test]
    public void IsVirtualColumn_False_NonDBMember ()
    {
      Assert.That (DatabaseInfoUtility.IsVirtualColumn (_databaseInfo, typeof (Student).GetProperty ("NonDBProperty")), Is.False);
    }

    [Test]
    public void GetColumn ()
    {
      var table = new Table ("Student", "s");
      Column column = DatabaseInfoUtility.GetColumn (_databaseInfo, table, typeof (Student).GetProperty ("First")).Value;
      Assert.That (column, Is.EqualTo (new Column (table, "FirstColumn")));
    }

    [Test]
    public void GetColumn_IsTableFalse_MemberNull ()
    {
      IColumnSource table = new LetColumnSource ("s", false);
      Column column = DatabaseInfoUtility.GetColumn (_databaseInfo, table, null).Value;
      Assert.That (column, Is.EqualTo (new Column (table, null)));
    }

    [Test]
    public void GetColumn_IsTableFalse_Member ()
    {
      IColumnSource table = new LetColumnSource ("s", false);
      Column column = DatabaseInfoUtility.GetColumn (_databaseInfo, table, typeof (Student).GetProperty ("First")).Value;
      Assert.That (column, Is.EqualTo (new Column (table, "FirstColumn")));
    }

    [Test]
    public void GetColumn_InvalidMember ()
    {
      IColumnSource table = new Table ("Student", "s");
      Assert.That (DatabaseInfoUtility.GetColumn (_databaseInfo, table, typeof (Student).GetProperty ("NonDBProperty")), Is.Null);
    }

    [Test]
    public void GetPrimaryKeyMember ()
    {
      MemberInfo studentDetailKeyMember = DatabaseInfoUtility.GetPrimaryKeyMember (_databaseInfo, typeof (Student_Detail));
      Assert.That (studentDetailKeyMember, Is.EqualTo (typeof (Student_Detail).GetProperty ("ID")));
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

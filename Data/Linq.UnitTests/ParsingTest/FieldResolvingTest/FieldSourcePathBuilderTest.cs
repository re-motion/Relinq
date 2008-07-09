/* Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh
 *
 * This program is free software: you can redistribute it and/or modify it under 
 * the terms of the re:motion license agreement in license.txt. If you did not 
 * receive it, please visit http://www.re-motion.org/licensing.
 * 
 * Unless otherwise provided, this software is distributed on an "AS IS" basis, 
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 */

using System.Reflection;
using NUnit.Framework;
using Remotion.Collections;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing.FieldResolving;

namespace Remotion.Data.Linq.UnitTests.ParsingTest.FieldResolvingTest
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
      FieldSourcePath result =
          new FieldSourcePathBuilder ().BuildFieldSourcePath (StubDatabaseInfo.Instance, _context, _initialTable, joinMembers);

      FieldSourcePath expected = new FieldSourcePath(_initialTable, new SingleJoin[0]);
      Assert.AreEqual (expected, result);
    }

    [Test]
    public void BuildFieldSourcePath_SimpleJoin ()
    {
      MemberInfo[] joinMembers = new MemberInfo[] { _studentMember };
      FieldSourcePath result =
          new FieldSourcePathBuilder ().BuildFieldSourcePath (StubDatabaseInfo.Instance, _context, _initialTable, joinMembers);

      Table relatedTable = DatabaseInfoUtility.GetRelatedTable (StubDatabaseInfo.Instance, _studentMember);
      Tuple<string, string> joinColumns = DatabaseInfoUtility.GetJoinColumnNames (StubDatabaseInfo.Instance, _studentMember);

      SingleJoin singleJoin = new SingleJoin (new Column (_initialTable, joinColumns.A), new Column (relatedTable, joinColumns.B));
      FieldSourcePath expected = new FieldSourcePath (_initialTable, new[] { singleJoin });
      Assert.AreEqual (expected, result);
    }

    [Test]
    public void BuildFieldSourcePath_NestedJoin ()
    {
      MemberInfo[] joinMembers = new MemberInfo[] { _studentDetailMember, _studentMember };
      FieldSourcePath result =
          new FieldSourcePathBuilder ().BuildFieldSourcePath (StubDatabaseInfo.Instance, _context, _initialTable, joinMembers);

      Table relatedTable1 = DatabaseInfoUtility.GetRelatedTable (StubDatabaseInfo.Instance, _studentDetailMember);
      Tuple<string, string> joinColumns1 = DatabaseInfoUtility.GetJoinColumnNames (StubDatabaseInfo.Instance, _studentDetailMember);

      Table relatedTable2 = DatabaseInfoUtility.GetRelatedTable (StubDatabaseInfo.Instance, _studentMember);
      Tuple<string, string> joinColumns2 = DatabaseInfoUtility.GetJoinColumnNames (StubDatabaseInfo.Instance, _studentMember);

      SingleJoin singleJoin1 = new SingleJoin (new Column (_initialTable, joinColumns1.A), new Column (relatedTable1, joinColumns1.B));
      SingleJoin singleJoin2 = new SingleJoin (new Column (relatedTable1, joinColumns2.A), new Column (relatedTable2, joinColumns2.B));
      FieldSourcePath expected = new FieldSourcePath (_initialTable, new[] { singleJoin1, singleJoin2 });
      Assert.AreEqual (expected, result);
    }

    [Test]
    public void BuildFieldSourcePath_UsesContext ()
    {
      Assert.AreEqual (0, _context.Count);
      BuildFieldSourcePath_SimpleJoin ();
      Assert.AreEqual (1, _context.Count);
    }

    //Reversed ordering
    //[Test]
    //public void BuildFieldSourcePath_NoJoin ()
    //{
    //  MemberInfo[] joinMembers = new MemberInfo[] { };
    //  Tuple<IFieldSourcePath, Table> result =
    //      new FieldSourcePathBuilder ().BuildFieldSourcePath (StubDatabaseInfo.Instance, _context, _initialTable, joinMembers);

    //  Assert.AreSame (_initialTable, result.A);
    //  Assert.AreSame (_initialTable, result.B);
    //}

    //[Test]
    //public void BuildFieldSourcePath_SimpleJoin ()
    //{
    //  MemberInfo[] joinMembers = new MemberInfo[] { _studentMember };
    //  Tuple<IFieldSourcePath, Table> result =
    //      new FieldSourcePathBuilder ().BuildFieldSourcePath (StubDatabaseInfo.Instance, _context, _initialTable, joinMembers);

    //  Table relatedTable = DatabaseInfoUtility.GetRelatedTable (StubDatabaseInfo.Instance, _studentMember);
    //  Tuple<string, string> joinColumns = DatabaseInfoUtility.GetJoinColumnNames (StubDatabaseInfo.Instance, _studentMember);
    //  JoinTree expectedJoinTree =
    //      new JoinTree (_initialTable, relatedTable, new Column (_initialTable, joinColumns.A), new Column (relatedTable, joinColumns.B));

    //  Assert.AreEqual (expectedJoinTree, result.A);
    //  Assert.AreEqual (relatedTable, result.B);
    //}

    //[Test]
    //public void BuildFieldSourcePath_NestedJoin ()
    //{
    //  MemberInfo[] joinMembers = new MemberInfo[] { _studentDetailMember, _studentMember };
    //  Tuple<IFieldSourcePath, Table> result =
    //      new FieldSourcePathBuilder ().BuildFieldSourcePath (StubDatabaseInfo.Instance, _context, _initialTable, joinMembers);

    //  Table relatedTable1 = DatabaseInfoUtility.GetRelatedTable (StubDatabaseInfo.Instance, _studentDetailMember);
    //  Table relatedTable2 = DatabaseInfoUtility.GetRelatedTable (StubDatabaseInfo.Instance, _studentMember);
    //  Tuple<string, string> joinColumns1 = DatabaseInfoUtility.GetJoinColumnNames (StubDatabaseInfo.Instance, _studentMember);
    //  Tuple<string, string> joinColumns2 = DatabaseInfoUtility.GetJoinColumnNames (StubDatabaseInfo.Instance, _studentDetailMember);

    //  JoinTree expectedInnerJoinTree =
    //      new JoinTree (relatedTable1, relatedTable2, new Column (relatedTable1, joinColumns1.A), new Column (relatedTable2, joinColumns1.B));

    //  JoinTree expectedOuterJoinTree =
    //      new JoinTree (_initialTable, expectedInnerJoinTree, new Column (_initialTable, joinColumns2.A), new Column (relatedTable2, joinColumns2.B));

    //  _context.CreateAliases ();

    //  Assert.AreEqual (expectedOuterJoinTree, result.A);
    //  Assert.AreEqual (relatedTable2, result.B);
    //}
  }
}

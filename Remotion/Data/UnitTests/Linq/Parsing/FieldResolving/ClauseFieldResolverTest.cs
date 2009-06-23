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
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using Remotion.Data.Linq.Clauses.Expressions;
using Rhino.Mocks;
using Remotion.Collections;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing.FieldResolving;
using Mocks_Is = Rhino.Mocks.Constraints.Is;
using Mocks_List = Rhino.Mocks.Constraints.List;
using NUnit.Framework.SyntaxHelpers;

namespace Remotion.Data.UnitTests.Linq.Parsing.FieldResolving
{
  [TestFixture]
  public class ClauseFieldResolverTest
  {
    private JoinedTableContext _context;
    private IResolveFieldAccessPolicy _policy;

    private MainFromClause _studentClause;
    private QuerySourceReferenceExpression _studentReference;

    private MainFromClause _studentDetailClause;
    private QuerySourceReferenceExpression _studentDetailReference;

    private MainFromClause _studentDetailDetailClause;
    private QuerySourceReferenceExpression _studentDetailDetailReference;
    
    private PropertyInfo _studentDetail_Student_Property;
    private PropertyInfo _studentDetailDetail_StudentDetail_Property;
    private PropertyInfo _student_ID_Property;
    private PropertyInfo _student_OtherStudent_Property;
    private PropertyInfo _student_First_Property;

    private MemberExpression _student_First_Expression;
    private MemberExpression _studentDetail_Student_Expression;
    private MemberExpression _studentDetail_Student_First_Expression;
    private MemberExpression _studentDetailDetail_StudentDetail_Student_First_Expression;
    private MemberExpression _studentDetailDetail_StudentDetail_Student_Expression;
    private MemberExpression _studentDetailDetail_StudentDetail_Expression;

    private MockRepository _mockRepository;
    private IResolveFieldAccessPolicy _policyMock;

    [SetUp]
    public void SetUp ()
    {
      _context = new JoinedTableContext();
      _policy = new SelectFieldAccessPolicy();
      _studentClause = ExpressionHelper.CreateMainFromClause (
          Expression.Parameter (typeof (Student), "s"), 
          ExpressionHelper.CreateQuerySource ());
      _studentReference = new QuerySourceReferenceExpression (_studentClause);

      _studentDetailClause = ExpressionHelper.CreateMainFromClause (
          Expression.Parameter (typeof (Student_Detail), "sd"),
          ExpressionHelper.CreateQuerySource_Detail());
      _studentDetailReference = new QuerySourceReferenceExpression (_studentDetailClause);

      _studentDetailDetailClause = ExpressionHelper.CreateMainFromClause (
          Expression.Parameter (typeof (Student_Detail_Detail), "sdd"),
          ExpressionHelper.CreateQuerySource_Detail_Detail());
      _studentDetailDetailReference = new QuerySourceReferenceExpression (_studentDetailDetailClause);

      _student_ID_Property = typeof (Student).GetProperty ("ID");
      _student_First_Property = typeof (Student).GetProperty ("First");
      _student_OtherStudent_Property = typeof (Student).GetProperty ("OtherStudent");
      _studentDetail_Student_Property = typeof (Student_Detail).GetProperty ("Student");
      _studentDetailDetail_StudentDetail_Property = typeof (Student_Detail_Detail).GetProperty ("Student_Detail");

      _student_First_Expression = Expression.MakeMemberAccess (_studentReference, _student_First_Property);
      _studentDetail_Student_Expression = Expression.MakeMemberAccess (_studentDetailReference, _studentDetail_Student_Property);
      _studentDetail_Student_First_Expression = Expression.MakeMemberAccess (_studentDetail_Student_Expression, _student_First_Property);
      _studentDetailDetail_StudentDetail_Expression = 
          Expression.MakeMemberAccess (_studentDetailDetailReference, _studentDetailDetail_StudentDetail_Property);
      _studentDetailDetail_StudentDetail_Student_Expression = 
          Expression.MakeMemberAccess (_studentDetailDetail_StudentDetail_Expression, _studentDetail_Student_Property);
      _studentDetailDetail_StudentDetail_Student_First_Expression =
          Expression.MakeMemberAccess (_studentDetailDetail_StudentDetail_Student_Expression, _student_First_Property);

      _mockRepository = new MockRepository ();
      _policyMock = _mockRepository.StrictMock<IResolveFieldAccessPolicy> ();
    }

    [Test]
    public void Resolve_QuerySourceReferenceExpression_Succeeds ()
    {
      // s

      IColumnSource table = _studentClause.GetColumnSource (StubDatabaseInfo.Instance);
      FieldDescriptor fieldDescriptor = new ClauseFieldResolver (StubDatabaseInfo.Instance, _policy).ResolveField (_studentClause, _studentReference, _context);

      var column = new Column (table, "*");
      var expected = new FieldDescriptor (null, new FieldSourcePath (table, new SingleJoin[0]), column);

      Assert.That (fieldDescriptor, Is.EqualTo (expected));
    }

    [Test]
    public void Resolve_SimpleMemberAccess_Succeeds ()
    {
      // s.First

      FieldDescriptor fieldDescriptor = 
          new ClauseFieldResolver (StubDatabaseInfo.Instance, _policy)
          .ResolveField (_studentClause, _student_First_Expression, _context);
      Assert.That (fieldDescriptor.Column, Is.EqualTo (new Column (new Table ("studentTable", "s"), "FirstColumn")));
    }

    [Test]
    public void Resolve_Join ()
    {
      // sd.Student.First

      FieldDescriptor fieldDescriptor =
          new ClauseFieldResolver (StubDatabaseInfo.Instance, _policy).ResolveField (_studentDetailClause, _studentDetail_Student_First_Expression, _context);

      Assert.That (fieldDescriptor.Column, Is.EqualTo (new Column (new Table ("studentTable", null), "FirstColumn")));
      Assert.That (fieldDescriptor.Member, Is.EqualTo (_student_First_Property));

      IColumnSource expectedSourceTable = fieldDescriptor.SourcePath.FirstSource;
      var expectedRelatedTable = new Table ("studentTable", null);
      var join = new SingleJoin (
          new Column (expectedSourceTable, "Student_Detail_PK"), new Column (expectedRelatedTable, "Student_Detail_to_Student_FK"));
      var expectedPath = new FieldSourcePath (expectedSourceTable, new[] { join });

      Assert.That (fieldDescriptor.SourcePath, Is.EqualTo (expectedPath));
    }

    [Test]
    public void Resolve_DoubleJoin ()
    {
      // sdd.Student_Detail.Student.First

      FieldDescriptor fieldDescriptor =
          new ClauseFieldResolver (StubDatabaseInfo.Instance, _policy).ResolveField (_studentDetailDetailClause, _studentDetailDetail_StudentDetail_Student_First_Expression, _context);

      Assert.That (fieldDescriptor.Column, Is.EqualTo (new Column (new Table ("studentTable", null), "FirstColumn")));
      Assert.That (fieldDescriptor.Member, Is.EqualTo (_student_First_Property));

      IColumnSource expectedDetailDetailTable = fieldDescriptor.SourcePath.FirstSource;
      var expectedDetailTable = new Table ("detailTable", null); // Student_Detail
      var join1 = new SingleJoin (
          new Column (expectedDetailDetailTable, "Student_Detail_Detail_PK"),
          new Column (expectedDetailTable, "Student_Detail_Detail_to_Student_Detail_FK"));

      var expectedStudentTable = new Table ("studentTable", null); // Student
      var join2 = new SingleJoin (
          new Column (expectedDetailTable, "Student_Detail_PK"), new Column (expectedStudentTable, "Student_Detail_to_Student_FK"));

      var expectedPath = new FieldSourcePath (expectedDetailDetailTable, new[] { join1, join2 });
      Assert.That (fieldDescriptor.SourcePath, Is.EqualTo (expectedPath));
    }

    [Test]
    [ExpectedException (typeof (FieldAccessResolveException), ExpectedMessage = "The member 'Remotion.Data.UnitTests.Linq.Student.First' does not "
                                                                                + "identify a relation.")]
    public void Resolve_Join_InvalidMember ()
    {
      // s.First.Length
      Expression fieldExpression =
          Expression.MakeMemberAccess (
              _student_First_Expression,
              typeof (string).GetProperty ("Length"));

      new ClauseFieldResolver (StubDatabaseInfo.Instance, _policy).ResolveField (_studentClause, fieldExpression, _context);
    }

    [Test]
    public void Resolve_SimpleMemberAccess_InvalidField ()
    {
      IColumnSource table = _studentClause.GetColumnSource (StubDatabaseInfo.Instance);

      Expression fieldExpression = Expression.MakeMemberAccess (
          _studentReference,
          typeof (Student).GetProperty ("NonDBProperty"));

      FieldDescriptor fieldDescriptor =
          new ClauseFieldResolver (StubDatabaseInfo.Instance, _policy).ResolveField (_studentClause, fieldExpression, _context);

      var path = new FieldSourcePath (table, new SingleJoin[0]);
      Assert.That (fieldDescriptor, Is.EqualTo (new FieldDescriptor (typeof (Student).GetProperty ("NonDBProperty"), path, null)));
    }

    [Test]
    public void Resolve_UsesContext ()
    {
      // sd.Student.First

      FieldDescriptor fieldDescriptor1 =
          new ClauseFieldResolver (StubDatabaseInfo.Instance, _policy).ResolveField (_studentClause, _studentDetail_Student_First_Expression, _context);
      FieldDescriptor fieldDescriptor2 =
          new ClauseFieldResolver (StubDatabaseInfo.Instance, _policy).ResolveField (_studentClause, _studentDetail_Student_First_Expression, _context);

      IColumnSource table1 = fieldDescriptor1.SourcePath.Joins[0].RightSide;
      IColumnSource table2 = fieldDescriptor2.SourcePath.Joins[0].RightSide;

      Assert.That (table2, Is.SameAs (table1));
    }

    [Test]
    public void Resolve_EntityField_Simple ()
    {
      //sd.Student

      FieldDescriptor fieldDescriptor =
          new ClauseFieldResolver (StubDatabaseInfo.Instance, _policy).ResolveField (_studentDetailClause, _studentDetail_Student_Expression, _context);

      IColumnSource detailTable = _studentDetailClause.GetColumnSource (StubDatabaseInfo.Instance);
      Table studentTable = DatabaseInfoUtility.GetRelatedTable (StubDatabaseInfo.Instance, _studentDetail_Student_Property);
      Tuple<string, string> joinColumns = DatabaseInfoUtility.GetJoinColumnNames (StubDatabaseInfo.Instance, _studentDetail_Student_Property);
      var join = new SingleJoin (new Column (detailTable, joinColumns.A), new Column (studentTable, joinColumns.B));
      var column = new Column (studentTable, "*");

      var expected = new FieldDescriptor (_studentDetail_Student_Property, new FieldSourcePath (detailTable, new[] { join }), column);
      Assert.That (fieldDescriptor, Is.EqualTo (expected));
    }

    [Test]
    public void Resolve_EntityField_Nested ()
    {
      //sdd.Student_Detail.Student

      FieldDescriptor fieldDescriptor =
          new ClauseFieldResolver (StubDatabaseInfo.Instance, _policy).ResolveField (_studentDetailDetailClause, _studentDetailDetail_StudentDetail_Student_Expression, _context);

      IColumnSource detailDetailTable = _studentDetailDetailClause.GetColumnSource (StubDatabaseInfo.Instance);
      Table detailTable = DatabaseInfoUtility.GetRelatedTable (StubDatabaseInfo.Instance, _studentDetailDetail_StudentDetail_Property);
      Table studentTable = DatabaseInfoUtility.GetRelatedTable (StubDatabaseInfo.Instance, _studentDetail_Student_Property);
      Tuple<string, string> innerJoinColumns = DatabaseInfoUtility.GetJoinColumnNames (StubDatabaseInfo.Instance, _studentDetailDetail_StudentDetail_Property);
      Tuple<string, string> outerJoinColumns = DatabaseInfoUtility.GetJoinColumnNames (StubDatabaseInfo.Instance, _studentDetail_Student_Property);

      var join1 = new SingleJoin (new Column (detailDetailTable, innerJoinColumns.A), new Column (detailTable, innerJoinColumns.B));
      var join2 = new SingleJoin (new Column (detailTable, outerJoinColumns.A), new Column (studentTable, outerJoinColumns.B));
      var column = new Column (studentTable, "*");

      var expected = new FieldDescriptor (_studentDetail_Student_Property, new FieldSourcePath (detailDetailTable, new[] { join1, join2 }), column);
      Assert.That (fieldDescriptor, Is.EqualTo (expected));
    }

    [Test]
    public void Resolve_FieldFromSubQuery ()
    {
      // from x in (...)
      // select x.ID
      SubQueryFromClause fromClause = ExpressionHelper.CreateSubQueryFromClause (Expression.Parameter (typeof (Student), "x"));

      PropertyInfo member = typeof (Student).GetProperty ("ID");
      Expression fieldExpression = Expression.MakeMemberAccess (new QuerySourceReferenceExpression (fromClause), member);

      FieldDescriptor fieldDescriptor =
          new ClauseFieldResolver (StubDatabaseInfo.Instance, _policy).ResolveField (fromClause, fieldExpression, _context);
      var subQuery = (SubQuery) fromClause.GetColumnSource (StubDatabaseInfo.Instance);
      var column = new Column (subQuery, "IDColumn");
      var expected = new FieldDescriptor (member, new FieldSourcePath (subQuery, new SingleJoin[0]), column);

      Assert.That (fieldDescriptor, Is.EqualTo (expected));
    }

    [Test]
    public void Resolver_UsesPolicyToAdjustRelationMembers ()
    {
      _policyMock.Expect (mock => mock.OptimizeRelatedKeyAccess ()).Return (false);
      var newJoinMembers = new[] { _studentDetailDetail_StudentDetail_Property, _studentDetail_Student_Property };
      _policyMock.Expect (
          mock => mock.AdjustMemberInfosForRelation (
              Arg.Is<MemberInfo> (_studentDetail_Student_Property), 
              Arg<IEnumerable<MemberInfo>>.List.Equal (new[] { _studentDetailDetail_StudentDetail_Property })))
          .Return (new Tuple<MemberInfo, IEnumerable<MemberInfo>> (_student_ID_Property, newJoinMembers));

      _policyMock.Replay();

      FieldDescriptor actualFieldDescriptor = new ClauseFieldResolver (StubDatabaseInfo.Instance, _policyMock)
          .ResolveField (_studentDetailDetailClause, _studentDetailDetail_StudentDetail_Student_Expression, _context);
      
      _policyMock.VerifyAllExpectations();

      var expectedPath = new FieldSourcePathBuilder().BuildFieldSourcePath (
          StubDatabaseInfo.Instance,
          _context,
          _studentDetailDetailClause.GetColumnSource (StubDatabaseInfo.Instance),
          newJoinMembers);
      var expectedFieldDescriptor = new FieldDescriptor (_studentDetail_Student_Property, expectedPath, new Column (expectedPath.LastSource, "IDColumn"));
      Assert.That (actualFieldDescriptor, Is.EqualTo (expectedFieldDescriptor));
    }

    [Test]
    public void Resolver_UsesPolicyToAdjustFromIdentifierAccess ()
    {
      _policyMock.Expect (mock => mock.OptimizeRelatedKeyAccess()).Return (false);

      Expression fieldExpression = _studentDetailDetailReference;

      var newJoinMembers = new[] { _student_OtherStudent_Property };
      _policyMock
          .Expect (mock => mock.AdjustMemberInfosForDirectAccessOfQuerySource (_studentDetailDetailReference))
          .Return (new Tuple<MemberInfo, IEnumerable<MemberInfo>> (_student_ID_Property, newJoinMembers));

      _policyMock.Replay ();

      FieldDescriptor actualFieldDescriptor = 
          new ClauseFieldResolver (StubDatabaseInfo.Instance, _policyMock)
          .ResolveField (_studentDetailDetailClause, fieldExpression, _context);
      
      _policyMock.VerifyAllExpectations();

      FieldSourcePath path = new FieldSourcePathBuilder().BuildFieldSourcePath (
          StubDatabaseInfo.Instance,
          _context,
          _studentDetailDetailClause.GetColumnSource (StubDatabaseInfo.Instance),
          newJoinMembers);
      var expectedFieldDescriptor = new FieldDescriptor (null, path, new Column (path.LastSource, "IDColumn"));
      Assert.That (actualFieldDescriptor, Is.EqualTo (expectedFieldDescriptor));
    }

    [Test]
    public void Resolver_PolicyOptimization_True ()
    {
      IResolveFieldAccessPolicy policy = new WhereFieldAccessPolicy (StubDatabaseInfo.Instance);
      Assert.That (policy.OptimizeRelatedKeyAccess(), Is.True);
      Expression fieldExpression = ExpressionHelper.Resolve<Student_Detail, int> (_studentDetailClause, sd => sd.IndustrialSector.ID);

      var resolver = new ClauseFieldResolver (StubDatabaseInfo.Instance, policy);

      FieldDescriptor result = resolver.ResolveField (_studentDetailClause, fieldExpression, _context);

      Assert.That (
          result.Column, Is.EqualTo (new Column (_studentDetailClause.GetColumnSource (StubDatabaseInfo.Instance), "Student_Detail_to_IndustrialSector_FK")));
    }

    [Test]
    public void Resolver_PolicyOptimization_False ()
    {
      IResolveFieldAccessPolicy policy = new SelectFieldAccessPolicy();
      Assert.That (policy.OptimizeRelatedKeyAccess(), Is.False);
      Expression fieldExpression = ExpressionHelper.Resolve<Student_Detail, int>(_studentDetailClause, sd => sd.IndustrialSector.ID);

      var resolver = new ClauseFieldResolver (StubDatabaseInfo.Instance, policy);
      FieldDescriptor result = resolver.ResolveField (_studentDetailClause, fieldExpression, _context);

      Assert.That (result.Column, Is.EqualTo (new Column (result.SourcePath.LastSource, "IDColumn")));
    }
  }
}
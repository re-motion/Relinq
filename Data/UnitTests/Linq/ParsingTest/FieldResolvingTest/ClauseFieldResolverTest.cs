/* Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh
 *
 * This program is free software: you can redistribute it and/or modify it under 
 * the terms of the re:motion license agreement in license.txt. If you did not 
 * receive it, please visit http://www.re-motion.org/licensing.
 * 
 * Unless otherwise provided, this software is distributed on an "AS IS" basis, 
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 */

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using Rhino.Mocks;
using Remotion.Collections;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing.FieldResolving;

using Mocks_Is = Rhino.Mocks.Constraints.Is;
using Mocks_List = Rhino.Mocks.Constraints.List;
using NUnit.Framework.SyntaxHelpers;

namespace Remotion.Data.UnitTests.Linq.ParsingTest.FieldResolvingTest
{
  [TestFixture]
  public class ClauseFieldResolverTest
  {
    private JoinedTableContext _context;
    private IResolveFieldAccessPolicy _policy;

    [SetUp]
    public void SetUp ()
    {
      _context = new JoinedTableContext();
      _policy = new SelectFieldAccessPolicy();
    }

    [Test]
    public void Resolve_ParameterAccess_Succeeds ()
    {
      ParameterExpression identifier = Expression.Parameter (typeof (Student), "fromIdentifier1");
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause(identifier, ExpressionHelper.CreateQuerySource());

      IColumnSource table = fromClause.GetFromSource (StubDatabaseInfo.Instance);

      FieldDescriptor fieldDescriptor =
          new ClauseFieldResolver (StubDatabaseInfo.Instance, _policy).ResolveField (table, identifier, identifier, identifier, _context);
      
      var column = new Column (table, "*");
      var expected = new FieldDescriptor (null,new FieldSourcePath(table, new SingleJoin[0]), column);

      Assert.AreEqual (expected, fieldDescriptor);
    }

    [Test]
    [ExpectedException (typeof (FieldAccessResolveException),
        ExpectedMessage = "This clause can only resolve field accesses for parameters "
            + "called 'fromIdentifier1', but a parameter called 'fromIdentifier5' was given.")]
    public void Resolve_ParameterAccess_InvalidParameterName ()
    {
      ParameterExpression identifier = Expression.Parameter (typeof (Student), "fromIdentifier1");
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause(identifier, ExpressionHelper.CreateQuerySource());

      IColumnSource table = fromClause.GetFromSource (StubDatabaseInfo.Instance);

      ParameterExpression identifier2 = Expression.Parameter (typeof (Student), "fromIdentifier5");
      new ClauseFieldResolver (StubDatabaseInfo.Instance, _policy).ResolveField (table, identifier, identifier2, identifier2, _context);
    }

    [Test]
    [ExpectedException (typeof (FieldAccessResolveException),
        ExpectedMessage = "This clause can only resolve field accesses for parameters of "
            + "type 'Remotion.Data.UnitTests.Linq.Student', but a parameter of type 'System.String' was given.")]
    public void Resolve_ParameterAccess_InvalidParameterType ()
    {
      ParameterExpression identifier = Expression.Parameter (typeof (Student), "fromIdentifier1");
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause(identifier, ExpressionHelper.CreateQuerySource());
      
      IColumnSource table = fromClause.GetFromSource (StubDatabaseInfo.Instance);

      ParameterExpression identifier2 = Expression.Parameter (typeof (string), "fromIdentifier1");
      new ClauseFieldResolver (StubDatabaseInfo.Instance, _policy).ResolveField (table, identifier, identifier2, identifier2, _context);
    }

    [Test]
    public void Resolve_SimpleMemberAccess_Succeeds ()
    {
      ParameterExpression identifier = Expression.Parameter (typeof (Student), "fromIdentifier1");
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause(identifier, ExpressionHelper.CreateQuerySource());
      IColumnSource table = fromClause.GetFromSource (StubDatabaseInfo.Instance);

      Expression fieldExpression = Expression.MakeMemberAccess (Expression.Parameter (typeof (Student), "fromIdentifier1"),
          typeof (Student).GetProperty ("First"));
      FieldDescriptor fieldDescriptor =
          new ClauseFieldResolver (StubDatabaseInfo.Instance, _policy).ResolveField (table,identifier, fieldExpression, fieldExpression, _context);
      Assert.AreEqual (new Column (new Table ("studentTable", "fromIdentifier1"), "FirstColumn"), fieldDescriptor.Column);
    }

    [Test]
    [ExpectedException (typeof (FieldAccessResolveException), ExpectedMessage = "This clause can only resolve field accesses for parameters "
        + "called 'fzlbf', but a parameter called 'fromIdentifier1' was given.")]
    public void Resolve_SimpleMemberAccess_InvalidName ()
    {
      ParameterExpression identifier = Expression.Parameter (typeof (Student), "fzlbf");
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause(identifier, ExpressionHelper.CreateQuerySource());
      IColumnSource table = fromClause.GetFromSource (StubDatabaseInfo.Instance);

      Expression fieldExpression = Expression.MakeMemberAccess (Expression.Parameter (typeof (Student), "fromIdentifier1"),
          typeof (Student).GetProperty ("First"));
      new ClauseFieldResolver (StubDatabaseInfo.Instance, _policy).ResolveField (table,identifier, fieldExpression, fieldExpression, _context);
    }

    [Test]
    [ExpectedException (typeof (FieldAccessResolveException), ExpectedMessage = "This clause can only resolve field accesses for parameters of "
        + "type 'Remotion.Data.UnitTests.Linq.Student', but a parameter of type 'Remotion.Data.UnitTests.Linq.Student_Detail' was given.")]
    public void Resolve_SimpleMemberAccess_InvalidType ()
    {
      ParameterExpression identifier = Expression.Parameter (typeof (Student), "fromIdentifier1");
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause(identifier, ExpressionHelper.CreateQuerySource());
      IColumnSource table = fromClause.GetFromSource (StubDatabaseInfo.Instance);

      Expression fieldExpression = Expression.MakeMemberAccess (Expression.Parameter (typeof (Student_Detail), "fromIdentifier1"),
          typeof (Student_Detail).GetProperty ("Student"));
      new ClauseFieldResolver (StubDatabaseInfo.Instance, _policy).ResolveField (table,identifier, fieldExpression, fieldExpression, _context);
    }

    [Test]
    public void Resolve_Join ()
    {
      // sd.Student.First
      ParameterExpression identifier = Expression.Parameter (typeof (Student_Detail), "sd");
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause(identifier, ExpressionHelper.CreateQuerySource_Detail());
      IColumnSource table = fromClause.GetFromSource (StubDatabaseInfo.Instance);

      Expression fieldExpression =
          Expression.MakeMemberAccess (
              Expression.MakeMemberAccess (Expression.Parameter (typeof (Student_Detail), "sd"),
                  typeof (Student_Detail).GetProperty ("Student")),
              typeof (Student).GetProperty ("First"));

      FieldDescriptor fieldDescriptor =
          new ClauseFieldResolver (StubDatabaseInfo.Instance, _policy).ResolveField (table,identifier, fieldExpression, fieldExpression, _context);

      Assert.AreEqual (new Column (new Table ("studentTable", null), "FirstColumn"), fieldDescriptor.Column);
      //Assert.AreSame (fromClause, fieldDescriptor.FromClause);
      Assert.AreEqual (typeof (Student).GetProperty ("First"), fieldDescriptor.Member);

      IColumnSource expectedSourceTable = fieldDescriptor.SourcePath.FirstSource;
      var expectedRelatedTable = new Table ("studentTable", null);
      var join = new SingleJoin(new Column (expectedSourceTable, "Student_Detail_PK"), new Column (expectedRelatedTable, "Student_Detail_to_Student_FK"));
      var expectedPath = new FieldSourcePath(expectedSourceTable, new [] { join });

      Assert.AreEqual (expectedPath, fieldDescriptor.SourcePath);
    }

    [Test]
    public void Resolve_DoubleJoin ()
    {
      // sdd.Student_Detail.Student.First
      ParameterExpression identifier = Expression.Parameter (typeof (Student_Detail_Detail), "sdd");
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause(identifier, ExpressionHelper.CreateQuerySource_Detail_Detail());
      IColumnSource table = fromClause.GetFromSource (StubDatabaseInfo.Instance);

      Expression fieldExpression =
          Expression.MakeMemberAccess (
              Expression.MakeMemberAccess (
                  Expression.MakeMemberAccess (Expression.Parameter (typeof (Student_Detail_Detail), "sdd"),
                      typeof (Student_Detail_Detail).GetProperty ("Student_Detail")),
                  typeof (Student_Detail).GetProperty ("Student")),
              typeof (Student).GetProperty ("First"));

      FieldDescriptor fieldDescriptor =
          new ClauseFieldResolver (StubDatabaseInfo.Instance, _policy).ResolveField (table,identifier, fieldExpression, fieldExpression, _context);

      Assert.AreEqual (new Column (new Table ("studentTable", null), "FirstColumn"), fieldDescriptor.Column);
      Assert.AreEqual (typeof (Student).GetProperty ("First"), fieldDescriptor.Member);

      IColumnSource expectedDetailDetailTable = fieldDescriptor.SourcePath.FirstSource;
      var expectedDetailTable = new Table ("detailTable", null); // Student_Detail
      var join1 = new SingleJoin (new Column (expectedDetailDetailTable, "Student_Detail_Detail_PK"), new Column (expectedDetailTable, "Student_Detail_Detail_to_Student_Detail_FK"));
      
      var expectedStudentTable = new Table ("studentTable", null); // Student
      var join2 = new SingleJoin(new Column (expectedDetailTable, "Student_Detail_PK"), new Column (expectedStudentTable, "Student_Detail_to_Student_FK"));

      var expectedPath = new FieldSourcePath(expectedDetailDetailTable, new[] { join1, join2 });
      Assert.AreEqual (expectedPath, fieldDescriptor.SourcePath);
    }

    [Test]
    [ExpectedException (typeof (FieldAccessResolveException), ExpectedMessage = "The member 'Remotion.Data.UnitTests.Linq.Student.First' does not "
        + "identify a relation.")]
    public void Resolve_Join_InvalidMember ()
    {
      // s.First.Length
      ParameterExpression identifier = Expression.Parameter (typeof (Student), "s");
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause(identifier, ExpressionHelper.CreateQuerySource_Detail());
      IColumnSource table = fromClause.GetFromSource (StubDatabaseInfo.Instance);

      Expression fieldExpression =
          Expression.MakeMemberAccess (
              Expression.MakeMemberAccess (Expression.Parameter (typeof (Student), "s"),
                  typeof (Student).GetProperty ("First")),
              typeof (string).GetProperty ("Length"));

      new ClauseFieldResolver (StubDatabaseInfo.Instance, _policy).ResolveField (table,identifier, fieldExpression, fieldExpression, _context);
    }

    [Test]
    public void Resolve_SimpleMemberAccess_InvalidField ()
    {
      ParameterExpression identifier = Expression.Parameter (typeof (Student), "fromIdentifier1");
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause(identifier, ExpressionHelper.CreateQuerySource());
      IColumnSource table = fromClause.GetFromSource (StubDatabaseInfo.Instance);

      Expression fieldExpression = Expression.MakeMemberAccess (Expression.Parameter (typeof (Student), "fromIdentifier1"),
          typeof (Student).GetProperty ("NonDBProperty"));
      FieldDescriptor fieldDescriptor =
          new ClauseFieldResolver (StubDatabaseInfo.Instance, _policy).ResolveField (table,identifier,fieldExpression, fieldExpression, _context);
      var path = new FieldSourcePath (table, new SingleJoin[0]);
      Assert.AreEqual (new FieldDescriptor (typeof (Student).GetProperty ("NonDBProperty"), path, null), fieldDescriptor);
    }

    [Test]
    public void Resolve_UsesContext ()
    {
      // sd.Student.First
      ParameterExpression identifier = Expression.Parameter (typeof (Student_Detail), "sd");
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause(identifier, ExpressionHelper.CreateQuerySource_Detail());
      IColumnSource table = fromClause.GetFromSource (StubDatabaseInfo.Instance);

      Expression fieldExpression =
          Expression.MakeMemberAccess (
              Expression.MakeMemberAccess (Expression.Parameter (typeof (Student_Detail), "sd"),
                  typeof (Student_Detail).GetProperty ("Student")),
              typeof (Student).GetProperty ("First"));

      FieldDescriptor fieldDescriptor1 =
          new ClauseFieldResolver (StubDatabaseInfo.Instance, _policy).ResolveField (table,identifier, fieldExpression, fieldExpression, _context);
      FieldDescriptor fieldDescriptor2 =
          new ClauseFieldResolver (StubDatabaseInfo.Instance, _policy).ResolveField (table, identifier, fieldExpression, fieldExpression, _context);

      IColumnSource table1 = fieldDescriptor1.SourcePath.Joins[0].RightSide;
      IColumnSource table2 = fieldDescriptor2.SourcePath.Joins[0].RightSide;

      Assert.AreSame (table1, table2);
    }

    [Test]
    public void Resolve_EntityField_Simple ()
    {
      //sd.Student
      ParameterExpression identifier = Expression.Parameter (typeof (Student_Detail), "sd");
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause(identifier, ExpressionHelper.CreateQuerySource_Detail ());
      IColumnSource table = fromClause.GetFromSource (StubDatabaseInfo.Instance);

      PropertyInfo member = typeof (Student_Detail).GetProperty ("Student");
      Expression fieldExpression =
          Expression.MakeMemberAccess (
              Expression.Parameter (typeof (Student_Detail), "sd"),
                  member);

      FieldDescriptor fieldDescriptor =
          new ClauseFieldResolver (StubDatabaseInfo.Instance, _policy).ResolveField (table,identifier, fieldExpression, fieldExpression, _context);

      IColumnSource detailTable = fromClause.GetFromSource (StubDatabaseInfo.Instance);
      Table studentTable = DatabaseInfoUtility.GetRelatedTable (StubDatabaseInfo.Instance, member);
      Tuple<string, string> joinColumns = DatabaseInfoUtility.GetJoinColumnNames (StubDatabaseInfo.Instance, member);
      var join = new SingleJoin (new Column(detailTable, joinColumns.A), new Column(studentTable, joinColumns.B));
      var column = new Column(studentTable, "*");

      var expected = new FieldDescriptor(member, new FieldSourcePath (detailTable, new[] { join }), column);
      Assert.AreEqual (expected, fieldDescriptor);
    }

    [Test]
    public void Resolve_EntityField_Nested ()
    {
      //sdd.Student_Detail.Student
      ParameterExpression identifier = Expression.Parameter (typeof (Student_Detail_Detail), "sdd");
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause(identifier, ExpressionHelper.CreateQuerySource_Detail_Detail ());
      IColumnSource table = fromClause.GetFromSource (StubDatabaseInfo.Instance);

      PropertyInfo member = typeof (Student_Detail).GetProperty ("Student");
      PropertyInfo innerRelationMember = typeof (Student_Detail_Detail).GetProperty ("Student_Detail");
      Expression fieldExpression =
          Expression.MakeMemberAccess (
              Expression.MakeMemberAccess (
                  Expression.Parameter (typeof (Student_Detail_Detail), "sdd"),
                  innerRelationMember),
              member);

      FieldDescriptor fieldDescriptor =
          new ClauseFieldResolver (StubDatabaseInfo.Instance, _policy).ResolveField (table,identifier, fieldExpression, fieldExpression, _context);

      IColumnSource detailDetailTable = fromClause.GetFromSource (StubDatabaseInfo.Instance);
      Table detailTable = DatabaseInfoUtility.GetRelatedTable (StubDatabaseInfo.Instance, innerRelationMember);
      Table studentTable = DatabaseInfoUtility.GetRelatedTable (StubDatabaseInfo.Instance, member);
      Tuple<string, string> innerJoinColumns = DatabaseInfoUtility.GetJoinColumnNames (StubDatabaseInfo.Instance, innerRelationMember);
      Tuple<string, string> outerJoinColumns = DatabaseInfoUtility.GetJoinColumnNames (StubDatabaseInfo.Instance, member);

      var join1 = new SingleJoin (new Column (detailDetailTable, innerJoinColumns.A), new Column (detailTable, innerJoinColumns.B));
      var join2 = new SingleJoin (new Column (detailTable, outerJoinColumns.A), new Column (studentTable, outerJoinColumns.B));
      var column = new Column (studentTable, "*");

      var expected = new FieldDescriptor (member, new FieldSourcePath (detailDetailTable, new[] { join1, join2 }), column);
      Assert.AreEqual (expected, fieldDescriptor);
    }

    [Test]
    public void Resolve_FieldFromSubQuery ()
    {
      // from x in (...)
      // select x.ID
      ParameterExpression identifier = Expression.Parameter (typeof (Student), "x");
      SubQueryFromClause fromClause = ExpressionHelper.CreateSubQueryFromClause(identifier);
      IColumnSource table = fromClause.GetFromSource (StubDatabaseInfo.Instance);

      PropertyInfo member = typeof (Student).GetProperty ("ID");
      Expression fieldExpression = Expression.MakeMemberAccess (identifier, member);

      FieldDescriptor fieldDescriptor =
          new ClauseFieldResolver (StubDatabaseInfo.Instance, _policy).ResolveField (table,identifier, fieldExpression, fieldExpression, _context);
      var subQuery = (SubQuery) fromClause.GetFromSource (StubDatabaseInfo.Instance);
      var column = new Column (subQuery, "IDColumn");
      var expected = new FieldDescriptor (member, new FieldSourcePath (subQuery, new SingleJoin[0]), column);

      Assert.AreEqual (expected, fieldDescriptor);
    }

    [Test]
    public void Resolver_UsesPolicyToAdjustRelationMembers ()
    {
      var mockRepository = new MockRepository ();
      var policyMock = mockRepository.StrictMock<IResolveFieldAccessPolicy> ();

      ParameterExpression identifier = Expression.Parameter (typeof (Student_Detail_Detail), "sdd");
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause (identifier, ExpressionHelper.CreateQuerySource_Detail_Detail ());
      IColumnSource table = fromClause.GetFromSource (StubDatabaseInfo.Instance);

      Expect.Call (policyMock.OptimizeRelatedKeyAccess ()).Return (false);

      var studentDetailMember = typeof (Student_Detail_Detail).GetProperty ("Student_Detail");
      var studentMember = typeof (Student_Detail).GetProperty ("Student");
      var idMember = typeof (Student).GetProperty ("ID");
      Expression fieldExpression = Expression.MakeMemberAccess (Expression.MakeMemberAccess (identifier, studentDetailMember), studentMember);

      var newJoinMembers = new[] {studentDetailMember, studentMember};
      Expect.Call (policyMock.AdjustMemberInfosForRelation (null, null))
          .Constraints (Mocks_Is.Equal (studentMember), Mocks_List.Equal (new[] { studentDetailMember }))
          .Return (new  Tuple<MemberInfo, IEnumerable<MemberInfo>>(idMember, newJoinMembers));

      mockRepository.ReplayAll ();
      FieldDescriptor actualFieldDescriptor =
          new ClauseFieldResolver (StubDatabaseInfo.Instance, policyMock).ResolveField (table,identifier, fieldExpression, fieldExpression, _context);
      mockRepository.VerifyAll ();

      var path = new FieldSourcePathBuilder().BuildFieldSourcePath (StubDatabaseInfo.Instance, _context, 
          fromClause.GetFromSource (StubDatabaseInfo.Instance), newJoinMembers);
      var expectedFieldDescriptor = new FieldDescriptor (studentMember, path, new Column(path.LastSource, "IDColumn"));

      Assert.That (actualFieldDescriptor, Is.EqualTo (expectedFieldDescriptor));
    }

    [Test]
    public void Resolver_UsesPolicyToAdjustFromIdentifierAccess ()
    {
      var mockRepository = new MockRepository ();
      var policyMock = mockRepository.StrictMock<IResolveFieldAccessPolicy> ();

      ParameterExpression identifier = Expression.Parameter (typeof (Student), "s");
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause (identifier, ExpressionHelper.CreateQuerySource_Detail_Detail ());
      IColumnSource table = fromClause.GetFromSource (StubDatabaseInfo.Instance);

      Expect.Call (policyMock.OptimizeRelatedKeyAccess()).Return (false);
      
      var otherStudentMember = typeof (Student).GetProperty ("OtherStudent");
      var idMember = typeof (Student).GetProperty ("ID");
      Expression fieldExpression = identifier;

      var newJoinMembers = new[] { otherStudentMember };
      Expect.Call (policyMock.AdjustMemberInfosForAccessedIdentifier (identifier))
          .Return (new Tuple<MemberInfo, IEnumerable<MemberInfo>> (idMember, newJoinMembers));

      mockRepository.ReplayAll ();
      FieldDescriptor actualFieldDescriptor =
          new ClauseFieldResolver (StubDatabaseInfo.Instance, policyMock).ResolveField (table,identifier, fieldExpression, fieldExpression, _context);
      mockRepository.VerifyAll ();

      FieldSourcePath path = new FieldSourcePathBuilder ().BuildFieldSourcePath (StubDatabaseInfo.Instance, _context,
          fromClause.GetFromSource (StubDatabaseInfo.Instance), newJoinMembers);
      var expectedFieldDescriptor = new FieldDescriptor (null, path, new Column (path.LastSource, "IDColumn"));

      Assert.That (actualFieldDescriptor, Is.EqualTo (expectedFieldDescriptor));
    }

    [Test]
    public void Resolver_PolicyOptimization_True ()
    {
      IResolveFieldAccessPolicy policy = new WhereFieldAccessPolicy (StubDatabaseInfo.Instance);
      Assert.That (policy.OptimizeRelatedKeyAccess (), Is.True);
      Expression fieldExpression = ExpressionHelper.MakeExpression<Student_Detail, int> (sd => sd.IndustrialSector.ID);
      
      var resolver = new ClauseFieldResolver (StubDatabaseInfo.Instance, policy);
      var columnSource = new Table ();
      var clauseIdentifier = Expression.Parameter (typeof (Student_Detail), "sd");
      FieldDescriptor result = resolver.ResolveField (columnSource, clauseIdentifier, fieldExpression, fieldExpression, _context);

      Assert.That (result.Column, Is.EqualTo (new Column (columnSource, "Student_Detail_to_IndustrialSector_FK")));
    }

    [Test]
    public void Resolver_PolicyOptimization_False ()
    {
      IResolveFieldAccessPolicy policy = new SelectFieldAccessPolicy ();
      Assert.That (policy.OptimizeRelatedKeyAccess (), Is.False);
      Expression fieldExpression = ExpressionHelper.MakeExpression<Student_Detail, int> (sd => sd.IndustrialSector.ID);

      var resolver = new ClauseFieldResolver (StubDatabaseInfo.Instance, policy);
      var columnSource = new Table ();
      var clauseIdentifier = Expression.Parameter (typeof (Student_Detail), "sd");
      FieldDescriptor result = resolver.ResolveField (columnSource, clauseIdentifier, fieldExpression, fieldExpression, _context);

      Assert.That (result.Column, Is.EqualTo (new Column (result.SourcePath.LastSource, "IDColumn")));
    }
  }
}

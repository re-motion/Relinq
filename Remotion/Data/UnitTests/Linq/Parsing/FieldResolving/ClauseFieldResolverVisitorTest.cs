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
using NUnit.Framework;
using System.Linq.Expressions;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Parsing.FieldResolving;
using Remotion.Data.UnitTests.Linq.Parsing.ExpressionTreeVisitors;

namespace Remotion.Data.UnitTests.Linq.Parsing.FieldResolving
{
  [TestFixture]
  public class ClauseFieldResolverVisitorTest
  {
    private MainFromClause _studentClause;
    private ParameterExpression _studentParameter;

    private MainFromClause _studentDetailClause;
    private ParameterExpression _studentDetailParameter;

    private MainFromClause _studentDetailDetailClause;
    private ParameterExpression _studentDetailDetailParameter;

    [SetUp]
    public void SetUp ()
    {
      _studentParameter = Expression.Parameter (typeof (Student), "fromIdentifier1");
      _studentClause = ExpressionHelper.CreateMainFromClause (_studentParameter, ExpressionHelper.CreateQuerySource());

      _studentDetailParameter = Expression.Parameter (typeof (Student_Detail), "sd");
      _studentDetailClause = ExpressionHelper.CreateMainFromClause (_studentDetailParameter, ExpressionHelper.CreateQuerySource_Detail());

      _studentDetailDetailParameter = Expression.Parameter (typeof (Student_Detail_Detail), "sdd");
      _studentDetailDetailClause = ExpressionHelper.CreateMainFromClause (
          _studentDetailDetailParameter, ExpressionHelper.CreateQuerySource_Detail_Detail());
    }

    [Test]
    public void QuerySourceReferenceExpression ()
    {
      var referenceExpression = new QuerySourceReferenceExpression (_studentClause);
      var result = ClauseFieldResolverVisitor.ParseFieldAccess(StubDatabaseInfo.Instance, _studentClause, referenceExpression, true);
      Assert.That (result.AccessedMember, Is.Null);
      Assert.That (result.JoinMembers, Is.Empty);
    }

    [Test]
    [ExpectedException (typeof (FieldAccessResolveException), ExpectedMessage = "This clause can only resolve field accesses for itself ('sd'), but " 
        + "a reference to a clause called 'fromIdentifier1' was given.")]
    public void QuerySourceReferenceExpression_InvalidClause ()
    {
      var referenceExpression = new QuerySourceReferenceExpression (_studentClause);
      ClauseFieldResolverVisitor.ParseFieldAccess (StubDatabaseInfo.Instance, _studentDetailClause, referenceExpression, true);
    }

    [Test]
    public void Parameter ()
    {
      FieldAccessInfo result = ClauseFieldResolverVisitor.ParseFieldAccess(StubDatabaseInfo.Instance, _studentClause, _studentParameter, true);
      Assert.That (result.AccessedMember, Is.Null);
      Assert.That (result.JoinMembers, Is.Empty);
    }

    [Test]
    public void NestedParameters ()
    {
      Expression expressionTree = Expression.MakeMemberAccess (_studentParameter, typeof (Student).GetProperty ("First"));
      FieldAccessInfo result = ClauseFieldResolverVisitor.ParseFieldAccess (StubDatabaseInfo.Instance, _studentClause, expressionTree, true);
      Assert.That (result.AccessedMember, Is.EqualTo (typeof (Student).GetProperty ("First")));
      Assert.That (result.JoinMembers, Is.Empty);
    }

    [Test]
    [ExpectedException (typeof (FieldAccessResolveException),
        ExpectedMessage = "This clause can only resolve field accesses for parameters "
                          + "called 'fromIdentifier1', but a parameter called 'fromIdentifier5' was given.")]
    public void Parameter_InvalidParameterName ()
    {
      ParameterExpression identifier2 = Expression.Parameter (typeof (Student), "fromIdentifier5");
      ClauseFieldResolverVisitor.ParseFieldAccess (StubDatabaseInfo.Instance, _studentClause, identifier2, true);
    }

    [Test]
    [ExpectedException (typeof (FieldAccessResolveException),
        ExpectedMessage = "This clause can only resolve field accesses for parameters of "
                          + "type 'Remotion.Data.UnitTests.Linq.Student', but a parameter of type 'System.String' was given.")]
    public void ParameterAccess_InvalidParameterType ()
    {
      ParameterExpression identifier2 = Expression.Parameter (typeof (string), "fromIdentifier1");
      ClauseFieldResolverVisitor.ParseFieldAccess (StubDatabaseInfo.Instance, _studentClause, identifier2, true);
    }

    [Test]
    public void NestedMembers ()
    {
      Expression expressionTree = Expression.MakeMemberAccess (
          Expression.MakeMemberAccess (_studentDetailParameter, typeof (Student_Detail).GetProperty ("Student")),
          typeof (Student).GetProperty ("First"));
      FieldAccessInfo result =
          ClauseFieldResolverVisitor.ParseFieldAccess (StubDatabaseInfo.Instance, _studentDetailClause, expressionTree, true);

      Assert.AreEqual (typeof (Student).GetProperty ("First"), result.AccessedMember);
      Assert.That (result.JoinMembers, Is.EqualTo (new object[] { typeof (Student_Detail).GetProperty ("Student") }));
    }

    [Test]
    public void DeeplyNestedMembers ()
    {
      Expression expressionTree =
          Expression.MakeMemberAccess (
              Expression.MakeMemberAccess (
                  Expression.MakeMemberAccess (
                      _studentDetailDetailParameter,
                      typeof (Student_Detail_Detail).GetProperty ("Student_Detail")),
                  typeof (Student_Detail).GetProperty ("Student")),
              typeof (Student).GetProperty ("First"));

      FieldAccessInfo result = 
          ClauseFieldResolverVisitor.ParseFieldAccess (StubDatabaseInfo.Instance, _studentDetailDetailClause, expressionTree, true);
      Assert.AreEqual (typeof (Student).GetProperty ("First"), result.AccessedMember);
      Assert.That (
          result.JoinMembers,
          Is.EqualTo (
              new object[]
              {
                  typeof (Student_Detail_Detail).GetProperty ("Student_Detail"),
                  typeof (Student_Detail).GetProperty ("Student")
              }));
    }

    [Test]
    [ExpectedException (typeof (FieldAccessResolveException), ExpectedMessage = "Only MemberExpressions, QuerySourceReferenceExpressions, and "
        + "ParameterExpressions can be resolved, found 'null'.")]
    public void InvalidExpression ()
    {
      Expression expressionTree = Expression.Constant (null, typeof (Student));
      ClauseFieldResolverVisitor.ParseFieldAccess (StubDatabaseInfo.Instance, _studentClause, expressionTree, true);
    }

    [Test]
    public void VisitMemberExpression_OptimizesAccessToRelatedPrimaryKey ()
    {
      Expression expressionTree = ExpressionHelper.MakeExpression<Student_Detail, int> (sd => sd.Student.ID);
      FieldAccessInfo result = ClauseFieldResolverVisitor.ParseFieldAccess (StubDatabaseInfo.Instance, _studentDetailClause, expressionTree, true);
      Assert.That (result.AccessedMember, Is.EqualTo (ExpressionHelper.GetMember<Student_Detail> (sd => sd.Student)));
      Assert.IsEmpty (result.JoinMembers);

      Expression optimizedExpressionTree = ExpressionHelper.MakeExpression<Student_Detail, Student> (sd => sd.Student);
      CheckOptimization (result, optimizedExpressionTree);
    }

    [Test]
    public void VisitMemberExpression_AccessToRelatedPrimaryKey_OptimizeFalse ()
    {
      Expression expressionTree = ExpressionHelper.MakeExpression<Student_Detail, int> (sd => sd.Student.ID);
      FieldAccessInfo result = ClauseFieldResolverVisitor.ParseFieldAccess (StubDatabaseInfo.Instance, _studentDetailClause, expressionTree, false);
      Assert.That (result.AccessedMember, Is.EqualTo (ExpressionHelper.GetMember<Student> (s => s.ID)));
      Assert.That (result.JoinMembers, Is.EqualTo (new[] { ExpressionHelper.GetMember<Student_Detail> (sd => sd.Student) }));
    }

    [Test]
    public void VisitMemberExpression_OptimzationWithRelatedPrimaryKeyOverSeveralSteps ()
    {
      Expression expressionTree = ExpressionHelper.MakeExpression<Student_Detail, int> (sd => sd.Student.OtherStudent.ID);
      FieldAccessInfo result = ClauseFieldResolverVisitor.ParseFieldAccess (StubDatabaseInfo.Instance, _studentDetailClause, expressionTree, true);
      Assert.That (result.AccessedMember, Is.EqualTo (ExpressionHelper.GetMember<Student> (s => s.OtherStudent)));
      Assert.That (result.JoinMembers, Is.EqualTo (new[] { ExpressionHelper.GetMember<Student_Detail> (sd => sd.Student) }));

      Expression optimizedExpressionTree = ExpressionHelper.MakeExpression<Student_Detail, Student> (sd => sd.Student.OtherStudent);
      CheckOptimization (result, optimizedExpressionTree);
    }

    private void CheckOptimization (FieldAccessInfo actualResult, Expression expectedEquivalentOptimization)
    {
      FieldAccessInfo optimizedResult = ClauseFieldResolverVisitor.ParseFieldAccess (StubDatabaseInfo.Instance, _studentDetailClause, 
          expectedEquivalentOptimization, false);
      Assert.That (actualResult.AccessedMember, Is.EqualTo (optimizedResult.AccessedMember));
      Assert.That (actualResult.JoinMembers, Is.EqualTo (optimizedResult.JoinMembers));
    }

  }
}
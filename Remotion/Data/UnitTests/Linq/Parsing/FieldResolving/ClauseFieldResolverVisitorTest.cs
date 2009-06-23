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

namespace Remotion.Data.UnitTests.Linq.Parsing.FieldResolving
{
  [TestFixture]
  public class ClauseFieldResolverVisitorTest
  {
    private MainFromClause _studentClause;
    private QuerySourceReferenceExpression _studentReference;

    private MainFromClause _studentDetailClause;
    private QuerySourceReferenceExpression _studentDetailReference;

    [SetUp]
    public void SetUp ()
    {
      _studentClause = ExpressionHelper.CreateMainFromClause (Expression.Parameter (typeof (Student), "s"), ExpressionHelper.CreateQuerySource());
      _studentReference = new QuerySourceReferenceExpression (_studentClause);

      _studentDetailClause = ExpressionHelper.CreateMainFromClause (Expression.Parameter (typeof (Student_Detail), "sd"), ExpressionHelper.CreateQuerySource_Detail ());
      _studentDetailReference = new QuerySourceReferenceExpression (_studentDetailClause);
    }

    [Test]
    public void QuerySourceReferenceExpression ()
    {
      var result = ClauseFieldResolverVisitor.ParseFieldAccess(StubDatabaseInfo.Instance, _studentReference, true);
      Assert.That (result.AccessedMember, Is.Null);
      Assert.That (result.JoinMembers, Is.Empty);
      Assert.That (result.QuerySourceReferenceExpression, Is.SameAs (_studentReference));
    }

    [Test]
    public void NestedQuerySourceReferenceExpression ()
    {
      Expression expressionTree = Expression.MakeMemberAccess (_studentReference, typeof (Student).GetProperty ("First"));
      FieldAccessInfo result = ClauseFieldResolverVisitor.ParseFieldAccess (StubDatabaseInfo.Instance, expressionTree, true);
      Assert.That (result.AccessedMember, Is.EqualTo (typeof (Student).GetProperty ("First")));
      Assert.That (result.JoinMembers, Is.Empty);
      Assert.That (result.QuerySourceReferenceExpression, Is.SameAs (_studentReference));
    }

    [Test]
    public void NestedMembers ()
    {
      Expression expressionTree = Expression.MakeMemberAccess (
          Expression.MakeMemberAccess (_studentDetailReference, typeof (Student_Detail).GetProperty ("Student")),
          typeof (Student).GetProperty ("First"));
      FieldAccessInfo result = ClauseFieldResolverVisitor.ParseFieldAccess (StubDatabaseInfo.Instance, expressionTree, true);

      Assert.That (result.AccessedMember, Is.EqualTo (typeof (Student).GetProperty ("First")));
      Assert.That (result.JoinMembers, Is.EqualTo (new object[] { typeof (Student_Detail).GetProperty ("Student") }));
      Assert.That (result.QuerySourceReferenceExpression, Is.SameAs (_studentDetailReference));
    }

    [Test]
    [ExpectedException (typeof (FieldAccessResolveException), ExpectedMessage = "Only MemberExpressions and QuerySourceReferenceExpressions "
        + "can be resolved, found 'null'.")]
    public void InvalidExpression ()
    {
      Expression expressionTree = Expression.Constant (null, typeof (Student));
      ClauseFieldResolverVisitor.ParseFieldAccess (StubDatabaseInfo.Instance, expressionTree, true);
    }

    [Test]
    public void VisitMemberExpression_OptimizesAccessToRelatedPrimaryKey ()
    {
      Expression expressionTree = ExpressionHelper.Resolve<Student_Detail, int> (_studentDetailClause, sd => sd.Student.ID);
      FieldAccessInfo result = ClauseFieldResolverVisitor.ParseFieldAccess (StubDatabaseInfo.Instance, expressionTree, true);
      Assert.That (result.AccessedMember, Is.EqualTo (ExpressionHelper.GetMember<Student_Detail> (sd => sd.Student)));
      Assert.IsEmpty (result.JoinMembers);

      Expression optimizedExpressionTree = ExpressionHelper.Resolve<Student_Detail, Student> (_studentDetailClause, sd => sd.Student);
      CheckOptimization (result, optimizedExpressionTree);
    }

    [Test]
    public void VisitMemberExpression_AccessToRelatedPrimaryKey_OptimizeFalse ()
    {
      Expression expressionTree = ExpressionHelper.Resolve<Student_Detail, int> (_studentDetailClause, sd => sd.Student.ID);
      FieldAccessInfo result = ClauseFieldResolverVisitor.ParseFieldAccess (StubDatabaseInfo.Instance, expressionTree, false);
      Assert.That (result.AccessedMember, Is.EqualTo (ExpressionHelper.GetMember<Student> (s => s.ID)));
      Assert.That (result.JoinMembers, Is.EqualTo (new[] { ExpressionHelper.GetMember<Student_Detail> (sd => sd.Student) }));
    }

    [Test]
    public void VisitMemberExpression_OptimzationWithRelatedPrimaryKeyOverSeveralSteps ()
    {
      Expression expressionTree = ExpressionHelper.Resolve<Student_Detail, int> (_studentDetailClause, sd => sd.Student.OtherStudent.ID);
      FieldAccessInfo result = ClauseFieldResolverVisitor.ParseFieldAccess (StubDatabaseInfo.Instance, expressionTree, true);
      Assert.That (result.AccessedMember, Is.EqualTo (ExpressionHelper.GetMember<Student> (s => s.OtherStudent)));
      Assert.That (result.JoinMembers, Is.EqualTo (new[] { ExpressionHelper.GetMember<Student_Detail> (sd => sd.Student) }));

      Expression optimizedExpressionTree = ExpressionHelper.Resolve<Student_Detail, Student> (_studentDetailClause, sd => sd.Student.OtherStudent);
      CheckOptimization (result, optimizedExpressionTree);
    }

    private void CheckOptimization (FieldAccessInfo actualResult, Expression expectedEquivalentOptimization)
    {
      FieldAccessInfo optimizedResult = ClauseFieldResolverVisitor.ParseFieldAccess (StubDatabaseInfo.Instance, 
          expectedEquivalentOptimization, false);
      Assert.That (actualResult.AccessedMember, Is.EqualTo (optimizedResult.AccessedMember));
      Assert.That (actualResult.JoinMembers, Is.EqualTo (optimizedResult.JoinMembers));
    }
  }
}
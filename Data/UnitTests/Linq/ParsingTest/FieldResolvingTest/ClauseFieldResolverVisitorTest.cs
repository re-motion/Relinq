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
using NUnit.Framework;
using System.Linq.Expressions;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Parsing.FieldResolving;

namespace Remotion.Data.UnitTests.Linq.ParsingTest.FieldResolvingTest
{
  [TestFixture]
  public class ClauseFieldResolverVisitorTest
  {
    [Test]
    public void NestedParameters()
    {
      ParameterExpression parameter = Expression.Parameter (typeof (Student), "s");
      Expression expressionTree = Expression.MakeMemberAccess (parameter, typeof (Student).GetProperty ("First"));
      ClauseFieldResolverVisitor.Result result = new ClauseFieldResolverVisitor (StubDatabaseInfo.Instance).ParseFieldAccess (expressionTree, expressionTree, true);
      Assert.AreSame (parameter, result.Parameter);
      Assert.AreEqual (typeof (Student).GetProperty ("First"), result.AccessedMember);
      Assert.That (result.JoinMembers, Is.Empty);
    }

    [Test]
    public void NestedMembers ()
    {
      ParameterExpression parameter = Expression.Parameter (typeof (Student_Detail), "sd");
      Expression expressionTree = Expression.MakeMemberAccess (
          Expression.MakeMemberAccess (parameter, typeof (Student_Detail).GetProperty ("Student")),
          typeof (Student).GetProperty ("First"));
      ClauseFieldResolverVisitor.Result result = new ClauseFieldResolverVisitor (StubDatabaseInfo.Instance).ParseFieldAccess (expressionTree, expressionTree, true);

      Assert.AreEqual (typeof (Student).GetProperty ("First"), result.AccessedMember);
      Assert.That (result.JoinMembers, Is.EqualTo (new object[] { typeof (Student_Detail).GetProperty ("Student") }));
    }

    [Test]
    public void DeeplyNestedMembers ()
    {
      Expression expressionTree =
          Expression.MakeMemberAccess (
              Expression.MakeMemberAccess (
                  Expression.MakeMemberAccess (Expression.Parameter (typeof (Student_Detail_Detail), "sdd"),
                      typeof (Student_Detail_Detail).GetProperty ("Student_Detail")),
                  typeof (Student_Detail).GetProperty ("Student")),
              typeof (Student).GetProperty ("First"));

      ClauseFieldResolverVisitor.Result result = new ClauseFieldResolverVisitor (StubDatabaseInfo.Instance).ParseFieldAccess (expressionTree, expressionTree, true);
      Assert.AreEqual (typeof (Student).GetProperty ("First"), result.AccessedMember);
      Assert.That (result.JoinMembers, Is.EqualTo (new object[]
          {
              typeof (Student_Detail_Detail).GetProperty ("Student_Detail"),
              typeof (Student_Detail).GetProperty ("Student")
          }));
    }

    [Test]
    public void NoMember ()
    {
      ParameterExpression parameter = Expression.Parameter (typeof (Student_Detail), "sd");
      Expression expressionTree = parameter;
      ClauseFieldResolverVisitor.Result result = new ClauseFieldResolverVisitor (StubDatabaseInfo.Instance).ParseFieldAccess (expressionTree, expressionTree, true);
      Assert.IsNull (result.AccessedMember);
      Assert.IsEmpty (result.JoinMembers);
      Assert.AreSame (parameter, result.Parameter);
    }

    [Test]
    [ExpectedException (typeof (FieldAccessResolveException), ExpectedMessage = "Only MemberExpressions and ParameterExpressions can be resolved, "
        + "found 'null' in expression '1'.")]
    public void InvalidExpression ()
    {
      Expression expressionTree = Expression.Constant (null, typeof (Student));
      Expression expressionTreeRoot = Expression.Constant (1);

      new ClauseFieldResolverVisitor (StubDatabaseInfo.Instance).ParseFieldAccess (expressionTree, expressionTreeRoot, true);
    }

    [Test]
    public void VisitMemberExpression_OptimizesAccessToRelatedPrimaryKey ()
    {
      Expression expressionTree = ExpressionHelper.MakeExpression<Student_Detail, int> (sd => sd.Student.ID);
      ClauseFieldResolverVisitor.Result result = new ClauseFieldResolverVisitor (StubDatabaseInfo.Instance).ParseFieldAccess (expressionTree, expressionTree, true);
      Assert.That (result.AccessedMember, Is.EqualTo (ExpressionHelper.GetMember<Student_Detail> (sd => sd.Student)));
      Assert.IsEmpty (result.JoinMembers);

      Expression optimizedExpressionTree = ExpressionHelper.MakeExpression<Student_Detail, Student> (sd => sd.Student);
      CheckOptimization (result, optimizedExpressionTree);
    }

    [Test]
    public void VisitMemberExpression_AccessToRelatedPrimaryKey_OptimizeFalse ()
    {
      Expression expressionTree = ExpressionHelper.MakeExpression<Student_Detail, int> (sd => sd.Student.ID);
      ClauseFieldResolverVisitor.Result result = new ClauseFieldResolverVisitor (StubDatabaseInfo.Instance).ParseFieldAccess (expressionTree, expressionTree, false);
      Assert.That (result.AccessedMember, Is.EqualTo (ExpressionHelper.GetMember<Student> (s => s.ID)));
      Assert.That (result.JoinMembers, Is.EqualTo (new [] { ExpressionHelper.GetMember<Student_Detail> (sd => sd.Student) }));
    }

    [Test]
    public void VisitMemberExpression_OptimzationWithRelatedPrimaryKeyOverSeveralSteps ()
    {
      Expression expressionTree = ExpressionHelper.MakeExpression<Student_Detail, int> (sd => sd.Student.OtherStudent.ID);
      ClauseFieldResolverVisitor.Result result = new ClauseFieldResolverVisitor (StubDatabaseInfo.Instance).ParseFieldAccess (expressionTree, expressionTree, true);
      Assert.That (result.AccessedMember, Is.EqualTo (ExpressionHelper.GetMember<Student> (s => s.OtherStudent)));
      Assert.That (result.JoinMembers, Is.EqualTo (new[]{ ExpressionHelper.GetMember<Student_Detail> (sd => sd.Student) }));

      Expression optimizedExpressionTree = ExpressionHelper.MakeExpression<Student_Detail, Student> (sd => sd.Student.OtherStudent);
      CheckOptimization(result, optimizedExpressionTree);
    }

    private void CheckOptimization (ClauseFieldResolverVisitor.Result actualResult, Expression expectedEquivalentOptimization)
    {
      ClauseFieldResolverVisitor.Result optimizedResult = new ClauseFieldResolverVisitor (StubDatabaseInfo.Instance).ParseFieldAccess (expectedEquivalentOptimization, expectedEquivalentOptimization, false);
      Assert.That (actualResult.AccessedMember, Is.EqualTo (optimizedResult.AccessedMember));
      Assert.That (actualResult.JoinMembers, Is.EqualTo (optimizedResult.JoinMembers));
      ExpressionTreeComparer.CheckAreEqualTrees (actualResult.Parameter, optimizedResult.Parameter);
    }
  }
}

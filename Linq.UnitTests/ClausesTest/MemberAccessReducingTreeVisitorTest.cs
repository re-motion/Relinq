using System;
using System.Linq.Expressions;
using NUnit.Framework;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Data.Linq.UnitTests.ParsingTest;
using Rubicon.Collections;

namespace Rubicon.Data.Linq.UnitTests.ClausesTest
{
  [TestFixture]
  public class MemberAccessReducingTreeVisitorTest
  {
    [Test]
    public void ReduceInnermostMemberExpression_NonNested()
    {
      MemberExpression expression = Expression.MakeMemberAccess (Expression.Constant (null, typeof (Student)), typeof (Student).GetProperty ("First"));
      Expression reducedExpression = new MemberAccessReducingTreeVisitor().ReduceInnermostMemberExpression (expression);
      Expression expected = Expression.Parameter (typeof (string), "First");
      ExpressionTreeComparer.CheckAreEqualTrees (expected, reducedExpression);
    }

    [Test]
    public void ReduceInnermostMemberExpression_NestedOneLevel ()
    {
      MemberExpression studentDetailExpression = Expression.MakeMemberAccess (Expression.Constant (null, typeof (Student_Detail)),
          typeof (Student_Detail).GetProperty ("Student"));
      MemberExpression expression = Expression.MakeMemberAccess (studentDetailExpression, typeof (Student).GetProperty ("First"));
      Expression reducedExpression = new MemberAccessReducingTreeVisitor ().ReduceInnermostMemberExpression (expression);
      Expression expected = Expression.MakeMemberAccess (Expression.Parameter (typeof (Student), "Student"), typeof (Student).GetProperty ("First"));
      ExpressionTreeComparer.CheckAreEqualTrees (expected, reducedExpression);
    }

    [Test]
    public void ReduceInnermostMemberExpression_NestedTwoLevels ()
    {
      MemberExpression tupleExpression = Expression.MakeMemberAccess (Expression.Constant (null, typeof (Tuple<Student_Detail, object>)),
          typeof (Tuple<Student_Detail, object>).GetProperty ("A"));
      MemberExpression studentDetailExpression = Expression.MakeMemberAccess (tupleExpression,
          typeof (Student_Detail).GetProperty ("Student"));
      MemberExpression expression = Expression.MakeMemberAccess (studentDetailExpression, typeof (Student).GetProperty ("First"));

      Expression reducedExpression = new MemberAccessReducingTreeVisitor ().ReduceInnermostMemberExpression (expression);
      Expression expected =
          Expression.MakeMemberAccess (
              Expression.MakeMemberAccess (Expression.Parameter (typeof (Student_Detail), "A"), typeof (Student_Detail).GetProperty ("Student")),
              typeof (Student).GetProperty ("First"));
      ExpressionTreeComparer.CheckAreEqualTrees (expected, reducedExpression);
    }
  }
}
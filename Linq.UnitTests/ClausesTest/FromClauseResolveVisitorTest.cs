using System;
using NUnit.Framework;
using System.Linq.Expressions;
using Rubicon.Data.Linq.Clauses;

namespace Rubicon.Data.Linq.UnitTests.ClausesTest
{
  [TestFixture]
  public class FromClauseResolveVisitorTest
  {
    [Test]
    public void NestedParameters()
    {
      ParameterExpression parameter = Expression.Parameter (typeof (Student), "s");
      Expression expressionTree = Expression.MakeMemberAccess (parameter, typeof (Student).GetProperty ("First"));
      FromClauseResolveVisitor.Result result = new FromClauseResolveVisitor ().ParseFieldAccess (expressionTree, expressionTree);
      Assert.AreSame (parameter, result.Parameter);
    }

    [Test]
    public void LastMember ()
    {
      ParameterExpression parameter = Expression.Parameter (typeof (Student_Detail), "sd");
      Expression expressionTree =
          Expression.MakeMemberAccess (
              Expression.MakeMemberAccess (parameter, typeof (Student_Detail).GetProperty ("Student")),
              typeof (Student).GetProperty ("First"));
      FromClauseResolveVisitor.Result result = new FromClauseResolveVisitor ().ParseFieldAccess (expressionTree, expressionTree);
      Assert.AreSame (parameter, result.Parameter);
      Assert.AreEqual (typeof (Student_Detail).GetProperty ("Student"), result.Member);
    }

    [Test]
    public void NoMember ()
    {
      ParameterExpression parameter = Expression.Parameter (typeof (Student_Detail), "sd");
      Expression expressionTree = parameter;
      FromClauseResolveVisitor.Result result = new FromClauseResolveVisitor ().ParseFieldAccess (expressionTree, expressionTree);
      Assert.IsNull (result.Member);
      Assert.AreSame (parameter, result.Parameter);
    }

    [Test]
    [ExpectedException (typeof (FieldAccessResolveException), ExpectedMessage = "Only MemberExpressions and ParameterExpressions can be resolved, "
        + "found 'null' in expression '1'.")]
    public void InvalidExpression ()
    {
      Expression expressionTree = Expression.Constant (null, typeof (Student));
      Expression expressionTreeRoot = Expression.Constant (1);

      new FromClauseResolveVisitor ().ParseFieldAccess (expressionTree, expressionTreeRoot);
    }
  }
}
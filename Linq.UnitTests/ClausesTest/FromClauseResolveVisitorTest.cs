using System;
using NUnit.Framework;
using System.Linq.Expressions;
using NUnit.Framework.SyntaxHelpers;
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
      Assert.That (result.Members, Is.EqualTo (new object[] { typeof (Student).GetProperty ("First")}));

    }

    [Test]
    public void NestedMembers ()
    {
      ParameterExpression parameter = Expression.Parameter (typeof (Student_Detail), "sd");
      Expression expressionTree = Expression.MakeMemberAccess (
        Expression.MakeMemberAccess (parameter, typeof (Student_Detail).GetProperty ("Student")),
        typeof (Student).GetProperty ("First"));
      FromClauseResolveVisitor.Result result = new FromClauseResolveVisitor ().ParseFieldAccess (expressionTree, expressionTree);
      Assert.That (result.Members, Is.EqualTo (new object[] { typeof (Student).GetProperty ("First"), typeof (Student_Detail).GetProperty ("Student") }));
    }

    [Test]
    public void NoMember ()
    {
      ParameterExpression parameter = Expression.Parameter (typeof (Student_Detail), "sd");
      Expression expressionTree = parameter;
      FromClauseResolveVisitor.Result result = new FromClauseResolveVisitor ().ParseFieldAccess (expressionTree, expressionTree);
      Assert.IsEmpty (result.Members);
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
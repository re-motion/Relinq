using System;
using System.Linq.Expressions;
using NUnit.Framework;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Data.Linq.UnitTests.ParsingTest;

namespace Rubicon.Data.Linq.UnitTests.ClausesTest
{
  [TestFixture]
  public class AdditionalFromClauseResolveVisitorTest
  {
    public class AnonymousType
    {
      public Student s;
    }

    private ParameterExpression _s;
    private ParameterExpression _s1;
    private ParameterExpression _s2;

    private ParameterExpression _transparent;
    private ParameterExpression _transparent1;
    private ParameterExpression _transparent2;

    [SetUp]
    public void SetUp()
    {
      _s = Expression.Parameter (typeof (Student), "s");
      _s1 = Expression.Parameter (typeof (Student), "s1");
      _s2 = Expression.Parameter (typeof (Student), "s2");

      _transparent = Expression.Parameter (typeof (AnonymousType), "transparent");
      _transparent1 = Expression.Parameter (typeof (AnonymousType), "transparent1");
      _transparent2 = Expression.Parameter (typeof (AnonymousType), "transparent2");
    }

    [Test]
    public void FromIdentifier_Found_NoTransparentIdentifier()
    {
      MemberExpression expression = Expression.MakeMemberAccess (_s, typeof (Student).GetProperty ("First"));
      AdditionalFromClauseResolveVisitor visitor = new AdditionalFromClauseResolveVisitor (_s, new ParameterExpression[] { _transparent });
      AdditionalFromClauseResolveVisitor.Result result = visitor.ParseAndReduce (expression, expression);
      Assert.AreSame (expression, result.ReducedExpression);
      Assert.IsTrue (result.FromIdentifierFound);
      Assert.AreEqual (typeof (Student).GetProperty ("First"), result.Member);
    }

    [Test]
    public void FromIdentifier_Found_NoTransparentIdentifier_NoMemberAccess ()
    {
      AdditionalFromClauseResolveVisitor visitor = new AdditionalFromClauseResolveVisitor (_s, new ParameterExpression[] { _transparent });
      
      Expression expression = _s;
      AdditionalFromClauseResolveVisitor.Result result = visitor.ParseAndReduce (expression, expression);
      Assert.AreSame (expression, result.ReducedExpression);
      Assert.IsTrue (result.FromIdentifierFound);
      Assert.IsNull (result.Member);
    }

    [Test]
    public void FromIdentifier_Found_NoTransparentIdentifier_NoMemberAccess_TransparentSameNameAsFromIdentifier ()
    {
      AdditionalFromClauseResolveVisitor visitor = new AdditionalFromClauseResolveVisitor (_s, new ParameterExpression[] { _transparent, _s });

      Expression expression = _s;
      AdditionalFromClauseResolveVisitor.Result result = visitor.ParseAndReduce (expression, expression);
      Assert.AreSame (expression, result.ReducedExpression);
      Assert.IsTrue (result.FromIdentifierFound);
      Assert.IsNull (result.Member);
    }

    [Test]
    public void FromIdentifier_NotFound_NoTransparentIdentifier ()
    {
      AdditionalFromClauseResolveVisitor visitor = new AdditionalFromClauseResolveVisitor (_s1, new ParameterExpression[] { _transparent });

      MemberExpression expression = Expression.MakeMemberAccess (_s2, typeof (Student).GetProperty ("First"));
      AdditionalFromClauseResolveVisitor.Result result = visitor.ParseAndReduce (expression, expression);
      Assert.AreSame (expression, result.ReducedExpression);
      Assert.IsFalse (result.FromIdentifierFound);
    }

    [Test]
    public void FromIdentifier_NotFound_NoTransparentIdentifier_NoMemberAccess ()
    {
      AdditionalFromClauseResolveVisitor visitor = new AdditionalFromClauseResolveVisitor (_s1, new ParameterExpression[] { _transparent });

      ParameterExpression expression = _s2;
      AdditionalFromClauseResolveVisitor.Result result = visitor.ParseAndReduce (expression, expression);
      Assert.AreSame (expression, result.ReducedExpression);
      Assert.IsFalse (result.FromIdentifierFound);
    }

    [Test]
    public void FromIdentifier_NotFound_TransparentIdentifier_OneLevelDeep ()
    {
      AdditionalFromClauseResolveVisitor visitor = new AdditionalFromClauseResolveVisitor (_s, new ParameterExpression[] { _transparent });

      MemberExpression expression = Expression.MakeMemberAccess (_transparent, typeof (AnonymousType).GetField ("s"));

      AdditionalFromClauseResolveVisitor.Result result = visitor.ParseAndReduce (expression, expression);
      Assert.AreNotSame (expression, result.ReducedExpression);

      Expression expectedReducedExpression = Expression.Parameter (typeof (Student), "s");
      ExpressionTreeComparer.CheckAreEqualTrees (expectedReducedExpression, result.ReducedExpression);

      Assert.IsFalse (result.FromIdentifierFound);
    }

    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "Expected ParameterExpression or MemberExpression for resolving field access in "
        + "additional from clause, found ConstantExpression (null).")]
    public void InvalidExpressionTree()
    {
      AdditionalFromClauseResolveVisitor visitor = new AdditionalFromClauseResolveVisitor (_s, new ParameterExpression[0]);

      ConstantExpression expression = Expression.Constant (null, typeof (string));
      visitor.ParseAndReduce (expression, expression);
    }

    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "Expected ParameterExpression or MemberExpression for resolving field access in "
        + "additional from clause, found ConstantExpression (null).")]
    public void InvalidExpressionTree_WithMemberAccess ()
    {
      AdditionalFromClauseResolveVisitor visitor = new AdditionalFromClauseResolveVisitor (_s, new ParameterExpression[0]);

      Expression expression = Expression.MakeMemberAccess (Expression.Constant (null, typeof (Student)),
        typeof (Student).GetProperty ("First"));
      visitor.ParseAndReduce (expression, expression);
    }

    [Test]
    [ExpectedException (ExpectedMessage = "The identifier 's' has a different type (System.String) than expected (Rubicon.Data.Linq.UnitTests.Student)"
        + " in expression 's'.")]
    public void InvalidFromIdentifierType()
    {
      AdditionalFromClauseResolveVisitor visitor = new AdditionalFromClauseResolveVisitor (_s, new ParameterExpression[0]);

      ParameterExpression invalidParameter = Expression.Parameter (typeof (string), "s");
      visitor.ParseAndReduce (invalidParameter, invalidParameter);
    }

    [Test]
    [ExpectedException (ExpectedMessage = "The identifier 'transparent' has a different type (System.String) than expected "
        + "(Rubicon.Data.Linq.UnitTests.ClausesTest.AdditionalFromClauseResolveVisitorTest+AnonymousType) in expression "
        + "'transparent'.")]
    public void InvalidTransparentIdentifierType ()
    {
      AdditionalFromClauseResolveVisitor visitor = new AdditionalFromClauseResolveVisitor (_s, new ParameterExpression[] {_transparent});

      ParameterExpression invalidTransparent = Expression.Parameter (typeof (string), "transparent");
      visitor.ParseAndReduce (invalidTransparent, invalidTransparent);
    }
  }
}
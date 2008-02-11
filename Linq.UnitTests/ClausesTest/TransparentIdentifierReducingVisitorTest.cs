using System;
using System.Linq.Expressions;
using NUnit.Framework;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Data.Linq.UnitTests.ParsingTest;

namespace Rubicon.Data.Linq.UnitTests.ClausesTest
{
  [TestFixture]
  public class TransparentIdentifierReducingVisitorTest
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
    public void TransparentIdentifier_NoneFound ()
    {
      TransparentIdentifierReducingVisitor visitor = new TransparentIdentifierReducingVisitor (new ParameterExpression[] { _transparent });

      MemberExpression expression = Expression.MakeMemberAccess (_s2, typeof (Student).GetProperty ("First"));
      Expression reducedExpression = visitor.ParseAndReduce (expression, expression);
      Assert.AreSame (expression, reducedExpression);
    }

    [Test]
    public void TransparentIdentifier_Found_TwoLevelsDeep ()
    {
      TransparentIdentifierReducingVisitor visitor = new TransparentIdentifierReducingVisitor (new ParameterExpression[] { _transparent });

      MemberExpression studentExpression = Expression.MakeMemberAccess (_transparent, typeof (AnonymousType).GetField ("s"));
      MemberExpression expression = Expression.MakeMemberAccess (studentExpression, typeof (Student).GetProperty ("First"));

      Expression reducedExpression = visitor.ParseAndReduce (expression, expression);
      Assert.AreNotSame (expression, reducedExpression);

      ParameterExpression expectedStudentParameter = Expression.Parameter (typeof (Student), "s");
      MemberExpression expectedReducedExpression = Expression.MakeMemberAccess (expectedStudentParameter, typeof (Student).GetProperty ("First"));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedReducedExpression, reducedExpression);
    }

    [Test]
    public void TransparentIdentifier_Found_OneLevelDeep ()
    {
      TransparentIdentifierReducingVisitor visitor = new TransparentIdentifierReducingVisitor (new ParameterExpression[] { _transparent });

      MemberExpression expression = Expression.MakeMemberAccess (_transparent, typeof (AnonymousType).GetField ("s"));

      Expression reducedExpression = visitor.ParseAndReduce (expression, expression);
      Assert.AreNotSame (expression, reducedExpression);

      Expression expectedReducedExpression = Expression.Parameter (typeof (Student), "s");
      ExpressionTreeComparer.CheckAreEqualTrees (expectedReducedExpression, reducedExpression);
    }

    [Test]
    public void TransparentIdentifier_Found_OneLevelDeep_SecondTransparentIdentifier ()
    {
      TransparentIdentifierReducingVisitor visitor =
          new TransparentIdentifierReducingVisitor (new ParameterExpression[] { _transparent1, _transparent2 });

      MemberExpression expression = Expression.MakeMemberAccess (_transparent2, typeof (AnonymousType).GetField ("s"));

      Expression reducedExpression = visitor.ParseAndReduce (expression, expression);
      Assert.AreNotSame (expression, reducedExpression);

      Expression expectedReducedExpression = Expression.Parameter (typeof (Student), "s");
      ExpressionTreeComparer.CheckAreEqualTrees (expectedReducedExpression, reducedExpression);
    }

    [Test]
    [ExpectedException (ExpectedMessage = "The identifier 'transparent' has a different type (System.String) than expected "
        + "(Rubicon.Data.Linq.UnitTests.ClausesTest.TransparentIdentifierReducingVisitorTest+AnonymousType) in expression "
        + "'transparent'.")]
    public void InvalidTransparentIdentifierType ()
    {
      TransparentIdentifierReducingVisitor visitor = new TransparentIdentifierReducingVisitor (new ParameterExpression[] {_transparent});

      ParameterExpression invalidTransparent = Expression.Parameter (typeof (string), "transparent");
      visitor.ParseAndReduce (invalidTransparent, invalidTransparent);
    }
  }
}
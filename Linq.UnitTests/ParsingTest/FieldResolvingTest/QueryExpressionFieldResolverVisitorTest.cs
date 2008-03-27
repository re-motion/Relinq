using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Data.Linq.Parsing.FieldResolving;
using Rubicon.Data.Linq.UnitTests.ParsingTest;

namespace Rubicon.Data.Linq.UnitTests.ParsingTest.FieldResolvingTest
{
  [TestFixture]
  public class QueryExpressionFieldResolverVisitorTest
  {
    public class AnonymousType
    {
      public Student s1;
      public Student fzlbf;
      public AnonymousType transparent2;
    }

    [Test]
    public void FirstFromIdentifierSpecified()
    {
      QueryExpression queryExpression = CreateQueryExpression();

      Expression sourceExpression = Expression.Parameter (typeof (Student), "s1");
      QueryExpressionFieldResolverVisitor.Result result = new QueryExpressionFieldResolverVisitor (queryExpression).ParseAndReduce(sourceExpression);
      Assert.AreSame (sourceExpression, result.ReducedExpression);
      Assert.AreSame (queryExpression.MainFromClause, result.FromClause);
    }

    [Test]
    public void SecondFromIdentifierSpecified ()
    {
      QueryExpression queryExpression = CreateQueryExpression ();

      Expression sourceExpression = Expression.Parameter (typeof (Student), "s2");
      QueryExpressionFieldResolverVisitor.Result result = new QueryExpressionFieldResolverVisitor (queryExpression).ParseAndReduce (sourceExpression);
      Assert.AreSame (sourceExpression, result.ReducedExpression);
      Assert.AreSame (queryExpression.BodyClauses.First(), result.FromClause);
    }

    [Test]
    public void MemberAccess_WithFromIdentifier ()
    {
      QueryExpression queryExpression = CreateQueryExpression ();

      Expression sourceExpression = Expression.MakeMemberAccess (
          Expression.Parameter (typeof (Student), "s1"),
          typeof (Student).GetProperty ("First"));
      QueryExpressionFieldResolverVisitor.Result result = new QueryExpressionFieldResolverVisitor (queryExpression).ParseAndReduce (sourceExpression);
      Assert.AreSame (sourceExpression, result.ReducedExpression);
      Assert.AreSame (queryExpression.MainFromClause, result.FromClause);
    }

    [Test]
    public void TransparentIdentifier_ThenFromIdentifier ()
    {
      QueryExpression queryExpression = CreateQueryExpression ();

      Expression sourceExpression = Expression.MakeMemberAccess (
          Expression.Parameter (typeof (AnonymousType), "transparent1"),
          typeof (AnonymousType).GetField ("s1"));

      QueryExpressionFieldResolverVisitor.Result result = new QueryExpressionFieldResolverVisitor (queryExpression).ParseAndReduce (sourceExpression);
      ParameterExpression expectedReducedExpression = Expression.Parameter (typeof (Student), "s1");
      ExpressionTreeComparer.CheckAreEqualTrees (expectedReducedExpression, result.ReducedExpression);
      Assert.AreSame (queryExpression.MainFromClause, result.FromClause);
    }

    [Test]
    public void TransparentIdentifier_ThenFromIdentifier_ThenField ()
    {
      QueryExpression queryExpression = CreateQueryExpression ();

      Expression sourceExpression = Expression.MakeMemberAccess (
          Expression.MakeMemberAccess (
              Expression.Parameter (typeof (AnonymousType), "transparent1"),
              typeof (AnonymousType).GetField ("s1")),
          typeof (Student).GetProperty ("First"));

      QueryExpressionFieldResolverVisitor.Result result = new QueryExpressionFieldResolverVisitor (queryExpression).ParseAndReduce (sourceExpression);
      Expression expectedReducedExpression = Expression.MakeMemberAccess (
          Expression.Parameter (typeof (Student), "s1"),
          typeof (Student).GetProperty ("First"));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedReducedExpression, result.ReducedExpression);
      Assert.AreSame (queryExpression.MainFromClause, result.FromClause);
    }

    [Test]
    public void TransparentIdentifiers_ThenFromIdentifier ()
    {
      QueryExpression queryExpression = CreateQueryExpression ();

      Expression sourceExpression = Expression.MakeMemberAccess (
          Expression.MakeMemberAccess (
              Expression.Parameter (typeof (AnonymousType), "transparent1"),
              typeof (AnonymousType).GetField ("transparent2")),
          typeof (AnonymousType).GetField ("s1"));
      
      QueryExpressionFieldResolverVisitor.Result result = new QueryExpressionFieldResolverVisitor (queryExpression).ParseAndReduce (sourceExpression);
      ParameterExpression expectedReducedExpression = Expression.Parameter (typeof (Student), "s1");
      ExpressionTreeComparer.CheckAreEqualTrees (expectedReducedExpression, result.ReducedExpression);
      Assert.AreSame (queryExpression.MainFromClause, result.FromClause);
    }

    [Test]
    public void NoFromIdentifierFound ()
    {
      QueryExpression queryExpression = CreateQueryExpression ();

      Expression sourceExpression = Expression.MakeMemberAccess (
          Expression.MakeMemberAccess (
              Expression.Parameter (typeof (AnonymousType), "transparent1"),
              typeof (AnonymousType).GetField ("transparent2")),
          typeof (AnonymousType).GetField ("fzlbf"));

      Assert.IsNull (new QueryExpressionFieldResolverVisitor (queryExpression).ParseAndReduce (sourceExpression));
    }

    private QueryExpression CreateQueryExpression ()
    {
      ParameterExpression s1 = Expression.Parameter (typeof (Student), "s1");
      ParameterExpression s2 = Expression.Parameter (typeof (Student), "s2");
      MainFromClause mainFromClause = ExpressionHelper.CreateMainFromClause(s1, ExpressionHelper.CreateQuerySource ());
      AdditionalFromClause additionalFromClause =
          new AdditionalFromClause (mainFromClause, s2, ExpressionHelper.CreateLambdaExpression (), ExpressionHelper.CreateLambdaExpression ());

      QueryExpression expression = ExpressionHelper.CreateQueryExpression (mainFromClause);
      expression.AddBodyClause (additionalFromClause);

      return expression;
    }
  }
}
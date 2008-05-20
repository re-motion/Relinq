using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Parsing.FieldResolving;
using Remotion.Data.Linq.UnitTests.ParsingTest;

namespace Remotion.Data.Linq.UnitTests.ParsingTest.FieldResolvingTest
{
  [TestFixture]
  public class QueryModelFieldResolverVisitorTest
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
      QueryModel queryModel = CreateQueryExpression();

      Expression sourceExpression = Expression.Parameter (typeof (Student), "s1");
      QueryModelFieldResolverVisitor.Result result = new QueryModelFieldResolverVisitor (queryModel).ParseAndReduce(sourceExpression);
      Assert.AreSame (sourceExpression, result.ReducedExpression);
      Assert.AreSame (queryModel.MainFromClause, result.ResolveableClause);
    }

    [Test]
    public void SecondFromIdentifierSpecified ()
    {
      QueryModel queryModel = CreateQueryExpression ();

      Expression sourceExpression = Expression.Parameter (typeof (Student), "s2");
      QueryModelFieldResolverVisitor.Result result = new QueryModelFieldResolverVisitor (queryModel).ParseAndReduce (sourceExpression);
      Assert.AreSame (sourceExpression, result.ReducedExpression);
      Assert.AreSame (queryModel.BodyClauses.First(), result.ResolveableClause);
    }

    [Test]
    public void MemberAccess_WithFromIdentifier ()
    {
      QueryModel queryModel = CreateQueryExpression ();

      Expression sourceExpression = Expression.MakeMemberAccess (
          Expression.Parameter (typeof (Student), "s1"),
          typeof (Student).GetProperty ("First"));
      QueryModelFieldResolverVisitor.Result result = new QueryModelFieldResolverVisitor (queryModel).ParseAndReduce (sourceExpression);
      Assert.AreSame (sourceExpression, result.ReducedExpression);
      Assert.AreSame (queryModel.MainFromClause, result.ResolveableClause);
    }

    [Test]
    public void TransparentIdentifier_ThenFromIdentifier ()
    {
      QueryModel queryModel = CreateQueryExpression ();

      Expression sourceExpression = Expression.MakeMemberAccess (
          Expression.Parameter (typeof (AnonymousType), "transparent1"),
          typeof (AnonymousType).GetField ("s1"));

      QueryModelFieldResolverVisitor.Result result = new QueryModelFieldResolverVisitor (queryModel).ParseAndReduce (sourceExpression);
      ParameterExpression expectedReducedExpression = Expression.Parameter (typeof (Student), "s1");
      ExpressionTreeComparer.CheckAreEqualTrees (expectedReducedExpression, result.ReducedExpression);
      Assert.AreSame (queryModel.MainFromClause, result.ResolveableClause);
    }

    [Test]
    public void TransparentIdentifier_ThenFromIdentifier_ThenField ()
    {
      QueryModel queryModel = CreateQueryExpression ();

      Expression sourceExpression = Expression.MakeMemberAccess (
          Expression.MakeMemberAccess (
              Expression.Parameter (typeof (AnonymousType), "transparent1"),
              typeof (AnonymousType).GetField ("s1")),
          typeof (Student).GetProperty ("First"));

      QueryModelFieldResolverVisitor.Result result = new QueryModelFieldResolverVisitor (queryModel).ParseAndReduce (sourceExpression);
      Expression expectedReducedExpression = Expression.MakeMemberAccess (
          Expression.Parameter (typeof (Student), "s1"),
          typeof (Student).GetProperty ("First"));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedReducedExpression, result.ReducedExpression);
      Assert.AreSame (queryModel.MainFromClause, result.ResolveableClause);
    }

    [Test]
    public void TransparentIdentifiers_ThenFromIdentifier ()
    {
      QueryModel queryModel = CreateQueryExpression ();

      Expression sourceExpression = Expression.MakeMemberAccess (
          Expression.MakeMemberAccess (
              Expression.Parameter (typeof (AnonymousType), "transparent1"),
              typeof (AnonymousType).GetField ("transparent2")),
          typeof (AnonymousType).GetField ("s1"));
      
      QueryModelFieldResolverVisitor.Result result = new QueryModelFieldResolverVisitor (queryModel).ParseAndReduce (sourceExpression);
      ParameterExpression expectedReducedExpression = Expression.Parameter (typeof (Student), "s1");
      ExpressionTreeComparer.CheckAreEqualTrees (expectedReducedExpression, result.ReducedExpression);
      Assert.AreSame (queryModel.MainFromClause, result.ResolveableClause);
    }

    [Test]
    public void NoFromIdentifierFound ()
    {
      QueryModel queryModel = CreateQueryExpression ();

      Expression sourceExpression = Expression.MakeMemberAccess (
          Expression.MakeMemberAccess (
              Expression.Parameter (typeof (AnonymousType), "transparent1"),
              typeof (AnonymousType).GetField ("transparent2")),
          typeof (AnonymousType).GetField ("fzlbf"));

      Assert.IsNull (new QueryModelFieldResolverVisitor (queryModel).ParseAndReduce (sourceExpression));
    }

    private QueryModel CreateQueryExpression ()
    {
      ParameterExpression s1 = Expression.Parameter (typeof (Student), "s1");
      ParameterExpression s2 = Expression.Parameter (typeof (Student), "s2");
      MainFromClause mainFromClause = ExpressionHelper.CreateMainFromClause(s1, ExpressionHelper.CreateQuerySource ());
      AdditionalFromClause additionalFromClause =
          new AdditionalFromClause (mainFromClause, s2, ExpressionHelper.CreateLambdaExpression (), ExpressionHelper.CreateLambdaExpression ());

      QueryModel model = ExpressionHelper.CreateQueryModel (mainFromClause);
      model.AddBodyClause (additionalFromClause);

      return model;
    }
  }
}
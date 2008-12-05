// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2008 rubicon informationstechnologie gmbh, www.rubicon.eu
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
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Data.Linq;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Parsing.FieldResolving;
using Remotion.Data.UnitTests.Linq.ParsingTest;

namespace Remotion.Data.UnitTests.Linq.ParsingTest.FieldResolvingTest
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

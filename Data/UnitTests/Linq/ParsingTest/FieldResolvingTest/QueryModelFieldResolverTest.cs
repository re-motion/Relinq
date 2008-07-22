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
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Data.Linq;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing.FieldResolving;

namespace Remotion.Data.UnitTests.Linq.ParsingTest.FieldResolvingTest
{
  [TestFixture]
  public class QueryModelFieldResolverTest
  {
    private ClauseFieldResolver _resolver;
    private WhereFieldAccessPolicy _policy;
    private JoinedTableContext _context;

    [SetUp]
    public void SetUp ()
    {
      _policy = new WhereFieldAccessPolicy (StubDatabaseInfo.Instance);
      _context = new JoinedTableContext();
      _resolver = new ClauseFieldResolver (StubDatabaseInfo.Instance, _policy);
    }

    [Test]
    public void ResolveField_MainFromClause ()
    {
      QueryModel queryModel = CreateQueryExpressionForResolve ();

      Expression fieldAccessExpression = Expression.Parameter (typeof (String), "s1");
      FieldDescriptor descriptor = new QueryModelFieldResolver(queryModel).ResolveField (_resolver, fieldAccessExpression, _context);

      IColumnSource expectedTable = queryModel.MainFromClause.GetFromSource (StubDatabaseInfo.Instance);
      FieldSourcePath expectedPath = new FieldSourcePath(expectedTable, new SingleJoin[0]);

      //Assert.AreSame (queryModel.MainFromClause, descriptor.FromClause);
      Assert.AreEqual (new Column (expectedTable, "*"), descriptor.Column);
      Assert.IsNull (descriptor.Member);
      Assert.AreEqual (expectedPath, descriptor.SourcePath);
    }

    [Test]
    public void ResolveField_LetClause_Table ()
    {
      ParameterExpression identifier = Expression.Parameter (typeof (Student), "x");
      LetClause letClause = ExpressionHelper.CreateLetClause (identifier); // let x = ...

      QueryModel queryModel = ExpressionHelper.CreateQueryModel ();
      queryModel.AddBodyClause (letClause);

      Expression fieldAccessExpression = Expression.Parameter (typeof (Student), "x"); // where x == ...
      FieldDescriptor descriptor = new QueryModelFieldResolver (queryModel).ResolveField (_resolver, fieldAccessExpression, _context);
      
      LetColumnSource expectedEvaluation = new LetColumnSource ("x", true);
      Assert.AreEqual (new Column (expectedEvaluation, "IDColumn"), descriptor.Column);
    }

    [Test]
    public void ResolveField_LetClause_Column ()
    {
      ParameterExpression identifier = Expression.Parameter (typeof (int), "x");
      LetClause letClause = ExpressionHelper.CreateLetClause (identifier); // let x = ...

      QueryModel queryModel = ExpressionHelper.CreateQueryModel ();
      queryModel.AddBodyClause (letClause);

      Expression fieldAccessExpression = Expression.Parameter (typeof (int), "x"); // where x == ...
      FieldDescriptor descriptor = new QueryModelFieldResolver (queryModel).ResolveField (_resolver, fieldAccessExpression, _context);

      LetColumnSource expectedEvaluation = new LetColumnSource ("x", false);
      Assert.AreEqual (new Column (expectedEvaluation, null), descriptor.Column);
    }

    [Test]
    [ExpectedException (typeof (FieldAccessResolveException), ExpectedMessage = "The field access expression 'fzlbf' does "
        + "not contain a from clause identifier.")]
    public void NoFromIdentifierFound ()
    {
      QueryModel queryModel = CreateQueryExpressionForResolve ();
      Expression sourceExpression = Expression.Parameter (typeof (Student), "fzlbf");

      new QueryModelFieldResolver (queryModel).ResolveField (_resolver, sourceExpression, _context);
    }

    [Test]
    public void ResolveInParentQuery ()
    {
      QueryModel parentQueryModel = CreateQueryExpressionForResolve ();
      QueryModel subQueryModel =
          ExpressionHelper.CreateQueryModel (new MainFromClause (Expression.Parameter (typeof (Student), "a"), Expression.Constant (null)));
      subQueryModel.SetParentQuery (parentQueryModel);
      Expression sourceExpression = Expression.Parameter (typeof (string), "s1");

      QueryModelFieldResolver fieldResolver = new QueryModelFieldResolver (subQueryModel);

      FieldDescriptor fieldDescriptor = fieldResolver.ResolveField (_resolver, sourceExpression, _context);
      Assert.AreEqual (parentQueryModel.MainFromClause.JoinClauses, fieldDescriptor.SourcePath.Joins);
    }

    private QueryModel CreateQueryExpressionForResolve ()
    {
      ParameterExpression s1 = Expression.Parameter (typeof (String), "s1");
      ParameterExpression s2 = Expression.Parameter (typeof (String), "s2");
      MainFromClause mainFromClause = ExpressionHelper.CreateMainFromClause(s1, ExpressionHelper.CreateQuerySource ());
      AdditionalFromClause additionalFromClause =
          new AdditionalFromClause (mainFromClause, s2, ExpressionHelper.CreateLambdaExpression (), ExpressionHelper.CreateLambdaExpression ());

      var expression = ExpressionHelper.CreateQueryModel (mainFromClause);
      
      expression.AddBodyClause (additionalFromClause);

      return expression;
    }
  }
}

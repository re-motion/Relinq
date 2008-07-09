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
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing.Structure;
using Remotion.Data.Linq.UnitTests.TestQueryGenerators;

namespace Remotion.Data.Linq.UnitTests.ClausesTest
{
  [TestFixture]
  public class ClauseFinderTest
  {
    [Test]
    public void FindClause_Null ()
    {
      IQueryable<Student> query = SelectTestQueryGenerator.CreateSimpleQuery (ExpressionHelper.CreateQuerySource ());
      QueryModel queryModel = new QueryParser (query.Expression).GetParsedQuery ();
      MainFromClause fromClause = queryModel.MainFromClause;
      Assert.IsNull (ClauseFinder.FindClause<SelectClause> (fromClause));
    }

    [Test]
    public void FindClause_WithNull ()
    {
      Assert.IsNull (ClauseFinder.FindClause<SelectClause> (null));
    }

    [Test]
    public void FindClause_Self()
    {
      IQueryable<Student> query = SelectTestQueryGenerator.CreateSimpleQuery (ExpressionHelper.CreateQuerySource ());
      QueryModel queryModel = new QueryParser (query.Expression).GetParsedQuery ();
      SelectClause selectClause = (SelectClause) queryModel.SelectOrGroupClause;
      
      Assert.AreSame (selectClause, ClauseFinder.FindClause<SelectClause> (selectClause));
    }

    [Test]
    public void FindClause_Previous ()
    {
      IQueryable<Student> query = SelectTestQueryGenerator.CreateSimpleQuery (ExpressionHelper.CreateQuerySource ());
      QueryModel queryModel = new QueryParser (query.Expression).GetParsedQuery ();
      SelectClause selectClause = (SelectClause) queryModel.SelectOrGroupClause;

      Assert.AreSame (selectClause.PreviousClause, ClauseFinder.FindClause<MainFromClause> (selectClause));
    }

    [Test]
    public void FindClause_PreviousWithBaseType ()
    {
      IQueryable<Student> query = SelectTestQueryGenerator.CreateSimpleQuery (ExpressionHelper.CreateQuerySource ());
      QueryModel queryModel = new QueryParser (query.Expression).GetParsedQuery ();
      SelectClause selectClause = (SelectClause) queryModel.SelectOrGroupClause;

      Assert.AreSame (selectClause.PreviousClause, ClauseFinder.FindClause<FromClauseBase> (selectClause));
    }

    [Test]
    public void FindLastFromClause_MainFromClause ()
    {
      IQueryable<Student> query = SelectTestQueryGenerator.CreateSimpleQuery (ExpressionHelper.CreateQuerySource ());
      QueryModel queryModel = new QueryParser (query.Expression).GetParsedQuery ();
      SelectClause selectClause = (SelectClause) queryModel.SelectOrGroupClause;
      Assert.AreSame (queryModel.MainFromClause, ClauseFinder.FindClause <FromClauseBase> (selectClause.PreviousClause));
    }

    [Test]
    public void FindLastFromClause_AdditionalFromClause ()
    {
      IQueryable<Student> source = ExpressionHelper.CreateQuerySource ();
      IQueryable<Student> query = FromTestQueryGenerator.CreateThreeFromQuery (source, source, source);
      QueryModel queryModel = new QueryParser (query.Expression).GetParsedQuery ();
      SelectClause selectClause = (SelectClause) queryModel.SelectOrGroupClause;
      Assert.AreSame (queryModel.BodyClauses.Last (), ClauseFinder.FindClause<FromClauseBase> (selectClause.PreviousClause));
    }

    [Test]
    public void FindLastFromClause_WhereClause ()
    {
      IQueryable<Student> source = ExpressionHelper.CreateQuerySource ();
      IQueryable<Student> query = WhereTestQueryGenerator.CreateMultiWhereQuery (source);
      QueryModel queryModel = new QueryParser (query.Expression).GetParsedQuery ();
      SelectClause selectClause = (SelectClause) queryModel.SelectOrGroupClause;
      Assert.AreSame (queryModel.MainFromClause, ClauseFinder.FindClause<FromClauseBase> (selectClause.PreviousClause));
    }

    [Test]
    public void FindClauses()
    {
      IQueryable<Student> source = ExpressionHelper.CreateQuerySource ();
      IQueryable<Student> query = MixedTestQueryGenerator.CreateThreeFromWhereQuery (source, source, source);
      QueryModel queryModel = new QueryParser (query.Expression).GetParsedQuery ();
      SelectClause selectClause = (SelectClause) queryModel.SelectOrGroupClause;
      Assert.That (ClauseFinder.FindClauses<FromClauseBase> (selectClause).ToArray(),Is.EqualTo(new object[]
          {
              queryModel.BodyClauses.Last(), 
              queryModel.BodyClauses.First(), 
              queryModel.MainFromClause
          }));
    }

    [Test]
    public void FindClauses_StartsFromAdditionalFromClause ()
    {
      IQueryable<Student> source = ExpressionHelper.CreateQuerySource ();
      IQueryable<Student> query = MixedTestQueryGenerator.CreateThreeFromWhereQuery (source, source, source);
      QueryModel queryModel = new QueryParser (query.Expression).GetParsedQuery ();
      Assert.That (ClauseFinder.FindClauses<FromClauseBase> 
        (queryModel.BodyClauses.Last ().PreviousClause).ToArray (), Is.EqualTo (new object[]
          {
              queryModel.BodyClauses.First(), 
              queryModel.MainFromClause
          }));
    }
  }
}

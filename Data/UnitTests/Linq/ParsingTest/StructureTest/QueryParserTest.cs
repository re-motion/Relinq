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
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.Parsing.Structure;
using Remotion.Data.UnitTests.Linq.TestQueryGenerators;

namespace Remotion.Data.UnitTests.Linq.ParsingTest.StructureTest
{
  [TestFixture]
  public class QueryParserTest
  {
    private Expression _expression;
    private QueryParser _parser;

    [SetUp]
    public void SetUp()
    {
      _expression = SelectTestQueryGenerator.CreateSimpleQuery (ExpressionHelper.CreateQuerySource()).Expression;
      _parser = new QueryParser (_expression);
    }

    [Test]
    public void Initialize()
    {
      Assert.AreSame (_expression, _parser.SourceExpression);
    }

    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "The expression 'WriteLine()' could not be evaluated as a query source because it " 
        + "cannot be compiled: Argument types do not match")]
    [Ignore ("TODO: Fix bug COMMONS-693")]
    public void Initialize_FromWrongExpression ()
    {
      MethodCallExpression expression = Expression.Call (typeof (Console), "WriteLine", Type.EmptyTypes);
      new QueryParser (expression).GetParsedQuery();
    }

    [Test]
    public void GetParsedQuery()
    {
      Assert.IsNotNull (_parser.GetParsedQuery());
    }

    [Test]
    public void ParsedQuery_StoresExpressionTree ()
    {
      QueryModel queryModel = _parser.GetParsedQuery ();
      Assert.AreSame (_expression, queryModel.GetExpressionTree());
    }

    [Test]
    public void PreviousClauses_SimpleQuery()
    {
      QueryModel parsedQuery = _parser.GetParsedQuery();
      Assert.IsNull (parsedQuery.MainFromClause.PreviousClause);
      Assert.AreSame (parsedQuery.MainFromClause, parsedQuery.SelectOrGroupClause.PreviousClause);
    }

    [Test]
    public void PreviousClauses_LargeQuery ()
    {
      IQueryable<Student> source = ExpressionHelper.CreateQuerySource();
      Expression queryExpression = MixedTestQueryGenerator.CreateMultiFromWhereQuery (source, source).Expression;
      QueryParser parser = new QueryParser (queryExpression);
      QueryModel parsedQuery = parser.GetParsedQuery ();
      
      Assert.IsNull (parsedQuery.MainFromClause.PreviousClause);
      Assert.AreSame (parsedQuery.BodyClauses.First(), parsedQuery.BodyClauses.Last ().PreviousClause);
      Assert.AreSame (parsedQuery.BodyClauses.Last(), parsedQuery.SelectOrGroupClause.PreviousClause);
    }

    [Test]
    public void PreviousClauses_MultiFromWhereOrderByQuery()
    {
      IQueryable<Student> source = ExpressionHelper.CreateQuerySource();
      Expression queryExpression = MixedTestQueryGenerator.CreateMultiFromWhereOrderByQuery (source, source).Expression;
      QueryParser parser = new QueryParser (queryExpression);
      QueryModel parsedQuery = parser.GetParsedQuery();

      Assert.IsNull (parsedQuery.MainFromClause.PreviousClause);
      Assert.AreSame (parsedQuery.BodyClauses.First(), parsedQuery.BodyClauses.Skip(1).First().PreviousClause);
      Assert.AreSame (parsedQuery.BodyClauses.Skip (1).First (), parsedQuery.BodyClauses.Last().PreviousClause);
    }






  }
}

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
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Expressions;
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
    [ExpectedException (typeof (ParserException), ExpectedMessage = "Parsing of expression 'WriteLine()' is not supported. The expression was " 
        + "interpreted as a from source, but there is no from identifier matching it in expression tree 'WriteLine()'.")]
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
    public void ParsedQuery_Simplifies ()
    {
      Expression simplifyableQuery = SelectTestQueryGenerator.CreateSimplifyableQuery (ExpressionHelper.CreateQuerySource()).Expression;
      QueryParser parser = new QueryParser (simplifyableQuery);

      QueryModel queryModel = parser.GetParsedQuery ();
      Assert.That (((SelectClause)queryModel.SelectOrGroupClause).ProjectionExpression.Body, Is.InstanceOfType (typeof (ConstantExpression)));
    }

    [Test]
    public void ParsedQuery_SubQueries_HaveParentSet ()
    {
      Expression queryWithSubQuery = SubQueryTestQueryGenerator.CreateSimpleSubQueryInWhereClause (ExpressionHelper.CreateQuerySource ()).Expression;
      QueryParser parser = new QueryParser (queryWithSubQuery);

      QueryModel queryModel = parser.GetParsedQuery ();
      WhereClause whereClause = ((WhereClause) queryModel.BodyClauses[0]);
      MethodCallExpression containsExpression = (MethodCallExpression) whereClause.BoolExpression.Body;
      SubQueryExpression subQueryExpression = (SubQueryExpression) containsExpression.Arguments[0];

      Assert.That (subQueryExpression.QueryModel.ParentQuery, Is.SameAs (queryModel));
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

using System;
using System.Linq;
using NUnit.Framework;
using Rubicon.Data.DomainObjects.Linq.Clauses;
using Rubicon.Data.DomainObjects.Linq.Parsing;

namespace Rubicon.Data.DomainObjects.Linq.UnitTests.ClausesTest
{
  [TestFixture]
  public class ClauseFinderHelperTest
  {
    [Test]
    public void FindClause_Null ()
    {
      IQueryable<Student> query = TestQueryGenerator.CreateSimpleQuery (ExpressionHelper.CreateQuerySource ());
      QueryExpression queryExpression = new QueryParser (query.Expression).GetParsedQuery ();
      MainFromClause fromClause = queryExpression.FromClause;
      Assert.IsNull (ClauseFinderHelper.FindClause<SelectClause> (fromClause));
    }

    [Test]
    public void FindClause_WithNull ()
    {
      Assert.IsNull (ClauseFinderHelper.FindClause<SelectClause> (null));
    }

    [Test]
    public void FindClause_Self()
    {
      IQueryable<Student> query = TestQueryGenerator.CreateSimpleQuery (ExpressionHelper.CreateQuerySource ());
      QueryExpression queryExpression = new QueryParser (query.Expression).GetParsedQuery ();
      SelectClause selectClause = (SelectClause) queryExpression.QueryBody.SelectOrGroupClause;
      
      Assert.AreSame (selectClause, ClauseFinderHelper.FindClause<SelectClause> (selectClause));
    }

    [Test]
    public void FindClause_Previous ()
    {
      IQueryable<Student> query = TestQueryGenerator.CreateSimpleQuery (ExpressionHelper.CreateQuerySource ());
      QueryExpression queryExpression = new QueryParser (query.Expression).GetParsedQuery ();
      SelectClause selectClause = (SelectClause) queryExpression.QueryBody.SelectOrGroupClause;

      Assert.AreSame (selectClause.PreviousClause, ClauseFinderHelper.FindClause<MainFromClause> (selectClause));
    }

    [Test]
    public void FindClause_PreviousWithBaseType ()
    {
      IQueryable<Student> query = TestQueryGenerator.CreateSimpleQuery (ExpressionHelper.CreateQuerySource ());
      QueryExpression queryExpression = new QueryParser (query.Expression).GetParsedQuery ();
      SelectClause selectClause = (SelectClause) queryExpression.QueryBody.SelectOrGroupClause;

      Assert.AreSame (selectClause.PreviousClause, ClauseFinderHelper.FindClause<FromClauseBase> (selectClause));
    }

    [Test]
    public void FindLastFromClause_MainFromClause ()
    {
      IQueryable<Student> query = TestQueryGenerator.CreateSimpleQuery (ExpressionHelper.CreateQuerySource ());
      QueryExpression queryExpression = new QueryParser (query.Expression).GetParsedQuery ();
      SelectClause selectClause = (SelectClause) queryExpression.QueryBody.SelectOrGroupClause;
      Assert.AreSame (queryExpression.FromClause, ClauseFinderHelper.FindClause <FromClauseBase> (selectClause.PreviousClause));
    }

    [Test]
    public void FindLastFromClause_AdditionalFromClause ()
    {
      IQueryable<Student> source = ExpressionHelper.CreateQuerySource ();
      IQueryable<Student> query = TestQueryGenerator.CreateThreeFromQuery (source, source, source);
      QueryExpression queryExpression = new QueryParser (query.Expression).GetParsedQuery ();
      SelectClause selectClause = (SelectClause) queryExpression.QueryBody.SelectOrGroupClause;
      Assert.AreSame (queryExpression.QueryBody.FromLetWhereClauses.Last (), ClauseFinderHelper.FindClause<FromClauseBase> (selectClause.PreviousClause));
    }

    [Test]
    public void FindLastFromClause_WhereClause ()
    {
      IQueryable<Student> source = ExpressionHelper.CreateQuerySource ();
      IQueryable<Student> query = TestQueryGenerator.CreateMultiWhereQuery (source);
      QueryExpression queryExpression = new QueryParser (query.Expression).GetParsedQuery ();
      SelectClause selectClause = (SelectClause) queryExpression.QueryBody.SelectOrGroupClause;
      Assert.AreSame (queryExpression.FromClause, ClauseFinderHelper.FindClause<FromClauseBase> (selectClause.PreviousClause));
    }
  }
}
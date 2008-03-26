using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Data.Linq.Parsing.Structure;
using Rubicon.Data.Linq.UnitTests.TestQueryGenerators;

namespace Rubicon.Data.Linq.UnitTests.ClausesTest
{
  [TestFixture]
  public class ClauseFinderTest
  {
    [Test]
    public void FindClause_Null ()
    {
      IQueryable<Student> query = SelectTestQueryGenerator.CreateSimpleQuery (ExpressionHelper.CreateQuerySource ());
      QueryExpression queryExpression = new QueryParser (query.Expression).GetParsedQuery ();
      MainFromClause fromClause = queryExpression.MainFromClause;
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
      QueryExpression queryExpression = new QueryParser (query.Expression).GetParsedQuery ();
      SelectClause selectClause = (SelectClause) queryExpression.QueryBody.SelectOrGroupClause;
      
      Assert.AreSame (selectClause, ClauseFinder.FindClause<SelectClause> (selectClause));
    }

    [Test]
    public void FindClause_Previous ()
    {
      IQueryable<Student> query = SelectTestQueryGenerator.CreateSimpleQuery (ExpressionHelper.CreateQuerySource ());
      QueryExpression queryExpression = new QueryParser (query.Expression).GetParsedQuery ();
      SelectClause selectClause = (SelectClause) queryExpression.QueryBody.SelectOrGroupClause;

      Assert.AreSame (selectClause.PreviousClause, ClauseFinder.FindClause<MainFromClause> (selectClause));
    }

    [Test]
    public void FindClause_PreviousWithBaseType ()
    {
      IQueryable<Student> query = SelectTestQueryGenerator.CreateSimpleQuery (ExpressionHelper.CreateQuerySource ());
      QueryExpression queryExpression = new QueryParser (query.Expression).GetParsedQuery ();
      SelectClause selectClause = (SelectClause) queryExpression.QueryBody.SelectOrGroupClause;

      Assert.AreSame (selectClause.PreviousClause, ClauseFinder.FindClause<FromClauseBase> (selectClause));
    }

    [Test]
    public void FindLastFromClause_MainFromClause ()
    {
      IQueryable<Student> query = SelectTestQueryGenerator.CreateSimpleQuery (ExpressionHelper.CreateQuerySource ());
      QueryExpression queryExpression = new QueryParser (query.Expression).GetParsedQuery ();
      SelectClause selectClause = (SelectClause) queryExpression.QueryBody.SelectOrGroupClause;
      Assert.AreSame (queryExpression.MainFromClause, ClauseFinder.FindClause <FromClauseBase> (selectClause.PreviousClause));
    }

    [Test]
    public void FindLastFromClause_AdditionalFromClause ()
    {
      IQueryable<Student> source = ExpressionHelper.CreateQuerySource ();
      IQueryable<Student> query = FromTestQueryGenerator.CreateThreeFromQuery (source, source, source);
      QueryExpression queryExpression = new QueryParser (query.Expression).GetParsedQuery ();
      SelectClause selectClause = (SelectClause) queryExpression.QueryBody.SelectOrGroupClause;
      Assert.AreSame (queryExpression.QueryBody.BodyClauses.Last (), ClauseFinder.FindClause<FromClauseBase> (selectClause.PreviousClause));
    }

    [Test]
    public void FindLastFromClause_WhereClause ()
    {
      IQueryable<Student> source = ExpressionHelper.CreateQuerySource ();
      IQueryable<Student> query = WhereTestQueryGenerator.CreateMultiWhereQuery (source);
      QueryExpression queryExpression = new QueryParser (query.Expression).GetParsedQuery ();
      SelectClause selectClause = (SelectClause) queryExpression.QueryBody.SelectOrGroupClause;
      Assert.AreSame (queryExpression.MainFromClause, ClauseFinder.FindClause<FromClauseBase> (selectClause.PreviousClause));
    }

    [Test]
    public void FindClauses()
    {
      IQueryable<Student> source = ExpressionHelper.CreateQuerySource ();
      IQueryable<Student> query = MixedTestQueryGenerator.CreateThreeFromWhereQuery (source, source, source);
      QueryExpression queryExpression = new QueryParser (query.Expression).GetParsedQuery ();
      SelectClause selectClause = (SelectClause) queryExpression.QueryBody.SelectOrGroupClause;
      Assert.That (ClauseFinder.FindClauses<FromClauseBase> (selectClause).ToArray(),Is.EqualTo(new object[]
          {
              queryExpression.QueryBody.BodyClauses.Last(), 
              queryExpression.QueryBody.BodyClauses.First(), 
              queryExpression.MainFromClause
          }));
    }

    [Test]
    public void FindClauses_StartsFromAdditionalFromClause ()
    {
      IQueryable<Student> source = ExpressionHelper.CreateQuerySource ();
      IQueryable<Student> query = MixedTestQueryGenerator.CreateThreeFromWhereQuery (source, source, source);
      QueryExpression queryExpression = new QueryParser (query.Expression).GetParsedQuery ();
      Assert.That (ClauseFinder.FindClauses<FromClauseBase> 
        (queryExpression.QueryBody.BodyClauses.Last ().PreviousClause).ToArray (), Is.EqualTo (new object[]
          {
              queryExpression.QueryBody.BodyClauses.First(), 
              queryExpression.MainFromClause
          }));
    }
  }
}
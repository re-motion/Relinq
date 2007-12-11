using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Data.Linq.Parsing;

namespace Rubicon.Data.Linq.UnitTests.ClausesTest
{
  [TestFixture]
  public class ClauseFinderTest
  {
    [Test]
    public void FindClause_Null ()
    {
      IQueryable<Student> query = TestQueryGenerator.CreateSimpleQuery (ExpressionHelper.CreateQuerySource ());
      QueryExpression queryExpression = new QueryParser (query.Expression).GetParsedQuery ();
      MainFromClause fromClause = queryExpression.FromClause;
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
      IQueryable<Student> query = TestQueryGenerator.CreateSimpleQuery (ExpressionHelper.CreateQuerySource ());
      QueryExpression queryExpression = new QueryParser (query.Expression).GetParsedQuery ();
      SelectClause selectClause = (SelectClause) queryExpression.QueryBody.SelectOrGroupClause;
      
      Assert.AreSame (selectClause, ClauseFinder.FindClause<SelectClause> (selectClause));
    }

    [Test]
    public void FindClause_Previous ()
    {
      IQueryable<Student> query = TestQueryGenerator.CreateSimpleQuery (ExpressionHelper.CreateQuerySource ());
      QueryExpression queryExpression = new QueryParser (query.Expression).GetParsedQuery ();
      SelectClause selectClause = (SelectClause) queryExpression.QueryBody.SelectOrGroupClause;

      Assert.AreSame (selectClause.PreviousClause, ClauseFinder.FindClause<MainFromClause> (selectClause));
    }

    [Test]
    public void FindClause_PreviousWithBaseType ()
    {
      IQueryable<Student> query = TestQueryGenerator.CreateSimpleQuery (ExpressionHelper.CreateQuerySource ());
      QueryExpression queryExpression = new QueryParser (query.Expression).GetParsedQuery ();
      SelectClause selectClause = (SelectClause) queryExpression.QueryBody.SelectOrGroupClause;

      Assert.AreSame (selectClause.PreviousClause, ClauseFinder.FindClause<FromClauseBase> (selectClause));
    }

    [Test]
    public void FindLastFromClause_MainFromClause ()
    {
      IQueryable<Student> query = TestQueryGenerator.CreateSimpleQuery (ExpressionHelper.CreateQuerySource ());
      QueryExpression queryExpression = new QueryParser (query.Expression).GetParsedQuery ();
      SelectClause selectClause = (SelectClause) queryExpression.QueryBody.SelectOrGroupClause;
      Assert.AreSame (queryExpression.FromClause, ClauseFinder.FindClause <FromClauseBase> (selectClause.PreviousClause));
    }

    [Test]
    public void FindLastFromClause_AdditionalFromClause ()
    {
      IQueryable<Student> source = ExpressionHelper.CreateQuerySource ();
      IQueryable<Student> query = TestQueryGenerator.CreateThreeFromQuery (source, source, source);
      QueryExpression queryExpression = new QueryParser (query.Expression).GetParsedQuery ();
      SelectClause selectClause = (SelectClause) queryExpression.QueryBody.SelectOrGroupClause;
      Assert.AreSame (queryExpression.QueryBody.FromLetWhereClauses.Last (), ClauseFinder.FindClause<FromClauseBase> (selectClause.PreviousClause));
    }

    [Test]
    public void FindLastFromClause_WhereClause ()
    {
      IQueryable<Student> source = ExpressionHelper.CreateQuerySource ();
      IQueryable<Student> query = TestQueryGenerator.CreateMultiWhereQuery (source);
      QueryExpression queryExpression = new QueryParser (query.Expression).GetParsedQuery ();
      SelectClause selectClause = (SelectClause) queryExpression.QueryBody.SelectOrGroupClause;
      Assert.AreSame (queryExpression.FromClause, ClauseFinder.FindClause<FromClauseBase> (selectClause.PreviousClause));
    }

    [Test]
    public void FindClauses()
    {
      IQueryable<Student> source = ExpressionHelper.CreateQuerySource ();
      IQueryable<Student> query = TestQueryGenerator.CreateThreeFromWhereQuery (source,source,source);
      QueryExpression queryExpression = new QueryParser (query.Expression).GetParsedQuery ();
      SelectClause selectClause = (SelectClause) queryExpression.QueryBody.SelectOrGroupClause;
      Assert.That (ClauseFinder.FindClauses<FromClauseBase> (selectClause).ToArray(),Is.EqualTo(new object[]
          {
              queryExpression.QueryBody.FromLetWhereClauses.Last(), 
              queryExpression.QueryBody.FromLetWhereClauses.First(), 
              queryExpression.FromClause
          }));
    }

    [Test]
    public void FindClauses_StartsFromAdditionalFromClause ()
    {
      IQueryable<Student> source = ExpressionHelper.CreateQuerySource ();
      IQueryable<Student> query = TestQueryGenerator.CreateThreeFromWhereQuery (source, source, source);
      QueryExpression queryExpression = new QueryParser (query.Expression).GetParsedQuery ();
      Assert.That (ClauseFinder.FindClauses<FromClauseBase> 
        (queryExpression.QueryBody.FromLetWhereClauses.Last ().PreviousClause).ToArray (), Is.EqualTo (new object[]
          {
              queryExpression.QueryBody.FromLetWhereClauses.First(), 
              queryExpression.FromClause
          }));
    }

    [Test]
    public void FindFromClauseForIdentifierName_StartsFromSelect ()
    {
      IQueryable<Student> query =
          TestQueryGenerator.CreateThreeFromWhereQuery (ExpressionHelper.CreateQuerySource (), ExpressionHelper.CreateQuerySource (),
              ExpressionHelper.CreateQuerySource ());
      QueryExpression queryExpression = ExpressionHelper.ParseQuery (query);

      FromClauseBase mainFromClause = ClauseFinder.FindFromClauseForIdentifierName (queryExpression.QueryBody.SelectOrGroupClause, "s1");
      Assert.AreSame (queryExpression.FromClause, mainFromClause);

      FromClauseBase additionalFromClause = ClauseFinder.FindFromClauseForIdentifierName (queryExpression.QueryBody.SelectOrGroupClause, "s2");
      Assert.AreSame (queryExpression.QueryBody.FromLetWhereClauses.First (), additionalFromClause);

      FromClauseBase additionalFromClause2 = ClauseFinder.FindFromClauseForIdentifierName (queryExpression.QueryBody.SelectOrGroupClause, "s3");
      Assert.AreSame (queryExpression.QueryBody.FromLetWhereClauses.Last (), additionalFromClause2);
    }

    [Test]
    public void FindFromClauseForIdentifierName_StartsFromWhere ()
    {
      IQueryable<Student> query =
          TestQueryGenerator.CreateThreeFromWhereQuery (ExpressionHelper.CreateQuerySource (), ExpressionHelper.CreateQuerySource (),
              ExpressionHelper.CreateQuerySource ());
      QueryExpression queryExpression = ExpressionHelper.ParseQuery (query);

      WhereClause startingPoint = (WhereClause) queryExpression.QueryBody.FromLetWhereClauses.Skip (1).First ();

      FromClauseBase mainFromClause = ClauseFinder.FindFromClauseForIdentifierName (startingPoint, "s1");
      Assert.AreSame (queryExpression.FromClause, mainFromClause);

      FromClauseBase additionalFromClause = ClauseFinder.FindFromClauseForIdentifierName (startingPoint, "s2");
      Assert.AreSame (queryExpression.QueryBody.FromLetWhereClauses.First (), additionalFromClause);
    }

    [Test]
    [ExpectedException (typeof (QueryParserException),
        ExpectedMessage = "The identifier 's8' is not defined in a from clause previous to the given SelectClause.")]
    public void FindFromClauseForIdentifierName_InvalidFromIdentifier ()
    {
      IQueryable<Student> query =
          TestQueryGenerator.CreateThreeFromWhereQuery (ExpressionHelper.CreateQuerySource (), ExpressionHelper.CreateQuerySource (),
              ExpressionHelper.CreateQuerySource ());
      QueryExpression queryExpression = ExpressionHelper.ParseQuery (query);

      ClauseFinder.FindFromClauseForIdentifierName (queryExpression.QueryBody.SelectOrGroupClause, "s8");
    }

    [Test]
    [ExpectedException (typeof (QueryParserException),
        ExpectedMessage = "The identifier 's3' is not defined in a from clause previous to the given WhereClause.")]
    public void FindFromClauseForIdentifierName_StartsFromStartingPoint ()
    {
      IQueryable<Student> query =
          TestQueryGenerator.CreateThreeFromWhereQuery (ExpressionHelper.CreateQuerySource (), ExpressionHelper.CreateQuerySource (),
              ExpressionHelper.CreateQuerySource ());
      QueryExpression queryExpression = ExpressionHelper.ParseQuery (query);

      WhereClause startingPoint = (WhereClause) queryExpression.QueryBody.FromLetWhereClauses.Skip (1).First ();
      ClauseFinder.FindFromClauseForIdentifierName (startingPoint, "s3");
    }

    [Test]
    public void FindFromClauseForExpression_ParameterExpression ()
    {
      IQueryable<Student> query = TestQueryGenerator.CreateSimpleQuery (ExpressionHelper.CreateQuerySource ());
      QueryExpression queryExpression = ExpressionHelper.ParseQuery (query);

      SelectClause selectClause = (SelectClause) queryExpression.QueryBody.SelectOrGroupClause;
      Assert.IsInstanceOfType (typeof (ParameterExpression), selectClause.ProjectionExpression.Body);
      FromClauseBase fromClause = ClauseFinder.FindFromClauseForExpression (selectClause, selectClause.ProjectionExpression.Body);
      Assert.AreSame (queryExpression.FromClause, fromClause);
    }

    [Test]
    public void FindFromClauseForExpression_MemberExpression ()
    {
      IQueryable<Student> query = TestQueryGenerator.CreateThreeFromWhereQuery (ExpressionHelper.CreateQuerySource (),
          ExpressionHelper.CreateQuerySource (), ExpressionHelper.CreateQuerySource ());
      QueryExpression queryExpression = ExpressionHelper.ParseQuery (query);

      SelectClause selectClause = (SelectClause) queryExpression.QueryBody.SelectOrGroupClause;
      Assert.IsInstanceOfType (typeof (MemberExpression), selectClause.ProjectionExpression.Body);
      FromClauseBase fromClause = ClauseFinder.FindFromClauseForExpression (selectClause, selectClause.ProjectionExpression.Body);
      Assert.AreSame (queryExpression.FromClause, fromClause);
    }

    [Test]
    public void FindFromClauseForExpression_MemberExpression_AdditionalFromClause ()
    {
      IQueryable<Student> query = TestQueryGenerator.CreateThreeFromQueryWithSelectS2 (ExpressionHelper.CreateQuerySource (),
          ExpressionHelper.CreateQuerySource (), ExpressionHelper.CreateQuerySource ());
      QueryExpression queryExpression = ExpressionHelper.ParseQuery (query);

      SelectClause selectClause = (SelectClause) queryExpression.QueryBody.SelectOrGroupClause;
      Assert.IsInstanceOfType (typeof (MemberExpression), selectClause.ProjectionExpression.Body);
      FromClauseBase fromClause = ClauseFinder.FindFromClauseForExpression (selectClause, selectClause.ProjectionExpression.Body);
      Assert.AreSame (queryExpression.QueryBody.FromLetWhereClauses.First (), fromClause);
    }

    [Test]
    [ExpectedException (typeof (QueryParserException), ExpectedMessage = "The expression cannot be parsed because the expression type NewArrayInit is not supported.",
        MatchType = MessageMatch.Contains)]
    public void FindFromClauseForExpression_InvalidExpressionKind ()
    {
      IQueryable<Student> query = TestQueryGenerator.CreateThreeFromQueryWithSelectS2 (ExpressionHelper.CreateQuerySource (),
          ExpressionHelper.CreateQuerySource (), ExpressionHelper.CreateQuerySource ());
      QueryExpression queryExpression = ExpressionHelper.ParseQuery (query);

      ClauseFinder.FindFromClauseForExpression (queryExpression.QueryBody.SelectOrGroupClause,
          ExpressionHelper.CreateNewIntArrayExpression ());
    }

    [Test]
    [ExpectedException (typeof (QueryParserException),
        ExpectedMessage = "The identifier 's4' is not defined in a from clause previous to the given SelectClause.")]
    public void FindFromClauseForExpression_IdentifierNotFound ()
    {
      IQueryable<Student> query = TestQueryGenerator.CreateThreeFromQueryWithSelectS2 (ExpressionHelper.CreateQuerySource (),
          ExpressionHelper.CreateQuerySource (), ExpressionHelper.CreateQuerySource ());
      QueryExpression queryExpression = ExpressionHelper.ParseQuery (query);

      ClauseFinder.FindFromClauseForExpression (queryExpression.QueryBody.SelectOrGroupClause,
          Expression.Parameter (typeof (Student), "s4"));
    }
  }
}
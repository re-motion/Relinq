using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Data.Linq.Parsing;

namespace Rubicon.Data.Linq.UnitTests.ParsingTest
{
  [TestFixture]
  public class FromClauseFinderTest
  {
    [Test]
    public void FindFromClauseForIdentifierName_StartsFromSelect()
    {
      IQueryable<Student> query =
          TestQueryGenerator.CreateThreeFromWhereQuery (ExpressionHelper.CreateQuerySource(), ExpressionHelper.CreateQuerySource(),
              ExpressionHelper.CreateQuerySource());
      QueryExpression queryExpression = ExpressionHelper.ParseQuery (query);

      FromClauseBase mainFromClause = FromClauseFinder.FindFromClauseForIdentifierName (queryExpression.QueryBody.SelectOrGroupClause, "s1");
      Assert.AreSame (queryExpression.FromClause, mainFromClause);

      FromClauseBase additionalFromClause = FromClauseFinder.FindFromClauseForIdentifierName (queryExpression.QueryBody.SelectOrGroupClause, "s2");
      Assert.AreSame (queryExpression.QueryBody.FromLetWhereClauses.First(), additionalFromClause);

      FromClauseBase additionalFromClause2 = FromClauseFinder.FindFromClauseForIdentifierName (queryExpression.QueryBody.SelectOrGroupClause, "s3");
      Assert.AreSame (queryExpression.QueryBody.FromLetWhereClauses.Last (), additionalFromClause2);
    }

    [Test]
    public void FindFromClauseForIdentifierName_StartsFromWhere ()
    {
      IQueryable<Student> query =
          TestQueryGenerator.CreateThreeFromWhereQuery (ExpressionHelper.CreateQuerySource (), ExpressionHelper.CreateQuerySource (),
              ExpressionHelper.CreateQuerySource ());
      QueryExpression queryExpression = ExpressionHelper.ParseQuery (query);

      WhereClause startingPoint = (WhereClause) queryExpression.QueryBody.FromLetWhereClauses.Skip (1).First();

      FromClauseBase mainFromClause = FromClauseFinder.FindFromClauseForIdentifierName (startingPoint, "s1");
      Assert.AreSame (queryExpression.FromClause, mainFromClause);

      FromClauseBase additionalFromClause = FromClauseFinder.FindFromClauseForIdentifierName (startingPoint, "s2");
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

      FromClauseFinder.FindFromClauseForIdentifierName (queryExpression.QueryBody.SelectOrGroupClause, "s8");
    }

    [Test]
    [ExpectedException (typeof (QueryParserException),
        ExpectedMessage = "The identifier 's3' is not defined in a from clause previous to the given WhereClause.")]
    public void FindFromClauseForIdentifierName_StartsFromStartingPoint ()
    {
      IQueryable<Student> query =
          TestQueryGenerator.CreateThreeFromWhereQuery (ExpressionHelper.CreateQuerySource(), ExpressionHelper.CreateQuerySource(),
              ExpressionHelper.CreateQuerySource());
      QueryExpression queryExpression = ExpressionHelper.ParseQuery (query);

      WhereClause startingPoint = (WhereClause) queryExpression.QueryBody.FromLetWhereClauses.Skip (1).First();
      FromClauseFinder.FindFromClauseForIdentifierName (startingPoint, "s3");
    }

    [Test]
    public void FindFromClauseForExpression_ParameterExpression ()
    {
      IQueryable<Student> query = TestQueryGenerator.CreateSimpleQuery (ExpressionHelper.CreateQuerySource ());
      QueryExpression queryExpression = ExpressionHelper.ParseQuery (query);

      SelectClause selectClause = (SelectClause) queryExpression.QueryBody.SelectOrGroupClause;
      Assert.IsInstanceOfType (typeof (ParameterExpression), selectClause.ProjectionExpression.Body);
      FromClauseBase fromClause = FromClauseFinder.FindFromClauseForExpression (selectClause, selectClause.ProjectionExpression.Body);
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
      FromClauseBase fromClause = FromClauseFinder.FindFromClauseForExpression (selectClause, selectClause.ProjectionExpression.Body);
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
      FromClauseBase fromClause = FromClauseFinder.FindFromClauseForExpression (selectClause, selectClause.ProjectionExpression.Body);
      Assert.AreSame (queryExpression.QueryBody.FromLetWhereClauses.First(), fromClause);
    }

    [Test]
    [ExpectedException (typeof (QueryParserException), ExpectedMessage = "The expression cannot be parsed because the expression type NewArrayInit is not supported.",
        MatchType = MessageMatch.Contains)]
    public void FindFromClauseForExpression_InvalidExpressionKind ()
    {
      IQueryable<Student> query = TestQueryGenerator.CreateThreeFromQueryWithSelectS2 (ExpressionHelper.CreateQuerySource (),
          ExpressionHelper.CreateQuerySource (), ExpressionHelper.CreateQuerySource ());
      QueryExpression queryExpression = ExpressionHelper.ParseQuery (query);

      FromClauseFinder.FindFromClauseForExpression (queryExpression.QueryBody.SelectOrGroupClause,
          ExpressionHelper.CreateNewIntArrayExpression());
    }

    [Test]
    [ExpectedException (typeof (QueryParserException),
        ExpectedMessage = "The identifier 's4' is not defined in a from clause previous to the given SelectClause.")]
    public void FindFromClauseForExpression_IdentifierNotFound ()
    {
      IQueryable<Student> query = TestQueryGenerator.CreateThreeFromQueryWithSelectS2 (ExpressionHelper.CreateQuerySource (),
          ExpressionHelper.CreateQuerySource (), ExpressionHelper.CreateQuerySource ());
      QueryExpression queryExpression = ExpressionHelper.ParseQuery (query);

      FromClauseFinder.FindFromClauseForExpression (queryExpression.QueryBody.SelectOrGroupClause,
          Expression.Parameter (typeof (Student), "s4"));
    }
  }
}
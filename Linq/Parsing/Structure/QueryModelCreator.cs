using System;
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Data.Linq.Parsing.Structure;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.Parsing.Structure
{
  public class QueryModelCreator
  {
    private readonly Expression _expressionTreeRoot;
    private readonly ParseResultCollector _result;
    private readonly List<IBodyClause> _bodyClauses = new List<IBodyClause> ();
    private readonly List<QueryModel> _subQueries = new List<QueryModel> ();

    private IClause _previousClause;
    private int _currentProjection;
    private OrderByClause _previousOrderByClause;

    public QueryModelCreator (Expression expressionTreeRoot, ParseResultCollector result)
    {
      ArgumentUtility.CheckNotNull ("expressionTreeRoot", expressionTreeRoot);
      ArgumentUtility.CheckNotNull ("result", result);

      _expressionTreeRoot = expressionTreeRoot;
      _result = result;
    }

    public QueryModel CreateQueryExpression()
    {
      var mainFromClause = CreateMainFromClause (_result);

      _previousClause = mainFromClause;
      _currentProjection = 0;
      _previousOrderByClause = null;

      foreach (BodyExpressionDataBase bodyExpression in _result.BodyExpressions)
      {
        var clause = CreateBodyClause (bodyExpression);
        if (clause != _previousClause)
          _bodyClauses.Add (clause);

        _previousClause = clause;
      }

      var selectClause = CreateSelectClause();
      var queryExpression = new QueryModel (_expressionTreeRoot.Type, mainFromClause, selectClause, _expressionTreeRoot);

      foreach (IBodyClause bodyClause in _bodyClauses)
        queryExpression.AddBodyClause (bodyClause);

      foreach (QueryModel subQuery in _subQueries)
        subQuery.SetParentQuery (queryExpression);

      return queryExpression;
    }

    private MainFromClause CreateMainFromClause (ParseResultCollector resultCollector)
    {
      Assertion.IsTrue (resultCollector.BodyExpressions.Count > 0 && resultCollector.BodyExpressions[0] is FromExpressionData);

      FromExpressionData mainFromExpressionData = resultCollector.ExtractMainFromExpression ();
      return new MainFromClause (mainFromExpressionData.Identifier, mainFromExpressionData.Expression);
    }

    private IBodyClause CreateBodyClause (BodyExpressionDataBase expression)
    {
      IBodyClause fromClause = CreateBodyFromClause (expression);
      if (fromClause != null)
        return fromClause;

      WhereClause whereClause = CreateWhereClause(expression);
      if (whereClause != null)
        return whereClause;

      OrderByClause orderByClause = CreateOrderByClause(expression);
      if (orderByClause != null)
        return orderByClause;

      LetClause letClause = CreateLetClause (expression);
      if (letClause != null)
        return letClause;

      throw new ParserException ("The FromLetWhereExpression type " + expression.GetType ().Name + " is not supported.");
    }

    private IBodyClause CreateBodyFromClause (BodyExpressionDataBase expression)
    {
      var fromExpression = expression as FromExpressionData;
      if (fromExpression == null)
        return null;

      if (_currentProjection >= _result.ProjectionExpressions.Count)
      {
        string message = string.Format ("From expression '{0}' ({1}) doesn't have a projection expression.", fromExpression.Identifier,
            fromExpression.Expression);
        throw new ParserException (message, _expressionTreeRoot, _expressionTreeRoot, null);
      }

      var lambdaExpression = (LambdaExpression) fromExpression.Expression;
      var projectionExpression = _result.ProjectionExpressions[_currentProjection];
      ++_currentProjection;

      return CreateBodyFromClause(fromExpression, lambdaExpression, projectionExpression);
    }

    private IBodyClause CreateBodyFromClause (FromExpressionData fromExpressionData, LambdaExpression lambdaExpression, LambdaExpression projectionExpression)
    {
      if (lambdaExpression.Body.NodeType == ExpressionType.Call)
      {
        QueryModel subQuery = CreateSubQuery(lambdaExpression);
        return new SubQueryFromClause (_previousClause, fromExpressionData.Identifier, subQuery, projectionExpression);
      }
      else
        return new AdditionalFromClause (_previousClause, fromExpressionData.Identifier, lambdaExpression, projectionExpression);
    }

    private QueryModel CreateSubQuery (LambdaExpression subQueryExpression)
    {
      QueryParser subQueryParser = new QueryParser (subQueryExpression.Body);
      QueryModel subQuery = subQueryParser.GetParsedQuery();
      _subQueries.Add (subQuery);
      return subQuery;
    }

    private WhereClause CreateWhereClause (BodyExpressionDataBase expression)
    {
      var whereExpression = expression as WhereExpressionData;
      if (whereExpression == null)
        return null;

      var newWhereExpression = new SubQueryFindingVisitor ().ReplaceSubQuery (whereExpression.Expression);
      var whereClause = new WhereClause (_previousClause, (LambdaExpression) newWhereExpression);
      return whereClause;
    }

    private OrderByClause CreateOrderByClause (BodyExpressionDataBase expression)
    {
      var orderExpression = expression as OrderExpressionData;
      if (orderExpression == null)
        return null;

      var orderingClause = new OrderingClause (_previousClause, orderExpression.Expression, orderExpression.OrderDirection);
      if (orderExpression.FirstOrderBy)
      {
        var orderByClause = new OrderByClause (orderingClause);
        _previousOrderByClause = orderByClause;
        return orderByClause;
      }
      else
      {
        if (_previousOrderByClause == null)
          throw ParserUtility.CreateParserException ("OrderBy or OrderByDescending", orderExpression, "beginning of an OrderBy clause",
              _expressionTreeRoot);
        else
        {
          _previousOrderByClause.Add (orderingClause);
          return _previousOrderByClause;
        }
      }
    }

    private LetClause CreateLetClause (BodyExpressionDataBase expression)
    {
      var letExpression = expression as LetExpressionData;

      if (letExpression == null)
        return null;

      if (_currentProjection >= _result.ProjectionExpressions.Count)
      {
        string message = string.Format ("Let expression '{0}' ({1}) doesn't have a projection expression.", letExpression.Identifier,
            letExpression.Expression);
        throw new ParserException (message, _expressionTreeRoot, _expressionTreeRoot, null);
      }

      var projectionExpression = _result.ProjectionExpressions[_currentProjection];
      var letClause = new LetClause (_previousClause, letExpression.Identifier, letExpression.Expression, projectionExpression);
      ++_currentProjection;

      return letClause;
    }

    private SelectClause CreateSelectClause ()
    {
      if (_result.ProjectionExpressions.Count == 0)
      {
        string message = "There is no projection for the select clause.";
        throw new ParserException (message, _expressionTreeRoot, _expressionTreeRoot, null);
      }

      LambdaExpression selectProjection = _result.ProjectionExpressions.Last ();
      return new SelectClause (_previousClause, selectProjection, _result.IsDistinct);
    }
  }
}
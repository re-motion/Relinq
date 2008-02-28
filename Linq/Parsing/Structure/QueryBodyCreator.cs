using System;
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Data.Linq.Parsing.Structure;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.Parsing.Structure
{
  public class QueryBodyCreator
  {
    private readonly Expression _expressionTreeRoot;
    private readonly ParseResultCollector _result;
    private readonly List<IBodyClause> _bodyClauses = new List<IBodyClause> ();

    private IClause _previousClause;
    private int currentProjection;
    private OrderByClause _previousOrderByClause;

    public QueryBodyCreator (Expression expressionTreeRoot, MainFromClause mainFromClause, ParseResultCollector result)
    {
      ArgumentUtility.CheckNotNull ("expressionTreeRoot", expressionTreeRoot);
      ArgumentUtility.CheckNotNull ("mainFromClause", mainFromClause);
      ArgumentUtility.CheckNotNull ("result", result);

      _expressionTreeRoot = expressionTreeRoot;
      _previousClause = mainFromClause;
      _result = result;
    }

    public QueryBody GetQueryBody()
    {
      currentProjection = 0;
      _previousOrderByClause = null;

      foreach (BodyExpressionBase bodyExpression in _result.BodyExpressions)
      {
        IBodyClause clause = CreateBodyClause (bodyExpression);
        if (clause != _previousClause)
          _bodyClauses.Add (clause);

        _previousClause = clause;
      }

      SelectClause selectClause = new SelectClause (_previousClause, _result.ProjectionExpressions.Last (), _result.IsDistinct);
      QueryBody queryBody = new QueryBody (selectClause);

      foreach (IBodyClause bodyClause in _bodyClauses)
        queryBody.Add (bodyClause);

      return queryBody;
    }

    private IBodyClause CreateBodyClause (BodyExpressionBase expression)
    {
      AdditionalFromClause fromClause = CreateFromClause (expression);
      if (fromClause != null)
        return fromClause;

      WhereClause whereClause = CreateWhereClause(expression);
      if (whereClause != null)
        return whereClause;

      OrderByClause orderByClause = CreateOrderByClause(expression);
      if (orderByClause != null)
        return orderByClause;

      throw new NotSupportedException ("The FromLetWhereExpression type " + expression.GetType ().Name + " is not supported.");
    }

    private AdditionalFromClause CreateFromClause (BodyExpressionBase expression)
    {
      FromExpression fromExpression = expression as FromExpression;
      if (fromExpression == null)
        return null;

      AdditionalFromClause additionalFromClause = new AdditionalFromClause (_previousClause, fromExpression.Identifier,
          (LambdaExpression) fromExpression.Expression, _result.ProjectionExpressions[currentProjection]);
      ++currentProjection;
      return additionalFromClause;
    }

    private WhereClause CreateWhereClause (BodyExpressionBase expression)
    {
      WhereExpression whereExpression = expression as WhereExpression;
      if (whereExpression == null)
        return null;

      WhereClause whereClause = new WhereClause (_previousClause, whereExpression.Expression);
      return whereClause;
    }

    private OrderByClause CreateOrderByClause (BodyExpressionBase expression)
    {
      OrderExpression orderExpression = expression as OrderExpression;
      if (orderExpression == null)
        return null;

      OrderingClause orderingClause = new OrderingClause (_previousClause, orderExpression.Expression, orderExpression.OrderDirection);
      if (orderExpression.FirstOrderBy)
      {
        OrderByClause orderByClause = new OrderByClause (orderingClause);
        _previousOrderByClause = orderByClause;
        return orderByClause;
      }
      else
      {
        if (_previousOrderByClause == null)
          throw ParserUtility.CreateParserException ("OrderBy or OrderByDescending", orderExpression, "beginning of an OrderBy clause", _expressionTreeRoot);
        else
        {
          _previousOrderByClause.Add (orderingClause);
          return _previousOrderByClause;
        }
      }
    }
  }
}
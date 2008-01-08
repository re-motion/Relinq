using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using Rubicon.Collections;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.Parsing
{
  public class QueryParser
  {
    private readonly List<BodyExpressionBase> _bodyExpressions = new List<BodyExpressionBase> ();
    private readonly List<LambdaExpression> _projectionExpressions = new List<LambdaExpression> ();
    private readonly List<OrderExpression> _orderExpressions = new List<OrderExpression>();

    public QueryParser (Expression expressionTreeRoot)
    {
      ArgumentUtility.CheckNotNull ("expressionTreeRoot", expressionTreeRoot);
      SourceExpression = expressionTreeRoot;

      SourceExpressionParser sourceExpressionParser = new SourceExpressionParser (SourceExpression, expressionTreeRoot, true, null, "parsing query");
      _bodyExpressions.AddRange (sourceExpressionParser.BodyExpressions);
      _projectionExpressions.AddRange (sourceExpressionParser.ProjectionExpressions);
    }

    public Expression SourceExpression { get; private set; }

    public ReadOnlyCollection<BodyExpressionBase> FromLetWhereExpressions
    {
      get { return new ReadOnlyCollection<BodyExpressionBase> (_bodyExpressions); }
    }

    public ReadOnlyCollection<LambdaExpression> ProjectionExpressions
    {
      get { return new ReadOnlyCollection<LambdaExpression> (_projectionExpressions); }
    }

    public ReadOnlyCollection<OrderExpression> OrderExpressions
    {
      get { return new ReadOnlyCollection<OrderExpression> (_orderExpressions); }
    }

    public QueryExpression GetParsedQuery ()
    {
      MainFromClause mainFromClause = CreateMainFromClause();
      QueryBody queryBody = CreateQueryBody(mainFromClause);

      return new QueryExpression (mainFromClause, queryBody, SourceExpression);
    }

    private MainFromClause CreateMainFromClause ()
    {
      FromExpression mainFromExpression = (FromExpression) _bodyExpressions[0];
      return new MainFromClause (mainFromExpression.Identifier,
          (IQueryable) ((ConstantExpression) mainFromExpression.Expression).Value);
    }

    private QueryBody CreateQueryBody (MainFromClause mainFromClause)
    {
      List<IBodyClause> fromLetWhereClauses = new List<IBodyClause> ();
      IClause previousClause = mainFromClause;

      int currentProjection = 0;
      for (int currentFromLetWhere = 1; currentFromLetWhere < _bodyExpressions.Count; currentFromLetWhere++)
      {
        IBodyClause clause = CreateFromLetWhereClause (_bodyExpressions[currentFromLetWhere], previousClause, ref currentProjection);
        fromLetWhereClauses.Add (clause);
        previousClause = clause;
      }

      SelectClause selectClause = new SelectClause (previousClause, _projectionExpressions.Last ());
      QueryBody queryBody = new QueryBody (selectClause);

      foreach (IBodyClause fromLetWhereClause in fromLetWhereClauses)
        queryBody.Add (fromLetWhereClause);

      // TODO: translate OrderExpressions into queryBody.OrderBy clause

      return queryBody;
    }

    private IBodyClause CreateFromLetWhereClause (BodyExpressionBase expression, IClause previousClause, ref int currentProjection)
    {
      FromExpression fromExpression = expression as FromExpression;
      if (fromExpression != null)
      {
        AdditionalFromClause additionalFromClause = new AdditionalFromClause (previousClause, fromExpression.Identifier,
            (LambdaExpression) fromExpression.Expression, _projectionExpressions[currentProjection]);
        ++currentProjection;
        return additionalFromClause;
      }

      WhereExpression whereExpression = expression as WhereExpression;
      if (whereExpression != null)
      {
        WhereClause whereClause = new WhereClause (previousClause, whereExpression.Expression);
        return whereClause;
      }

      throw new NotSupportedException ("The FromLetWhereExpression type " + expression.GetType().Name + " is not supported.");
    }
  }
}
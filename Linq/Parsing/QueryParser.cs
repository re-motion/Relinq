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
      QueryBodyCreator bodyCreator = new QueryBodyCreator (SourceExpression, mainFromClause, _projectionExpressions, _bodyExpressions);
      return bodyCreator.GetQueryBody();
    }
  }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Rubicon.Data.DomainObjects.Linq.Clauses;
using Rubicon.Utilities;

namespace Rubicon.Data.DomainObjects.Linq.Parsing
{
  public class QueryParser
  {
    private readonly List<Expression> _fromExpressions = new List<Expression> ();
    private readonly List<ParameterExpression> _fromIdentifiers = new List<ParameterExpression> ();
    private readonly List<LambdaExpression> _whereExpressions = new List<LambdaExpression> ();
    private readonly List<LambdaExpression> _projectionExpressions = new List<LambdaExpression> ();

    public QueryParser (Expression expressionTreeRoot)
    {
      ArgumentUtility.CheckNotNull ("expressionTreeRoot", expressionTreeRoot);
      MethodCallExpression rootExpression = ParserUtility.GetTypedExpression<MethodCallExpression> (expressionTreeRoot,
          "expression tree root", expressionTreeRoot);

      SourceExpression = expressionTreeRoot;

      switch (ParserUtility.CheckMethodCallExpression (rootExpression, expressionTreeRoot, "Select", "Where","SelectMany"))
      {
        case "Select":
          SelectExpressionParser se = new SelectExpressionParser(rootExpression, expressionTreeRoot);
          _fromExpressions.AddRange (se.FromExpressions);
          _fromIdentifiers.AddRange (se.FromIdentifiers);
          _whereExpressions.AddRange (se.WhereExpressions);
          _projectionExpressions.AddRange (se.ProjectionExpressions);
          break;
        case "Where":
          WhereExpressionParser we = new WhereExpressionParser (rootExpression, expressionTreeRoot, true);
          _fromExpressions.AddRange (we.FromExpressions);
          _fromIdentifiers.AddRange (we.FromIdentifiers);
          _whereExpressions.AddRange (we.BoolExpressions);
          _projectionExpressions.AddRange (we.ProjectionExpressions);
          break;
        case "SelectMany":
          SelectManyExpressionParser sm = new SelectManyExpressionParser (rootExpression, expressionTreeRoot);
          _fromExpressions.AddRange (sm.FromExpressions);
          _fromIdentifiers.AddRange (sm.FromIdentifiers);
          _projectionExpressions.AddRange (sm.ProjectionExpressions);
          break;
      }
    }

    public Expression SourceExpression { get; private set; }

    public QueryExpression GetParsedQuery ()
    {
      MainFromClause fromClause = new MainFromClause (_fromIdentifiers[0], (IQueryable)((ConstantExpression)_fromExpressions[0]).Value);
      SelectClause selectClause = new SelectClause (_projectionExpressions.ToArray());
      QueryBody queryBody = new QueryBody (selectClause);

      for (int i = 1; i < _fromExpressions.Count; i++)
      {
        AdditionalFromClause additionalFromClause = new AdditionalFromClause (_fromIdentifiers[i], _fromExpressions[i]);
        queryBody.Add (additionalFromClause);
      }

      foreach (LambdaExpression whereExpression in _whereExpressions)
      {
        WhereClause whereClause = new WhereClause (whereExpression);
        queryBody.Add (whereClause);
      }
      return new QueryExpression (fromClause, queryBody);
    }
  }
}
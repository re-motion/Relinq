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
          SelectExpressionParser selectExpressionParser = new SelectExpressionParser(rootExpression, expressionTreeRoot);
          _fromExpressions.AddRange (selectExpressionParser.FromExpressions);
          _fromIdentifiers.AddRange (selectExpressionParser.FromIdentifiers);
          _whereExpressions.AddRange (selectExpressionParser.WhereExpressions);
          _projectionExpressions.AddRange (selectExpressionParser.ProjectionExpressions);
          break;
        case "Where":
          WhereExpressionParser whereExpressionParser = new WhereExpressionParser (rootExpression, expressionTreeRoot, true);
          _fromExpressions.AddRange (whereExpressionParser.FromExpressions);
          _fromIdentifiers.AddRange (whereExpressionParser.FromIdentifiers);
          _whereExpressions.AddRange (whereExpressionParser.BoolExpressions);
          _projectionExpressions.AddRange (whereExpressionParser.ProjectionExpressions);
          break;
        case "SelectMany":
          SelectManyExpressionParser selectManyExpressionParser = new SelectManyExpressionParser (rootExpression, expressionTreeRoot);
          _fromExpressions.AddRange (selectManyExpressionParser.FromExpressions);
          _fromIdentifiers.AddRange (selectManyExpressionParser.FromIdentifiers);
          _projectionExpressions.AddRange (selectManyExpressionParser.ProjectionExpressions);
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
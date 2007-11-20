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
    private List<Expression> _fromExpressions = new List<Expression> ();
    private List<ParameterExpression> _fromIdentifiers = new List<ParameterExpression> ();
    private List<LambdaExpression> _whereIdentifiers = new List<LambdaExpression> ();
    private List<LambdaExpression> _projectionExpressions = new List<LambdaExpression> ();

    public QueryParser (Expression expressionTreeRoot)
    {
      ArgumentUtility.CheckNotNull ("expressionTreeRoot", expressionTreeRoot);
      MethodCallExpression rootExpression = ParserUtility.GetTypedExpression<MethodCallExpression> (expressionTreeRoot,
          "expression tree root", expressionTreeRoot);

      SourceExpression = expressionTreeRoot;

      switch (ParserUtility.CheckMethodCallExpression (rootExpression, expressionTreeRoot, "Select"))
      {
        case "Select":
          SelectExpressionParser se = new SelectExpressionParser(rootExpression, expressionTreeRoot);
          _fromExpressions.AddRange (se.FromExpressions);
          _fromIdentifiers.AddRange (se.FromIdentifiers);
          _whereIdentifiers.AddRange (se.WhereExpressions);
          _projectionExpressions.AddRange (se.ProjectionExpressions);
          break;
      }
    }

    public Expression SourceExpression { get; private set; }

    public QueryExpression GetParsedQuery ()
    {
      FromClause fromClause = new FromClause (_fromIdentifiers[0], (IQueryable)((ConstantExpression)_fromExpressions[0]).Value);
      SelectClause selectClause = new SelectClause (_projectionExpressions.ToArray());
      QueryBody queryBody = new QueryBody (selectClause);
      return new QueryExpression (fromClause, queryBody);
    }
  }
}
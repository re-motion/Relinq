using System;
using System.Linq.Expressions;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Data.Linq.Visitor;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.Parsing.Structure
{
  public class SubQueryFindingVisitor : ExpressionTreeVisitor
  {
    private readonly SourceExpressionParser _referenceParser = new SourceExpressionParser (true);

    public Expression ReplaceSubQuery (Expression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      return VisitExpression(expression);
    }

    protected override Expression VisitMethodCallExpression (MethodCallExpression expression)
    {
      if (_referenceParser.CallDispatcher.CanParse (expression.Method))
        return CreateSubQueryNode (expression);
      else
        return base.VisitMethodCallExpression (expression);
    }

    private SubQueryExpression CreateSubQueryNode (MethodCallExpression methodCallExpression)
    {
      QueryParser parser = new QueryParser (methodCallExpression);
      QueryModel queryModel = parser.GetParsedQuery ();
      return new SubQueryExpression (queryModel);
    }
  }
}
using System.Collections.Generic;
using System.Linq.Expressions;
using Remotion.Data.Linq.Visitor;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.Structure
{
  public class SubQueryFindingVisitor : ExpressionTreeVisitor
  {
    private readonly List<QueryModel> _subQueryRegistry;
    private readonly SourceExpressionParser _referenceParser = new SourceExpressionParser (true);

    public SubQueryFindingVisitor (List<QueryModel> subQueryRegistry)
    {
      _subQueryRegistry = subQueryRegistry;
    }

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
      _subQueryRegistry.Add (queryModel);
      return new SubQueryExpression (queryModel);
    }
  }
}
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.Details.SelectProjectionParsing
{
  public class MethodCallExpressionParser : ISelectProjectionParser
  {
    private readonly SelectProjectionParserRegistry _parserRegistry;

    public MethodCallExpressionParser (SelectProjectionParserRegistry parserRegistry)
    {
      ArgumentUtility.CheckNotNull ("parserRegistry", parserRegistry);
      _parserRegistry = parserRegistry;
    }

    public IEvaluation Parse (MethodCallExpression methodCallExpression, ParseContext parseContext)
    {
      ArgumentUtility.CheckNotNull ("methodCallExpression", methodCallExpression);
      ArgumentUtility.CheckNotNull ("parseContext", parseContext);
      MethodInfo methodInfo = methodCallExpression.Method;
      IEvaluation evaluationObject;
      if (methodCallExpression.Object == null)
        evaluationObject = null;
      else
        evaluationObject = _parserRegistry.GetParser (methodCallExpression.Object).Parse (methodCallExpression.Object, parseContext);

      List<IEvaluation> evaluationArguments = new List<IEvaluation> ();
      foreach (Expression exp in methodCallExpression.Arguments)
      {
        evaluationArguments.Add (_parserRegistry.GetParser (exp).Parse (exp, parseContext));
      }
      return new MethodCall (methodInfo, evaluationObject, evaluationArguments);
    }

    IEvaluation ISelectProjectionParser.Parse (Expression expression, ParseContext parseContext)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      ArgumentUtility.CheckNotNull ("parseContext", parseContext);
      return Parse ((MethodCallExpression) expression, parseContext);
    }

    public bool CanParse(Expression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      return expression is MethodCallExpression;
    }
  }
}
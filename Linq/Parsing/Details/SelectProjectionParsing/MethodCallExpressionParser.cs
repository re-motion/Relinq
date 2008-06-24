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

    public List<IEvaluation> Parse (MethodCallExpression methodCallExpression, List<FieldDescriptor> fieldDescriptorCollection)
    {
      MethodInfo methodInfo = methodCallExpression.Method;
      IEvaluation evaluationObject;
      if (methodCallExpression.Object == null)
        evaluationObject = null;
      else 
        evaluationObject = _parserRegistry.GetParser (methodCallExpression.Object).Parse (methodCallExpression.Object, fieldDescriptorCollection)[0];

      List<IEvaluation> evaluationArguments = new List<IEvaluation> ();
      foreach (Expression exp in methodCallExpression.Arguments)
      {
        evaluationArguments.AddRange (_parserRegistry.GetParser (exp).Parse (exp, fieldDescriptorCollection));
      }
      return new List<IEvaluation> {new MethodCallEvaluation (methodInfo, evaluationObject, evaluationArguments)};
    }

    List<IEvaluation> ISelectProjectionParser.Parse (Expression expression, List<FieldDescriptor> fieldDescriptors)
    {
      return Parse ((MethodCallExpression) expression, fieldDescriptors);
    }

    public bool CanParse(Expression expression)
    {
      return expression is MethodCallExpression;
    }
  }
}
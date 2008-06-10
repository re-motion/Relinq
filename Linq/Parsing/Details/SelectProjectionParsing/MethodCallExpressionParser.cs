using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.Details.SelectProjectionParsing
{
  public class MethodCallExpressionParser : ISelectProjectionParser<MethodCallExpression>, ISelectProjectionParser
  {
    private readonly QueryModel _queryModel;
    private readonly ParserRegistry _parserRegistry;

    public MethodCallExpressionParser (QueryModel queryModel, ParserRegistry parserRegistry)
    {
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);
      ArgumentUtility.CheckNotNull ("parserRegistry", parserRegistry);

      _queryModel = queryModel;
      _parserRegistry = parserRegistry;
    }

    public List<IEvaluation> Parse (MethodCallExpression methodCallExpression, List<FieldDescriptor> fieldDescriptorCollection)
    {
      MethodInfo methodInfo = methodCallExpression.Method;
      List<IEvaluation> evaluationObject = 
        GetParser (methodCallExpression.Object).Parse (methodCallExpression.Object, fieldDescriptorCollection);

      List<IEvaluation> evaluationArguments = new List<IEvaluation> ();
      foreach (Expression exp in methodCallExpression.Arguments)
      {
        evaluationArguments.AddRange (GetParser (exp).Parse (exp, fieldDescriptorCollection));
      }
      return new List<IEvaluation> {new MethodCallEvaluation (methodInfo, evaluationObject[0], evaluationArguments)};
    }

    public bool CanParse(MethodCallExpression methodCallExpression)
    {
      throw new System.NotImplementedException();
    }

    public List<IEvaluation> Parse(Expression expression, List<FieldDescriptor> fieldDescriptors)
    {
      return Parse ((MethodCallExpression) expression, fieldDescriptors);
    }

    private ISelectProjectionParser GetParser (Expression expression)
    {
      if (expression.GetType () == typeof (ConstantExpression))
        return (ISelectProjectionParser) _parserRegistry.GetParser ((ConstantExpression) expression);
      else if (expression.GetType () == typeof (BinaryExpression))
        return (ISelectProjectionParser) _parserRegistry.GetParser ((BinaryExpression) expression);
      else if (expression.GetType () == typeof (MemberExpression))
        return (ISelectProjectionParser) _parserRegistry.GetParser ((MemberExpression) expression);
      else if (expression.GetType () == typeof (MethodCallExpression))
        return (ISelectProjectionParser) _parserRegistry.GetParser ((MethodCallExpression)expression);
      else if (expression.GetType () == typeof (ParameterExpression))
        return (ISelectProjectionParser) _parserRegistry.GetParser ((ParameterExpression) expression);
      else if (expression.GetType () == typeof (NewExpression))
        return (ISelectProjectionParser) _parserRegistry.GetParser ((NewExpression) expression);
      throw ParserUtility.CreateParserException ("no parser for expression found", expression, "GetParser",
        _queryModel.GetExpressionTree ());
    }
  }
}
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.Parsing.Details.SelectProjectionParsing
{
  public class MethodCallExpressionParser
  {
    private readonly QueryModel _queryModel;
    private readonly Func<Expression, IEvaluation> _parsingCall;

    public MethodCallExpressionParser (QueryModel queryModel, Func<Expression, IEvaluation> parsingCall)
    {
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);
      ArgumentUtility.CheckNotNull ("parsingCall", parsingCall);

      _queryModel = queryModel;
      _parsingCall = parsingCall;
    }

    public MethodCallEvaluation Parse (MethodCallExpression methodCallExpression, List<FieldDescriptor> fieldDescriptorCollection)
    {
      MethodInfo methodInfo = methodCallExpression.Method;
      IEvaluation evaluationObject = _parsingCall (methodCallExpression.Object);
      List<IEvaluation> evaluationArguments = new List<IEvaluation> ();
      foreach (Expression expression in methodCallExpression.Arguments)
      {
        evaluationArguments.Add ( _parsingCall (expression));
      }
      return new MethodCallEvaluation (methodInfo, evaluationObject, evaluationArguments);
    }
  }
}
using System;
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Data.Linq.Parsing.TreeEvaluation;

namespace Remotion.Data.Linq.Parsing.Structure
{
  public class SimpleFromSourceExpressionParser
  {
    public void Parse (ParseResultCollector resultCollector, Expression sourceExpression, ParameterExpression potentialFromIdentifier, string context)
    {
      if (potentialFromIdentifier == null)
      {
        string message = string.Format (
            "Parsing of expression '{0}' is not supported. The expression was interpreted as a from source, but there is no from identifier matching "
            + "it in expression tree '{1}'.",
            sourceExpression,
            resultCollector.ExpressionTreeRoot);
        throw new ParserException (message, sourceExpression, resultCollector.ExpressionTreeRoot, null);
      }

      switch (sourceExpression.NodeType)
      {
        case ExpressionType.Constant:
          ParseConstantExpressionAsSimpleSource (resultCollector, (ConstantExpression) sourceExpression, potentialFromIdentifier);
          break;
        case ExpressionType.MemberAccess:
          ParseSimpleSource (resultCollector, sourceExpression, potentialFromIdentifier);
          break;
        case ExpressionType.Call:
          EvaluateExpressionAsSimpleSource (resultCollector, (MethodCallExpression) sourceExpression, potentialFromIdentifier);
          break;
        default:
          throw ParserUtility.CreateParserException (
              "Constant, MemberAccess, or Call expression",
              sourceExpression,
              context,
              resultCollector.ExpressionTreeRoot);
      }
    }

    private void EvaluateExpressionAsSimpleSource (ParseResultCollector resultCollector, Expression expression, ParameterExpression potentialFromIdentifier)
    {
      ConstantExpression evaluatedExpression;
      try
      {
        evaluatedExpression = PartialTreeEvaluator.EvaluateSubtree (expression);
      }
      catch (TargetInvocationException targetInvocationException)
      {
        string message = string.Format ("The expression '{0}' could not be evaluated as a query source because it threw an exception: {1}", 
            expression, targetInvocationException.InnerException.Message);
        throw new ParserException (message, targetInvocationException);
      }
      catch (Exception ex)
      {
        string message = string.Format ("The expression '{0}' could not be evaluated as a query source because it cannot be compiled: {1}",
            expression, ex.Message);
        throw new ParserException (message, ex);
      }
      ParseConstantExpressionAsSimpleSource (resultCollector, evaluatedExpression, potentialFromIdentifier);
    }

    private void ParseConstantExpressionAsSimpleSource (ParseResultCollector resultCollector, ConstantExpression constantExpression, ParameterExpression potentialFromIdentifier)
    {
      if (constantExpression.Value == null)
        throw new ParserException ("Query sources cannot be null.");
      ParseSimpleSource (resultCollector, constantExpression, potentialFromIdentifier);
    }

    private void ParseSimpleSource (ParseResultCollector resultCollector, Expression sourceExpression, ParameterExpression potentialFromIdentifier)
    {
      resultCollector.AddBodyExpression (new FromExpressionData (sourceExpression, potentialFromIdentifier));
    }
  }
}
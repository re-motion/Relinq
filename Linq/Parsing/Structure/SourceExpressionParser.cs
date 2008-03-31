using System;
using System.Linq.Expressions;
using System.Reflection;
using Rubicon.Data.Linq.Parsing.Structure;
using Rubicon.Data.Linq.Parsing.TreeEvaluation;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.Parsing.Structure
{
  public class SourceExpressionParser
  {
    private readonly bool _isTopLevel;
    
    public SourceExpressionParser (bool isTopLevel)
    {
      _isTopLevel = isTopLevel;
    }

    public void Parse (ParseResultCollector resultCollector, Expression sourceExpression, ParameterExpression potentialFromIdentifier, string context)
    {
      ArgumentUtility.CheckNotNull ("resultCollector", resultCollector);
      ArgumentUtility.CheckNotNull ("sourceExpression", sourceExpression);
      ArgumentUtility.CheckNotNull ("context", context);

      switch (sourceExpression.NodeType)
      {
        case ExpressionType.Constant:
          ParseConstantExpressionAsSimpleSource (resultCollector, (ConstantExpression) sourceExpression, potentialFromIdentifier);
          break;
        case ExpressionType.MemberAccess:
          ParseSimpleSource (resultCollector, sourceExpression, potentialFromIdentifier);
          break;
        case ExpressionType.Call:
          var methodCallExpression = (MethodCallExpression) sourceExpression;
          string methodName = methodCallExpression.Method.Name;
          switch (methodName)
          {
            case "Select":
              ParseSelectSource (resultCollector, sourceExpression);
              break;
            case "SelectMany":
              ParseSelectManySource (resultCollector, sourceExpression);
              break;
            case "Where":
              ParseWhereSource (resultCollector, sourceExpression);
              break;
            case "OrderBy":
            case "OrderByDescending":
            case "ThenBy":
            case "ThenByDescending":
              ParseOrderBy (resultCollector, sourceExpression);
              break;
            case "Distinct":
              ParseDistinct (resultCollector, sourceExpression, potentialFromIdentifier);
              break;
            default:
              EvaluateExpressionAsSimpleSource (resultCollector, methodCallExpression, potentialFromIdentifier);
              break;
          }
          break;
        default:
          throw ParserUtility.CreateParserException ("Constant or Call expression", sourceExpression, context,
              resultCollector.ExpressionTreeRoot);
      }
    }

    private void EvaluateExpressionAsSimpleSource (ParseResultCollector resultCollector, Expression expression, ParameterExpression potentialFromIdentifier)
    {
      try
      {
        ConstantExpression evaluatedExpression = PartialTreeEvaluator.EvaluateSubtree (expression);
        ParseConstantExpressionAsSimpleSource(resultCollector, evaluatedExpression, potentialFromIdentifier);
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

    private void ParseSelectSource (ParseResultCollector resultCollector, Expression sourceExpression)
    {
      MethodCallExpression methodCallExpression = (MethodCallExpression) sourceExpression;
      new SelectExpressionParser().Parse (resultCollector, methodCallExpression);
    }

    private void ParseSelectManySource (ParseResultCollector resultCollector, Expression sourceExpression)
    {
      MethodCallExpression methodCallExpression = (MethodCallExpression) sourceExpression;
      new SelectManyExpressionParser().Parse (resultCollector, methodCallExpression);
    }

    private void ParseWhereSource (ParseResultCollector resultCollector, Expression sourceExpression)
    {
      MethodCallExpression methodCallExpression = (MethodCallExpression) sourceExpression;
      new WhereExpressionParser (_isTopLevel).Parse (resultCollector, methodCallExpression);
    }

    private void ParseOrderBy (ParseResultCollector resultCollector, Expression sourceExpression)
    {
      MethodCallExpression methodCallExpression = (MethodCallExpression) sourceExpression;
      new OrderByExpressionParser (_isTopLevel).Parse ( resultCollector, methodCallExpression);
    }

    private void ParseDistinct (ParseResultCollector resultCollector, Expression sourceExpression, ParameterExpression potentialFromIdentifier)
    {
      if (!_isTopLevel)
      {
        string message = string.Format ("Distinct is only allowed at the top level of a query, not in the middle: '{0}'.", resultCollector.ExpressionTreeRoot);
        throw new ParserException (message, sourceExpression, resultCollector.ExpressionTreeRoot, null);
      }

      resultCollector.SetDistinct ();
      Expression selectExpression = ((MethodCallExpression) sourceExpression).Arguments[0];
      Parse (resultCollector, selectExpression, potentialFromIdentifier, "first argument of distinct");
    }
  }
}
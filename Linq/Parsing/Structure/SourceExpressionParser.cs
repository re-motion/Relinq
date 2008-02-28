using System;
using System.Linq.Expressions;
using Rubicon.Data.Linq.Parsing.Structure;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.Parsing.Structure
{
  internal class SourceExpressionParser
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
          ParseSimpleSource (resultCollector, sourceExpression, potentialFromIdentifier);
          break;
        case ExpressionType.Call:
          string methodName = ParserUtility.CheckMethodCallExpression (
              (MethodCallExpression) sourceExpression, resultCollector.ExpressionTreeRoot, 
              "Select", "SelectMany", "Where", "OrderBy", "OrderByDescending", "ThenBy", "ThenByDescending", "Distinct");
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
              ParseDistinct (resultCollector, sourceExpression);
              break;
          }
          break;
        default:
          throw ParserUtility.CreateParserException ("Constant or Call expression", sourceExpression, context,
              resultCollector.ExpressionTreeRoot);
      }
    }

    private void ParseSimpleSource (ParseResultCollector resultCollector, Expression sourceExpression, ParameterExpression potentialFromIdentifier)
    {
      ConstantExpression constantExpression = (ConstantExpression) sourceExpression;
      resultCollector.AddBodyExpression (new FromExpression (constantExpression, potentialFromIdentifier));
    }

    private void ParseSelectSource (ParseResultCollector resultCollector, Expression sourceExpression)
    {
      MethodCallExpression methodCallExpression = (MethodCallExpression) sourceExpression;
      new SelectExpressionParser().Parse (resultCollector, methodCallExpression);
    }

    private void ParseDistinct (ParseResultCollector resultCollector, Expression sourceExpression) //only supports distinct in select (query.Distinct())
    {
      resultCollector.SetDistinct();
      Expression selectExpression = ((MethodCallExpression) sourceExpression).Arguments[0];
      ParseSelectSource (resultCollector, selectExpression);
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
      new OrderByExpressionParser (resultCollector, methodCallExpression, _isTopLevel);
    }
  }
}
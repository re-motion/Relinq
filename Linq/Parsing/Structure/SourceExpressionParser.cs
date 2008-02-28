using System;
using System.Linq.Expressions;
using Rubicon.Data.Linq.Parsing.Structure;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.Parsing.Structure
{
  internal class SourceExpressionParser
  {
    private readonly ParseResultCollector _resultCollector;
    private readonly bool _isTopLevel;
    private readonly ParameterExpression _potentialFromIdentifier;

    public SourceExpressionParser (ParseResultCollector resultCollector, Expression sourceExpression, bool isTopLevel,
                                   ParameterExpression potentialFromIdentifier, string context)
    {
      ArgumentUtility.CheckNotNull ("sourceExpression", sourceExpression);
      ArgumentUtility.CheckNotNull ("resultCollector", resultCollector);

      _resultCollector = resultCollector;
      _isTopLevel = isTopLevel;
      _potentialFromIdentifier = potentialFromIdentifier;

      SourceExpression = sourceExpression;

      switch (sourceExpression.NodeType)
      {
        case ExpressionType.Constant:
          ParseSimpleSource ();
          break;
        case ExpressionType.Call:
          string methodName = ParserUtility.CheckMethodCallExpression (
              (MethodCallExpression) sourceExpression, _resultCollector.ExpressionTreeRoot, 
              "Select", "SelectMany", "Where", "OrderBy", "OrderByDescending", "ThenBy", "ThenByDescending", "Distinct");
          switch (methodName)
          {
            case "Select":
              ParseSelectSource ();
              break;
            case "SelectMany":
              ParseSelectManySource ();
              break;
            case "Where":
              ParseWhereSource ();
              break;
            case "OrderBy":
            case "OrderByDescending":
            case "ThenBy":
            case "ThenByDescending":
              ParseOrderBy ();
              break;
            case "Distinct":
              ParseDistinct ();
              break;
          }
          break;
        default:
          throw ParserUtility.CreateParserException ("Constant or Call expression", sourceExpression, context,
              _resultCollector.ExpressionTreeRoot);
      }
    }

    public Expression SourceExpression { get; private set; }

    private void ParseSimpleSource ()
    {
      ConstantExpression constantExpression = (ConstantExpression) SourceExpression;
      _resultCollector.AddBodyExpression (new FromExpression (constantExpression, _potentialFromIdentifier));
    }

    private void ParseSelectSource ()
    {
      MethodCallExpression methodCallExpression = (MethodCallExpression) SourceExpression;
      new SelectExpressionParser (_resultCollector, methodCallExpression);
    }

    private void ParseDistinct () //only supports distinct in select (query.Distinct())
    {
      _resultCollector.SetDistinct();
      Expression newTreeRoot = ((MethodCallExpression) _resultCollector.ExpressionTreeRoot).Arguments[0];
      SourceExpression = newTreeRoot;
      ParseSelectSource ();
    }

    private void ParseSelectManySource ()
    {
      MethodCallExpression methodCallExpression = (MethodCallExpression) SourceExpression;
      new SelectManyExpressionParser (_resultCollector, methodCallExpression);
    }

    private void ParseWhereSource ()
    {
      MethodCallExpression methodCallExpression = (MethodCallExpression) SourceExpression;
      new WhereExpressionParser (_resultCollector, methodCallExpression, _isTopLevel);
    }

    private void ParseOrderBy ()
    {
      MethodCallExpression methodCallExpression = (MethodCallExpression) SourceExpression;
      new OrderByExpressionParser (_resultCollector, methodCallExpression, _isTopLevel);
    }
  }
}
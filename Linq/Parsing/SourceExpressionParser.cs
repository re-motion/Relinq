using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using Rubicon.Text;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.Parsing
{
  internal class SourceExpressionParser
  {
    private readonly List<FromLetWhereExpressionBase> _fromLetWhereExpressions = new List<FromLetWhereExpressionBase> ();
    private readonly List<LambdaExpression> _projectionExpressions = new List<LambdaExpression> ();
    private readonly bool _isTopLevel;
    private readonly ParameterExpression _potentialFromIdentifier;

    public SourceExpressionParser (Expression sourceExpression, Expression expressionTreeRoot, bool isTopLevel,
        ParameterExpression potentialFromIdentifier, string context)
    {
      ArgumentUtility.CheckNotNull ("sourceExpression", sourceExpression);
      ArgumentUtility.CheckNotNull ("expressionTreeRoot", expressionTreeRoot);

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
            (MethodCallExpression) sourceExpression, expressionTreeRoot, "Select", "SelectMany", "Where");
          switch (methodName)
          {
            case "Select":
              ParseSelectSource (expressionTreeRoot);
              break;
            case "SelectMany":
              ParseSelectManySource (expressionTreeRoot);
              break;
            case "Where":
              ParseWhereSource (expressionTreeRoot);
              break;
          }
          break;
        default:
          throw ParserUtility.CreateParserException ("Constant or Call expression", sourceExpression, context,
              expressionTreeRoot);
      }
    }

    private void ParseSimpleSource ()
    {
      ConstantExpression constantExpression = (ConstantExpression) SourceExpression;

      _fromLetWhereExpressions.Add (new FromExpression (constantExpression, _potentialFromIdentifier));
     
    }

    private void ParseSelectSource (Expression expressionTreeRoot)
    {
      MethodCallExpression methodCallExpression = (MethodCallExpression) SourceExpression;
      SelectExpressionParser selectExpressionParser = new SelectExpressionParser (methodCallExpression, expressionTreeRoot);

      _fromLetWhereExpressions.AddRange (selectExpressionParser.FromLetWhereExpressions);
      _projectionExpressions.AddRange (selectExpressionParser.ProjectionExpressions);
    }

    private void ParseSelectManySource (Expression expressionTreeRoot)
    {
      MethodCallExpression methodCallExpression = (MethodCallExpression) SourceExpression;
      SelectManyExpressionParser selectManyExpressionParser = new SelectManyExpressionParser (methodCallExpression, expressionTreeRoot);

      _fromLetWhereExpressions.AddRange (selectManyExpressionParser.FromLetWhereExpressions);
      _projectionExpressions.AddRange (selectManyExpressionParser.ProjectionExpressions);
    }

    private void ParseWhereSource (Expression expressionTreeRoot)
    {
      MethodCallExpression methodCallExpression = (MethodCallExpression) SourceExpression;
      WhereExpressionParser whereExpressionParser = new WhereExpressionParser (methodCallExpression, expressionTreeRoot, _isTopLevel);

      _fromLetWhereExpressions.AddRange (whereExpressionParser.FromLetWhereExpressions);
      _projectionExpressions.AddRange (whereExpressionParser.ProjectionExpressions);
    }

    public Expression SourceExpression { get; private set; }

    public ReadOnlyCollection<FromLetWhereExpressionBase> FromLetWhereExpressions
    {
      get { return new ReadOnlyCollection<FromLetWhereExpressionBase> (_fromLetWhereExpressions); }
    }

    public ReadOnlyCollection<LambdaExpression> ProjectionExpressions
    {
      get { return new ReadOnlyCollection<LambdaExpression> (_projectionExpressions); }
    }
  }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.Parsing
{
  public class WhereExpressionParser
  {
    private readonly bool _isTopLevel;
    private readonly List<FromLetWhereExpressionBase> _fromLetWhereExpressions = new List<FromLetWhereExpressionBase> ();
    private readonly List<LambdaExpression> _projectionExpressions = new List<LambdaExpression>();

    public WhereExpressionParser (MethodCallExpression whereExpression, Expression expressionTreeRoot, bool isTopLevel)
    {
      ArgumentUtility.CheckNotNull ("whereExpression", whereExpression);
      ArgumentUtility.CheckNotNull ("expressionTreeRoot", expressionTreeRoot);

      _isTopLevel = isTopLevel;

      ParserUtility.CheckMethodCallExpression (whereExpression, expressionTreeRoot, "Where");

      SourceExpression = whereExpression;

      switch (whereExpression.Arguments[0].NodeType)
      {
        case ExpressionType.Constant:
          ParseSimpleWhere (expressionTreeRoot);
          break;
        case ExpressionType.Call:
          string methodName =
              ParserUtility.CheckMethodCallExpression ((MethodCallExpression) whereExpression.Arguments[0], expressionTreeRoot, "Where", "SelectMany");
          switch (methodName)
          {
            case "Where":
              ParseRecursiveWhere (expressionTreeRoot);
              break;
            case "SelectMany":
              ParseSelectManyWhere (expressionTreeRoot);
              break;
          }
          break;
        default:
          throw ParserUtility.CreateParserException ("Constant or Call expression", whereExpression, "first argument of Where expression",
              expressionTreeRoot);
      }
    }

    private void ParseSelectManyWhere (Expression expressionTreeRoot)
    {
      MethodCallExpression methodCallExpression = ParserUtility.GetTypedExpression<MethodCallExpression> (SourceExpression.Arguments[0],
          "first argument of Where expression", expressionTreeRoot);
      SelectManyExpressionParser selectManyExpressionParser = new SelectManyExpressionParser (methodCallExpression, expressionTreeRoot);

      UnaryExpression unaryExpression = ParserUtility.GetTypedExpression<UnaryExpression> (SourceExpression.Arguments[1],
          "second argument of Where expression", expressionTreeRoot);

      LambdaExpression ueLambda = ParserUtility.GetTypedExpression<LambdaExpression> (unaryExpression.Operand,
          "second argument of Where expression", expressionTreeRoot);

      _fromLetWhereExpressions.AddRange (selectManyExpressionParser.FromLetWhereExpressions);
      _fromLetWhereExpressions.Add (new WhereExpression (ueLambda));
      _projectionExpressions.AddRange (selectManyExpressionParser.ProjectionExpressions);
    }


    private void ParseSimpleWhere (Expression expressionTreeRoot)
    {
      ConstantExpression constantExpression = ParserUtility.GetTypedExpression<ConstantExpression> (SourceExpression.Arguments[0],
          "first argument of Where expression", expressionTreeRoot);
      UnaryExpression unaryExpression = ParserUtility.GetTypedExpression<UnaryExpression> (SourceExpression.Arguments[1],
          "second argument of Where expression", expressionTreeRoot);
      LambdaExpression ueLambda = ParserUtility.GetTypedExpression<LambdaExpression> (unaryExpression.Operand,
          "second argument of Where expression", expressionTreeRoot);

      _fromLetWhereExpressions.Add (new FromExpression (constantExpression, ueLambda.Parameters[0]));
      _fromLetWhereExpressions.Add (new WhereExpression (ueLambda));
      if (_isTopLevel)
        _projectionExpressions.Add (null);
    }

    private void ParseRecursiveWhere (Expression expressionTreeRoot)
    {
      MethodCallExpression methodCallExpression = ParserUtility.GetTypedExpression<MethodCallExpression> (SourceExpression.Arguments[0],
          "first argument of Where expression", expressionTreeRoot);
      WhereExpressionParser whereExpressionParser = new WhereExpressionParser (methodCallExpression, expressionTreeRoot, _isTopLevel);

      UnaryExpression unaryExpression = ParserUtility.GetTypedExpression<UnaryExpression> (SourceExpression.Arguments[1],
          "second argument of Where expression", expressionTreeRoot);
      LambdaExpression ueLambda = ParserUtility.GetTypedExpression<LambdaExpression> (unaryExpression.Operand,
          "second argument of Where expression", expressionTreeRoot);

      _fromLetWhereExpressions.AddRange (whereExpressionParser.FromLetWhereExpressions);
      _fromLetWhereExpressions.Add (new WhereExpression (ueLambda));
      _projectionExpressions.AddRange (whereExpressionParser.ProjectionExpressions);
    }

    public MethodCallExpression SourceExpression
    {
      get;
      private set;
    }

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
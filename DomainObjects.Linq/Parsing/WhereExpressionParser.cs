using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using Rubicon.Utilities;

namespace Rubicon.Data.DomainObjects.Linq.Parsing
{
  public class WhereExpressionParser
  {
    private readonly bool _isTopLevel;
    private readonly List<Expression> _fromExpressions = new List<Expression>();
    private readonly List<ParameterExpression> _fromIdentifiers = new List<ParameterExpression>();
    private readonly List<LambdaExpression> _boolExpressions = new List<LambdaExpression>();
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
          switch (
              ParserUtility.CheckMethodCallExpression ((MethodCallExpression) whereExpression.Arguments[0], expressionTreeRoot, "Where", "SelectMany")
              )
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

      _fromExpressions.AddRange (selectManyExpressionParser.FromExpressions);
      _fromIdentifiers.AddRange (selectManyExpressionParser.FromIdentifiers);
      _boolExpressions.Add (ueLambda);
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

      _fromExpressions.Add (constantExpression);
      _fromIdentifiers.Add (ueLambda.Parameters[0]);
      _boolExpressions.Add (ueLambda);
      if (_isTopLevel)
        _projectionExpressions.Add (Expression.Lambda (ueLambda.Parameters[0]));
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

      _fromExpressions.AddRange (whereExpressionParser.FromExpressions);
      _fromIdentifiers.AddRange (whereExpressionParser.FromIdentifiers);
      _boolExpressions.AddRange (whereExpressionParser.BoolExpressions);
      _boolExpressions.Add (ueLambda);
      _projectionExpressions.AddRange (whereExpressionParser.ProjectionExpressions);
    }

    public MethodCallExpression SourceExpression
    {
      get;
      private set;
    }

    public ReadOnlyCollection<Expression> FromExpressions
    {
      get { return new ReadOnlyCollection<Expression> (_fromExpressions); }
    }

    public ReadOnlyCollection<ParameterExpression> FromIdentifiers
    {
      get { return new ReadOnlyCollection<ParameterExpression> (_fromIdentifiers); }
    }

    public ReadOnlyCollection<LambdaExpression> BoolExpressions
    {
      get { return new ReadOnlyCollection<LambdaExpression> (_boolExpressions); }
    }

    public ReadOnlyCollection<LambdaExpression> ProjectionExpressions
    {
      get { return new ReadOnlyCollection<LambdaExpression> (_projectionExpressions); }
    }
  }
}
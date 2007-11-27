using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using Rubicon.Text;
using Rubicon.Utilities;

namespace Rubicon.Data.DomainObjects.Linq.Parsing
{
  public class SelectManyExpressionParser
  {
    private readonly List<FromLetWhereExpressionBase> _fromLetWhereExpressions = new List<FromLetWhereExpressionBase> ();
    private readonly List<LambdaExpression> _projectionExpressions = new List<LambdaExpression> ();

    public SelectManyExpressionParser (MethodCallExpression selectManyExpression, Expression expressionTreeRoot)
    {
      ArgumentUtility.CheckNotNull ("selectManyExpression", selectManyExpression);
      ArgumentUtility.CheckNotNull ("expressionTreeRoot", expressionTreeRoot);

      ParserUtility.CheckMethodCallExpression (selectManyExpression, expressionTreeRoot, "SelectMany");

      SourceExpression = selectManyExpression;

      switch (selectManyExpression.Arguments[0].NodeType)
      {
        case ExpressionType.Constant:
          ParseSimpleSelectMany (expressionTreeRoot);
          break;
        case ExpressionType.Call:
          string methodName = ParserUtility.CheckMethodCallExpression (
            (MethodCallExpression) selectManyExpression.Arguments[0], expressionTreeRoot, "SelectMany","Where");
          switch (methodName)
          {
            case "SelectMany":
              ParseRecursiveSelectMany (expressionTreeRoot);
              break;
            case "Where":
              ParseWhereSelectMany (expressionTreeRoot);
              break;
          }

          
          break;
        default:
          throw ParserUtility.CreateParserException ("Constant or Call expression", selectManyExpression, "first argument of SelectMany expression",
              expressionTreeRoot);
      }
    }

    private void ParseWhereSelectMany (Expression expressionTreeRoot)
    {
      MethodCallExpression methodCallExpression = ParserUtility.GetTypedExpression<MethodCallExpression> (SourceExpression.Arguments[0],
          "first argument of SelectMany expression", expressionTreeRoot);
      WhereExpressionParser whereExpressionParser = new WhereExpressionParser (methodCallExpression, expressionTreeRoot,false);
      UnaryExpression unaryExpression1 = ParserUtility.GetTypedExpression<UnaryExpression> (SourceExpression.Arguments[1],
          "second argument of SelectMany expression", expressionTreeRoot);
      UnaryExpression unaryExpression2 = ParserUtility.GetTypedExpression<UnaryExpression> (SourceExpression.Arguments[2],
          "third argument of SelectMany expression", expressionTreeRoot);
      LambdaExpression ueLambda1 = ParserUtility.GetTypedExpression<LambdaExpression> (unaryExpression1.Operand,
          "second argument of SelectMany expression", expressionTreeRoot);
      LambdaExpression ueLambda2 = ParserUtility.GetTypedExpression<LambdaExpression> (unaryExpression2.Operand,
                "second argument of SelectMany expression", expressionTreeRoot);

      _fromLetWhereExpressions.AddRange (whereExpressionParser.FromLetWhereExpressions);
      _fromLetWhereExpressions.Add (new FromExpression (ueLambda1, ueLambda2.Parameters[1]));
      _projectionExpressions.AddRange (whereExpressionParser.ProjectionExpressions);
      _projectionExpressions.Add (ueLambda2);
    }

    private void ParseRecursiveSelectMany (Expression expressionTreeRoot)
    {
      MethodCallExpression methodCallExpression = ParserUtility.GetTypedExpression<MethodCallExpression> (SourceExpression.Arguments[0],
          "first argument of SelectMany expression", expressionTreeRoot);
      SelectManyExpressionParser selectManyExpressionParser = new SelectManyExpressionParser (methodCallExpression,expressionTreeRoot);
      UnaryExpression unaryExpression1 = ParserUtility.GetTypedExpression<UnaryExpression> (SourceExpression.Arguments[1],
          "second argument of SelectMany expression", expressionTreeRoot);
      UnaryExpression unaryExpression2 = ParserUtility.GetTypedExpression<UnaryExpression> (SourceExpression.Arguments[2],
          "third argument of SelectMany expression", expressionTreeRoot);
      LambdaExpression ueLambda1 = ParserUtility.GetTypedExpression<LambdaExpression> (unaryExpression1.Operand,
          "second argument of SelectMany expression", expressionTreeRoot);
      LambdaExpression ueLambda2 = ParserUtility.GetTypedExpression<LambdaExpression> (unaryExpression2.Operand,
                "second argument of SelectMany expression", expressionTreeRoot);


      _fromLetWhereExpressions.AddRange (selectManyExpressionParser.FromLetWhereExpressions);
      _fromLetWhereExpressions.Add (new FromExpression (ueLambda1, ueLambda2.Parameters[1]));
      _projectionExpressions.AddRange (selectManyExpressionParser.ProjectionExpressions);
      _projectionExpressions.Add (ueLambda2);
    }

    private void ParseSimpleSelectMany (Expression expressionTreeRoot)
    {
      ConstantExpression constantExpression = ParserUtility.GetTypedExpression<ConstantExpression> (SourceExpression.Arguments[0],
          "first argument of SelectMany expression", expressionTreeRoot);
      UnaryExpression unaryExpression1 = ParserUtility.GetTypedExpression<UnaryExpression> (SourceExpression.Arguments[1],
          "second argument of SelectMany expression", expressionTreeRoot);
      UnaryExpression unaryExpression2 = ParserUtility.GetTypedExpression<UnaryExpression> (SourceExpression.Arguments[2],
          "second argument of SelectMany expression", expressionTreeRoot);
      LambdaExpression ueLambda1 = ParserUtility.GetTypedExpression<LambdaExpression> (unaryExpression1.Operand,
          "second argument of SelectMany expression", expressionTreeRoot);
      LambdaExpression ueLambda2 = ParserUtility.GetTypedExpression<LambdaExpression> (unaryExpression2.Operand,
          "second argument of SelectMany expression", expressionTreeRoot);

      _fromLetWhereExpressions.Add (new FromExpression (constantExpression, ueLambda2.Parameters[0]));
      _fromLetWhereExpressions.Add (new FromExpression (ueLambda1, ueLambda2.Parameters[1]));
      _projectionExpressions.Add (ueLambda2);
    }

    public MethodCallExpression SourceExpression { get; private set; }

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
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using Rubicon.Text;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.Parsing
{
  public class SelectExpressionParser
  {
    private readonly List<FromLetWhereExpressionBase> _fromLetWhereExpressions = new List<FromLetWhereExpressionBase> ();
    private readonly List<LambdaExpression> _projectionExpressions = new List<LambdaExpression> ();

    public SelectExpressionParser (MethodCallExpression selectExpression, Expression expressionTreeRoot)
    {
      ArgumentUtility.CheckNotNull ("selectExpression", selectExpression);
      ArgumentUtility.CheckNotNull ("expressionTreeRoot", expressionTreeRoot);

      ParserUtility.CheckMethodCallExpression (selectExpression, expressionTreeRoot, "Select");

      SourceExpression = selectExpression;

      switch (selectExpression.Arguments[0].NodeType)
      {
        case ExpressionType.Constant:
          ParseSimpleSelect (expressionTreeRoot);
          break;
        case ExpressionType.Call:
          switch (ParserUtility.CheckMethodCallExpression ((MethodCallExpression) selectExpression.Arguments[0], expressionTreeRoot, "Where"))
          {
            case "Where":
              ParseWhereSelect (expressionTreeRoot);
              break;
          }
          break;
        default:
          throw ParserUtility.CreateParserException ("Constant or Call expression", selectExpression, "first argument of Select expression",
              expressionTreeRoot);
      }
    }

    private void ParseWhereSelect (Expression expressionTreeRoot)
    {
      MethodCallExpression methodCallExpression = ParserUtility.GetTypedExpression<MethodCallExpression> (SourceExpression.Arguments[0],
          "first argument of Select expression", expressionTreeRoot);
      WhereExpressionParser whereExpressionParser = new WhereExpressionParser (methodCallExpression, expressionTreeRoot, false);
      UnaryExpression unaryExpression = ParserUtility.GetTypedExpression<UnaryExpression> (SourceExpression.Arguments[1],
          "second argument of Select expression", expressionTreeRoot);
      LambdaExpression ueLambda = ParserUtility.GetTypedExpression<LambdaExpression> (unaryExpression.Operand,
          "second argument of Select expression", expressionTreeRoot);

      _fromLetWhereExpressions.AddRange (whereExpressionParser.FromLetWhereExpressions);
      _projectionExpressions.AddRange (whereExpressionParser.ProjectionExpressions);
      _projectionExpressions.Add (ueLambda);
    }

    private void ParseSimpleSelect (Expression expressionTreeRoot)
    {
      ConstantExpression constantExpression = ParserUtility.GetTypedExpression<ConstantExpression> (SourceExpression.Arguments[0],
          "first argument of Select expression", expressionTreeRoot);
      UnaryExpression unaryExpression = ParserUtility.GetTypedExpression<UnaryExpression> (SourceExpression.Arguments[1],
          "second argument of Select expression", expressionTreeRoot);
      LambdaExpression ueLambda = ParserUtility.GetTypedExpression<LambdaExpression> (unaryExpression.Operand,
          "second argument of Select expression", expressionTreeRoot);

      _fromLetWhereExpressions.Add (new FromExpression (constantExpression, ueLambda.Parameters[0]));
      _projectionExpressions.Add (ueLambda);
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
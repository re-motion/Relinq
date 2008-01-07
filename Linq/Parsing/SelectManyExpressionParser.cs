using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.Parsing
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
      if (selectManyExpression.Arguments.Count != 3)
        throw ParserUtility.CreateParserException ("SelectMany call with three arguments", selectManyExpression, "SelectMany expressions",
            expressionTreeRoot);

      SourceExpression = selectManyExpression;

      ParseSelectMany (expressionTreeRoot);
    }

    private void ParseSelectMany (Expression expressionTreeRoot)
    {
      UnaryExpression unaryExpression1 = ParserUtility.GetTypedExpression<UnaryExpression> (SourceExpression.Arguments[1],
          "second argument of SelectMany expression", expressionTreeRoot);
      UnaryExpression unaryExpression2 = ParserUtility.GetTypedExpression<UnaryExpression> (SourceExpression.Arguments[2],
          "third argument of SelectMany expression", expressionTreeRoot);
      LambdaExpression ueLambda1 = ParserUtility.GetTypedExpression<LambdaExpression> (unaryExpression1.Operand,
          "second argument of SelectMany expression", expressionTreeRoot);
      LambdaExpression ueLambda2 = ParserUtility.GetTypedExpression<LambdaExpression> (unaryExpression2.Operand,
                "second argument of SelectMany expression", expressionTreeRoot);

      SourceExpressionParser sourceExpressionParser = new SourceExpressionParser (SourceExpression.Arguments[0], expressionTreeRoot, false,
          ueLambda2.Parameters[0], "first argument of SelectMany expression");

      _fromLetWhereExpressions.AddRange (sourceExpressionParser.FromLetWhereExpressions);
      _fromLetWhereExpressions.Add (new FromExpression (ueLambda1, ueLambda2.Parameters[1]));
      _projectionExpressions.AddRange (sourceExpressionParser.ProjectionExpressions);
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

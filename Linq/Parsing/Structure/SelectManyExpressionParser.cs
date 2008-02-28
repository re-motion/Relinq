using System;
using System.Linq.Expressions;
using Rubicon.Data.Linq.Parsing.Structure;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.Parsing.Structure
{
  public class SelectManyExpressionParser
  {
    private readonly ParseResultCollector _resultCollector;
    private readonly SourceExpressionParser _sourceParser = new SourceExpressionParser (false);

    public SelectManyExpressionParser (ParseResultCollector resultCollector, MethodCallExpression selectManyExpression)
    {
      ArgumentUtility.CheckNotNull ("selectManyExpression", selectManyExpression);
      ArgumentUtility.CheckNotNull ("resultCollector", resultCollector);

      _resultCollector = resultCollector;

      ParserUtility.CheckMethodCallExpression (selectManyExpression, _resultCollector.ExpressionTreeRoot, "SelectMany");
      if (selectManyExpression.Arguments.Count != 3)
        throw ParserUtility.CreateParserException ("SelectMany call with three arguments", selectManyExpression, "SelectMany expressions",
            _resultCollector.ExpressionTreeRoot);

      SourceExpression = selectManyExpression;

      ParseSelectMany ();
    }

    public MethodCallExpression SourceExpression { get; private set; }

    private void ParseSelectMany ()
    {
      UnaryExpression unaryExpression1 = ParserUtility.GetTypedExpression<UnaryExpression> (SourceExpression.Arguments[1],
          "second argument of SelectMany expression", _resultCollector.ExpressionTreeRoot);
      UnaryExpression unaryExpression2 = ParserUtility.GetTypedExpression<UnaryExpression> (SourceExpression.Arguments[2],
          "third argument of SelectMany expression", _resultCollector.ExpressionTreeRoot);
      LambdaExpression ueLambda1 = ParserUtility.GetTypedExpression<LambdaExpression> (unaryExpression1.Operand,
          "second argument of SelectMany expression", _resultCollector.ExpressionTreeRoot);
      LambdaExpression ueLambda2 = ParserUtility.GetTypedExpression<LambdaExpression> (unaryExpression2.Operand,
          "second argument of SelectMany expression", _resultCollector.ExpressionTreeRoot);

      _sourceParser.Parse (_resultCollector, SourceExpression.Arguments[0], ueLambda2.Parameters[0], "first argument of SelectMany expression");

      _resultCollector.AddBodyExpression (new FromExpression (ueLambda1, ueLambda2.Parameters[1]));
      _resultCollector.AddProjectionExpression (ueLambda2);
    }
  }
}
using System.Linq.Expressions;
using Remotion.Data.Linq.Parsing.Structure;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.Structure
{
  public class SelectManyExpressionParser
  {
    private readonly SourceExpressionParser _sourceParser = new SourceExpressionParser (false);

    public SelectManyExpressionParser ()
    {
    }

    public void Parse (ParseResultCollector resultCollector, MethodCallExpression selectManyExpression)
    {
      ArgumentUtility.CheckNotNull ("selectManyExpression", selectManyExpression);
      ArgumentUtility.CheckNotNull ("resultCollector", resultCollector);

      ParserUtility.CheckMethodCallExpression (selectManyExpression, resultCollector.ExpressionTreeRoot, "SelectMany");
      if (selectManyExpression.Arguments.Count != 3)
        throw ParserUtility.CreateParserException ("SelectMany call with three arguments", selectManyExpression, "SelectMany expressions",
            resultCollector.ExpressionTreeRoot);

      ParseSelectMany (resultCollector, selectManyExpression);
    }

    private void ParseSelectMany (ParseResultCollector resultCollector, MethodCallExpression sourceExpression)
    {
      UnaryExpression unaryExpression1 = ParserUtility.GetTypedExpression<UnaryExpression> (sourceExpression.Arguments[1],
          "second argument of SelectMany expression", resultCollector.ExpressionTreeRoot);
      UnaryExpression unaryExpression2 = ParserUtility.GetTypedExpression<UnaryExpression> (sourceExpression.Arguments[2],
          "third argument of SelectMany expression", resultCollector.ExpressionTreeRoot);
      LambdaExpression ueLambda1 = ParserUtility.GetTypedExpression<LambdaExpression> (unaryExpression1.Operand,
          "second argument of SelectMany expression", resultCollector.ExpressionTreeRoot);
      LambdaExpression ueLambda2 = ParserUtility.GetTypedExpression<LambdaExpression> (unaryExpression2.Operand,
          "second argument of SelectMany expression", resultCollector.ExpressionTreeRoot);

      _sourceParser.Parse (resultCollector, sourceExpression.Arguments[0], ueLambda2.Parameters[0], "first argument of SelectMany expression");

      resultCollector.AddBodyExpression (new FromExpressionData (ueLambda1, ueLambda2.Parameters[1]));
      resultCollector.AddProjectionExpression (ueLambda2);
    }
  }
}
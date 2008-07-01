using System;
using System.Linq.Expressions;
using Remotion.Data.Linq.Parsing.Structure;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.Structure
{
  public class SelectExpressionParser
  {
    private readonly SourceExpressionParser _sourceParser = new SourceExpressionParser (false);

    public SelectExpressionParser ()
    {
    }

    public void Parse (ParseResultCollector resultCollector, MethodCallExpression selectExpression)
    {
      ArgumentUtility.CheckNotNull ("selectExpression", selectExpression);
      ArgumentUtility.CheckNotNull ("resultCollector", resultCollector);

      ParserUtility.CheckMethodCallExpression (selectExpression, resultCollector.ExpressionTreeRoot, "Select");

      if (selectExpression.Arguments.Count != 2)
        throw ParserUtility.CreateParserException ("Select call with two arguments", selectExpression, "Select expressions",
            resultCollector.ExpressionTreeRoot);

      ParseSelect (resultCollector, selectExpression);
    }

    private void ParseSelect (ParseResultCollector resultCollector, MethodCallExpression sourceExpression)
    {
      UnaryExpression unaryExpression = ParserUtility.GetTypedExpression<UnaryExpression> (sourceExpression.Arguments[1],
          "second argument of Select expression", resultCollector.ExpressionTreeRoot);
      LambdaExpression ueLambda = ParserUtility.GetTypedExpression<LambdaExpression> (unaryExpression.Operand,
          "second argument of Select expression", resultCollector.ExpressionTreeRoot);

      CheckForSubQuery(resultCollector, sourceExpression, ueLambda);

      _sourceParser.Parse (resultCollector, sourceExpression.Arguments[0], ueLambda.Parameters[0],  "first argument of Select expression");
      resultCollector.AddProjectionExpression (ueLambda);
    }

    private void CheckForSubQuery (ParseResultCollector resultCollector, MethodCallExpression sourceExpression, LambdaExpression ueLambda)
    {
      var possibleSubQuery = ueLambda.Body as MethodCallExpression;
      if (possibleSubQuery != null && _sourceParser.CallDispatcher.CanParse (possibleSubQuery.Method))
        throw ParserUtility.CreateParserException(
            "no subqueries",
            sourceExpression,
            "Select expressions",
            resultCollector.ExpressionTreeRoot);
    }
  }
}
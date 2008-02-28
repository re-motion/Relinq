using System;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using Rubicon.Data.Linq.Parsing.Structure;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.Parsing.Structure
{
  public class SelectExpressionParser
  {
    private readonly ParseResultCollector _resultCollector;
    private readonly SourceExpressionParser _sourceParser = new SourceExpressionParser (false);

    public SelectExpressionParser (ParseResultCollector resultCollector, MethodCallExpression selectExpression)
    {
      ArgumentUtility.CheckNotNull ("selectExpression", selectExpression);
      ArgumentUtility.CheckNotNull ("resultCollector", resultCollector);

      _resultCollector = resultCollector;

      ParserUtility.CheckMethodCallExpression (selectExpression, _resultCollector.ExpressionTreeRoot, "Select");

      if (selectExpression.Arguments.Count != 2)
        throw ParserUtility.CreateParserException ("Select call with two arguments", selectExpression, "Select expressions",
            _resultCollector.ExpressionTreeRoot);

      SourceExpression = selectExpression;

      ParseSelect ();
    }

    public MethodCallExpression SourceExpression { get; private set; }

    private void ParseSelect ()
    {
      UnaryExpression unaryExpression = ParserUtility.GetTypedExpression<UnaryExpression> (SourceExpression.Arguments[1],
          "second argument of Select expression", _resultCollector.ExpressionTreeRoot);
      LambdaExpression ueLambda = ParserUtility.GetTypedExpression<LambdaExpression> (unaryExpression.Operand,
          "second argument of Select expression", _resultCollector.ExpressionTreeRoot);

      _sourceParser.Parse (_resultCollector, SourceExpression.Arguments[0], ueLambda.Parameters[0],  "first argument of Select expression");

      _resultCollector.AddProjectionExpression (ueLambda);
    }
  }
}
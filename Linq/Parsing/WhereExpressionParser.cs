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
    private readonly List<BodyExpressionBase> _fromLetWhereExpressions = new List<BodyExpressionBase> ();
    private readonly List<LambdaExpression> _projectionExpressions = new List<LambdaExpression>();

    public WhereExpressionParser (MethodCallExpression whereExpression, Expression expressionTreeRoot, bool isTopLevel)
    {
      ArgumentUtility.CheckNotNull ("whereExpression", whereExpression);
      ArgumentUtility.CheckNotNull ("expressionTreeRoot", expressionTreeRoot);

      _isTopLevel = isTopLevel;

      ParserUtility.CheckMethodCallExpression (whereExpression, expressionTreeRoot, "Where");
      if (whereExpression.Arguments.Count != 2)
        throw ParserUtility.CreateParserException ("Where call with two arguments", whereExpression, "Where expressions",
            expressionTreeRoot);

      SourceExpression = whereExpression;

      ParseWhere (expressionTreeRoot);
    }

    private void ParseWhere (Expression expressionTreeRoot)
    {
      UnaryExpression unaryExpression = ParserUtility.GetTypedExpression<UnaryExpression> (SourceExpression.Arguments[1],
        "second argument of Where expression", expressionTreeRoot);
      LambdaExpression ueLambda = ParserUtility.GetTypedExpression<LambdaExpression> (unaryExpression.Operand,
        "second argument of Where expression", expressionTreeRoot);

      SourceExpressionParser sourceExpressionParser = new SourceExpressionParser (SourceExpression.Arguments[0], expressionTreeRoot, false,
          ueLambda.Parameters[0], "first argument of Where expression");

      _fromLetWhereExpressions.AddRange (sourceExpressionParser.BodyExpressions);
      _fromLetWhereExpressions.Add (new WhereExpression (ueLambda));
      _projectionExpressions.AddRange (sourceExpressionParser.ProjectionExpressions);
      if (_isTopLevel)
        _projectionExpressions.Add (null);
    }

    public MethodCallExpression SourceExpression { get; private set; }

    public ReadOnlyCollection<BodyExpressionBase> FromLetWhereExpressions
    {
      get { return new ReadOnlyCollection<BodyExpressionBase> (_fromLetWhereExpressions); }
    }

    public ReadOnlyCollection<LambdaExpression> ProjectionExpressions
    {
      get { return new ReadOnlyCollection<LambdaExpression> (_projectionExpressions); }
    }
  }
}
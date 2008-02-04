using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Text;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.Parsing
{
  public class SelectExpressionParser
  {
    private readonly List<BodyExpressionBase> _fromLetWhereExpressions = new List<BodyExpressionBase> ();
    private readonly List<LambdaExpression> _projectionExpressions = new List<LambdaExpression> ();

    public SelectExpressionParser (MethodCallExpression selectExpression, Expression expressionTreeRoot)
    {
      ArgumentUtility.CheckNotNull ("selectExpression", selectExpression);
      ArgumentUtility.CheckNotNull ("expressionTreeRoot", expressionTreeRoot);

      ParserUtility.CheckMethodCallExpression (selectExpression, expressionTreeRoot, "Select");

      if (selectExpression.Arguments.Count != 2)
        throw ParserUtility.CreateParserException ("Select call with two arguments", selectExpression, "Select expressions",
            expressionTreeRoot);

      SourceExpression = selectExpression;

      ParseSelect (expressionTreeRoot);
    }

    private void ParseSelect (Expression expressionTreeRoot)
    {
      UnaryExpression unaryExpression = ParserUtility.GetTypedExpression<UnaryExpression> (SourceExpression.Arguments[1],
          "second argument of Select expression", expressionTreeRoot);
      LambdaExpression ueLambda = ParserUtility.GetTypedExpression<LambdaExpression> (unaryExpression.Operand,
          "second argument of Select expression", expressionTreeRoot);

      SourceExpressionParser sourceExpressionParser = new SourceExpressionParser (SourceExpression.Arguments[0], expressionTreeRoot, false,
          ueLambda.Parameters[0], "first argument of Select expression");

      _fromLetWhereExpressions.AddRange (sourceExpressionParser.BodyExpressions);
      _projectionExpressions.AddRange (sourceExpressionParser.ProjectionExpressions);
      _projectionExpressions.Add (ueLambda);
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
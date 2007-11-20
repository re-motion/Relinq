using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using Rubicon.Utilities;

namespace Rubicon.Data.DomainObjects.Linq.Parsing
{
  public class WhereExpressionParser
  {
    private readonly List<Expression> _fromExpressions = new List<Expression> ();
    private readonly List<ParameterExpression> _fromIdentifiers = new List<ParameterExpression> ();
    private readonly List<LambdaExpression> _boolExpressions = new List<LambdaExpression> ();
    private readonly List<LambdaExpression> _projectionExpressions = new List<LambdaExpression> ();

    public WhereExpressionParser (MethodCallExpression whereExpression, Expression expressionTreeRoot)
    {
      ArgumentUtility.CheckNotNull ("whereExpression", whereExpression);
      ArgumentUtility.CheckNotNull ("expressionTreeRoot", expressionTreeRoot);

      ParserUtility.CheckMethodCallExpression (whereExpression, expressionTreeRoot, "Where");

      SourceExpression = whereExpression;

      ConstantExpression ce = ParserUtility.GetTypedExpression<ConstantExpression> (SourceExpression.Arguments[0],
          "first argument of Where expression", expressionTreeRoot);
      UnaryExpression ue = ParserUtility.GetTypedExpression<UnaryExpression> (SourceExpression.Arguments[1],
          "second argument of Where expression", expressionTreeRoot);
      LambdaExpression ueLambda = ParserUtility.GetTypedExpression<LambdaExpression> (ue.Operand,
          "second argument of Where expression", expressionTreeRoot);

      _fromExpressions.Add (ce);
      _fromIdentifiers.Add (ueLambda.Parameters[0]);
      _boolExpressions.Add (ueLambda);
      _projectionExpressions.Add (Expression.Lambda (ueLambda.Parameters[0]));
    }

    public MethodCallExpression SourceExpression { get; private set; }

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
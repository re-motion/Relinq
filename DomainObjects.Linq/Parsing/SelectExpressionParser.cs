using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using Rubicon.Text;
using Rubicon.Utilities;

namespace Rubicon.Data.DomainObjects.Linq.Parsing
{
  public class SelectExpressionParser
  {
    private readonly List<Expression> _fromExpressions = new List<Expression> ();
    private readonly List<ParameterExpression> _fromIdentifiers = new List<ParameterExpression> ();
    private readonly List<LambdaExpression> _whereExpressions = new List<LambdaExpression> ();
    private readonly List<LambdaExpression> _projectionExpressions = new List<LambdaExpression> ();

    public SelectExpressionParser (MethodCallExpression selectExpression, Expression expressionTreeRoot)
    {
      ArgumentUtility.CheckNotNull ("selectExpression", selectExpression);
      ArgumentUtility.CheckNotNull ("expressionTreeRoot", expressionTreeRoot);

      ParserUtility.CheckMethodCallExpression (selectExpression, expressionTreeRoot, "Select");

      SourceExpression = selectExpression;

      ConstantExpression ce = ParserUtility.GetTypedExpression<ConstantExpression> (SourceExpression.Arguments[0],
          "first argument of Select expression", expressionTreeRoot);
      UnaryExpression ue = ParserUtility.GetTypedExpression<UnaryExpression> (SourceExpression.Arguments[1],
          "second argument of Select expression", expressionTreeRoot);
      LambdaExpression ueLambda = ParserUtility.GetTypedExpression<LambdaExpression> (ue.Operand,
          "second argument of Select expression", expressionTreeRoot);

      _fromExpressions.Add (ce);
      _fromIdentifiers.Add (ueLambda.Parameters[0]);
      _projectionExpressions.Add (ueLambda);
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

    public ReadOnlyCollection<LambdaExpression> WhereExpressions
    {
      get { return new ReadOnlyCollection<LambdaExpression> (_whereExpressions); }
    }

    public ReadOnlyCollection<LambdaExpression> ProjectionExpressions
    {
      get { return new ReadOnlyCollection<LambdaExpression> (_projectionExpressions); }
    }
  }
}
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

      switch (whereExpression.Arguments[0].NodeType)
      {
        case ExpressionType.Constant:
          ParseSimpleWhere (expressionTreeRoot);
          break;
        case ExpressionType.Call:
          switch (ParserUtility.CheckMethodCallExpression ((MethodCallExpression)whereExpression.Arguments[0], expressionTreeRoot, "Where"))
          {
            case "Where":
              ParseRecursiveWhere (expressionTreeRoot);
              break;
          }
          break;
        default:
          throw ParserUtility.CreateParserException ("Constant or Call expression", whereExpression, "first argument of Where expression",
              expressionTreeRoot);
      }
    }

   
    private void ParseSimpleWhere (Expression expressionTreeRoot)
    {
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

    private void ParseRecursiveWhere (Expression expressionTreeRoot)
    {
      MethodCallExpression me = ParserUtility.GetTypedExpression<MethodCallExpression> (SourceExpression.Arguments[0],
                                                                                    "first argument of Where expression", expressionTreeRoot);
      WhereExpressionParser we = new WhereExpressionParser (me, expressionTreeRoot);

      UnaryExpression ue = ParserUtility.GetTypedExpression<UnaryExpression> (SourceExpression.Arguments[1],
                                                                              "second argument of Where expression", expressionTreeRoot);
      LambdaExpression ueLambda = ParserUtility.GetTypedExpression<LambdaExpression> (ue.Operand,
                                                                                      "second argument of Where expression", expressionTreeRoot);

      _fromExpressions.AddRange (we.FromExpressions);
      _fromIdentifiers.AddRange (we.FromIdentifiers);
      _boolExpressions.AddRange (we.BoolExpressions);
      _boolExpressions.Add (ueLambda);
      _projectionExpressions.AddRange (we.ProjectionExpressions);

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
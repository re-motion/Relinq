using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using Rubicon.Text;
using Rubicon.Utilities;

namespace Rubicon.Data.DomainObjects.Linq.Parsing
{
  public class SelectManyExpressionParser
  {
    private readonly List<Expression> _fromExpressions = new List<Expression> ();
    private readonly List<ParameterExpression> _fromIdentifiers = new List<ParameterExpression> ();
    private readonly List<LambdaExpression> _projectionExpressions = new List<LambdaExpression> ();

    public SelectManyExpressionParser (MethodCallExpression selectManyExpression, Expression expressionTreeRoot)
    {
      ArgumentUtility.CheckNotNull ("selectManyExpression", selectManyExpression);
      ArgumentUtility.CheckNotNull ("expressionTreeRoot", expressionTreeRoot);

      ParserUtility.CheckMethodCallExpression (selectManyExpression, expressionTreeRoot, "SelectMany");

      SourceExpression = selectManyExpression;

      switch (selectManyExpression.Arguments[0].NodeType)
      {
        case ExpressionType.Constant:
          ParseSimpleSelectMany (expressionTreeRoot);
          break;
        case ExpressionType.Call:
          ParserUtility.CheckMethodCallExpression ((MethodCallExpression) selectManyExpression.Arguments[0], expressionTreeRoot, "SelectMany");
          ParseRecursiveSelectMany (expressionTreeRoot);
          break;
        default:
          throw ParserUtility.CreateParserException ("Constant or Call expression", selectManyExpression, "first argument of SelectMany expression",
              expressionTreeRoot);
      }
    }

    private void ParseRecursiveSelectMany (Expression expressionTreeRoot)
    {
      //WhereExpressionParser we = new WhereExpressionParser ((MethodCallExpression) SourceExpression.Arguments[0], expressionTreeRoot, false);
      //UnaryExpression ue = ParserUtility.GetTypedExpression<UnaryExpression> (SourceExpression.Arguments[1],
      //    "second argument of Select expression", expressionTreeRoot);
      //LambdaExpression ueLambda = ParserUtility.GetTypedExpression<LambdaExpression> (ue.Operand,
      //    "second argument of Select expression", expressionTreeRoot);

      //_fromExpressions.AddRange (we.FromExpressions);
      //_fromIdentifiers.AddRange (we.FromIdentifiers);
      //_whereExpressions.AddRange (we.BoolExpressions);
      //_projectionExpressions.AddRange (we.ProjectionExpressions);
      //_projectionExpressions.Add (ueLambda);
    }

    private void ParseSimpleSelectMany (Expression expressionTreeRoot)
    {
      ConstantExpression ce = ParserUtility.GetTypedExpression<ConstantExpression> (SourceExpression.Arguments[0],
          "first argument of SelectMany expression", expressionTreeRoot);
      UnaryExpression ue1 = ParserUtility.GetTypedExpression<UnaryExpression> (SourceExpression.Arguments[1],
          "second argument of SelectMany expression", expressionTreeRoot);
      UnaryExpression ue2 = ParserUtility.GetTypedExpression<UnaryExpression> (SourceExpression.Arguments[2],
          "second argument of SelectMany expression", expressionTreeRoot);
      LambdaExpression ueLambda1 = ParserUtility.GetTypedExpression<LambdaExpression> (ue1.Operand,
          "second argument of SelectMany expression", expressionTreeRoot);
      LambdaExpression ueLambda2 = ParserUtility.GetTypedExpression<LambdaExpression> (ue2.Operand,
          "second argument of SelectMany expression", expressionTreeRoot);


      _fromExpressions.Add (ce);
      _fromExpressions.Add (ueLambda1);
      _fromIdentifiers.Add (ueLambda2.Parameters[0]);
      _fromIdentifiers.Add (ueLambda2.Parameters[1]);
      _projectionExpressions.Add (ueLambda2);
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


    public ReadOnlyCollection<LambdaExpression> ProjectionExpressions
    {
      get { return new ReadOnlyCollection<LambdaExpression> (_projectionExpressions); }
    }
  }
}
using System.Linq.Expressions;
using Rubicon.Utilities;

namespace Rubicon.Data.DomainObjects.Linq.UnitTests
{
  public class ExpressionTreeNavigator
  {
    private readonly Expression _expression;

    public ExpressionTreeNavigator (Expression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      _expression = expression;
    }

    public ExpressionTreeNavigator Operand
    {
      get { return new ExpressionTreeNavigator (((UnaryExpression)Expression).Operand); }
    }

    public ExpressionTreeNavigator Body
    {
      get { return new ExpressionTreeNavigator (((LambdaExpression) Expression).Body); }
    }

    public ExpressionTreeNavigator Left
    {
      get { return new ExpressionTreeNavigator (((BinaryExpression) Expression).Left); }
    }

    public ExpressionTreeNavigator Right
    {
      get { return new ExpressionTreeNavigator (((BinaryExpression) Expression).Right); }
    }

    public object Value
    {
      get { return ((ConstantExpression) Expression).Value; }
    }

    public ArgumentsNavigator Arguments
    {
      get { return new ArgumentsNavigator (this); }
    }

    public ParametersNavigator Parameters
    {
      get { return new ParametersNavigator (this); }
    }

    public Expression Expression
    {
      get { return _expression; }
    }

    public class ArgumentsNavigator
    {
      private readonly ExpressionTreeNavigator _navigator;

      public ArgumentsNavigator (ExpressionTreeNavigator navigator)
      {
        _navigator = navigator;
      }

      public ExpressionTreeNavigator this[int index]
      {
        get { return new ExpressionTreeNavigator (((MethodCallExpression) _navigator.Expression).Arguments[index]); }
      }
    }

    public class ParametersNavigator
    {
      private readonly ExpressionTreeNavigator _navigator;

      public ParametersNavigator (ExpressionTreeNavigator navigator)
      {
        _navigator = navigator;
      }

      public ExpressionTreeNavigator this[int index]
      {
        get { return new ExpressionTreeNavigator (((LambdaExpression) _navigator.Expression).Parameters[index]); }
      }
    }
  }
}
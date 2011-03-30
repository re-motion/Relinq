using System;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Linq.Utilities;

namespace Remotion.Linq.Parsing.ExpressionTreeVisitors.Transformation.PredefinedTransformations
{
  /// <summary>
  /// Provides a base class for transformers detecting <see cref="NewExpression"/> nodes for tuple types and adding <see cref="MemberInfo"/> metadata 
  /// to those nodes. This allows LINQ providers to match member access and constructor arguments more easily.
  /// </summary>
  public abstract class MemberAddingNewExpressionTransformerBase : IExpressionTransformer<NewExpression>
  {
    protected abstract bool CanAddMembers (Type instantiatedType, ReadOnlyCollection<Expression> arguments);
    protected abstract MemberInfo[] GetMembers (ConstructorInfo constructorInfo, ReadOnlyCollection<Expression> arguments);

    public ExpressionType[] SupportedExpressionTypes
    {
      get { return new[] { ExpressionType.New }; }
    }

    public Expression Transform (NewExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

// ReSharper disable ConditionIsAlwaysTrueOrFalse
// ReSharper disable HeuristicUnreachableCode
      if (expression.Members == null && CanAddMembers (expression.Type, expression.Arguments))
      {
        var members = GetMembers (expression.Constructor, expression.Arguments);
        return Expression.New (
            expression.Constructor,
            ExpressionTreeVisitor.AdjustArgumentsForNewExpression (expression.Arguments, members),
            members);
      }
// ReSharper restore HeuristicUnreachableCode
// ReSharper restore ConditionIsAlwaysTrueOrFalse

      return expression;
    }

    protected MemberInfo GetMemberForNewExpression (Type instantiatedType, string propertyName)
    {
      // In .NET 4, Expression.New (...) will convert the get method into a property. That way, the generated NewExpression will look exactly like
      // an anonymous type expression.
      return instantiatedType.GetProperty (propertyName).GetGetMethod ();
    }
  }
}
using System;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.Parsing.ExpressionTreeVisitors.Transformation.PredefinedTransformations
{
  /// <summary>
  /// Provides a base class for transformers detecting <see cref="NewExpression"/> nodes for tuple types and adding <see cref="MemberInfo"/> metadata 
  /// to those nodes. This allows LINQ providers to match member access and constructor arguments more easily.
  /// </summary>
  /// <remarks>
  /// <para>
  /// The <see cref="MemberAddingNewExpressionTransformerBase"/> class tries to mimic the expressions created by the C# compiler when
  /// an anonymous type is instantiated. This means that in .NET 3.5, getter methods are associated with the constructor
  /// arguments; in .NET 4, the respective <see cref="PropertyInfo"/> instances are associated with the constructor arguments.
  /// </para>
  /// <para>
  /// For example, conside the following query:
  /// <code>
  /// from c in Cooks
  /// select new KeyValuePair&lt;string, int&gt; (c.Name, c.Age)
  /// </code>
  /// The C# compiler creates the following <see cref="NewExpression"/>:
  /// <code>
  /// Expression.New (
  ///   typeof (KeyValuePair&lt;string, int&gt;).GetConstructor (...),
  ///   new Expression[] { ... },
  ///   null)
  /// </code>
  /// The transformation creates the following for .NET 3.5:
  /// <code>
  /// Expression.New (
  ///   typeof (KeyValuePair&lt;string, int&gt;).GetConstructor (...),
  ///   new Expression[] { ... },
  ///   new MemberInfo[] { 
  ///     typeof (KeyValuePair&lt;string, int&gt;).GetMethod ("get_Key"),
  ///     typeof (KeyValuePair&lt;string, int&gt;).GetMethod ("get_Value")
  ///   })
  /// </code>
  /// For .NET 4, this changes to:
  /// <code>
  /// Expression.New (
  ///   typeof (KeyValuePair&lt;string, int&gt;).GetConstructor (...),
  ///   new Expression[] { ... },
  ///   new MemberInfo[] { 
  ///     typeof (KeyValuePair&lt;string, int&gt;).GetProperty ("Key"),
  ///     typeof (KeyValuePair&lt;string, int&gt;).GetProperty ("Value")
  ///   })
  /// </code>
  /// </para>
  /// <para>
  /// This difference in behavior has been implemented to be consistent with the construction of anonymous types.
  /// </para>
  /// </remarks>
  public abstract class MemberAddingNewExpressionTransformerBase : IExpressionTransformer<NewExpression>
  {
    private readonly Version _frameworkVersion;

    protected MemberAddingNewExpressionTransformerBase (Version frameworkVersion)
    {
      ArgumentUtility.CheckNotNull ("frameworkVersion", frameworkVersion);
      _frameworkVersion = frameworkVersion;
    }

    protected abstract bool CanAddMembers (Type instantiatedType, ReadOnlyCollection<Expression> arguments);
    protected abstract MemberInfo[] GetMembers (ConstructorInfo constructorInfo, ReadOnlyCollection<Expression> arguments);

    public Version FrameworkVersion
    {
      get { return _frameworkVersion; }
    }

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
      if (_frameworkVersion.Major < 4)
        return instantiatedType.GetProperty (propertyName).GetGetMethod ();
      else
        return instantiatedType.GetProperty (propertyName);
    }
  }
}
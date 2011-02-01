using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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

    public ExpressionType[] SupportedExpressionTypes
    {
      get { return new[] { ExpressionType.New }; }
    }

    public Expression Transform (NewExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      if (CanAddMembers (expression.Type, expression.Arguments))
      {
        var members = GetMembers (expression.Constructor, expression.Arguments);
        return Expression.New (
            expression.Constructor,
            AdjustTypes (expression.Arguments, members),
            members);
      }

      return expression;
    }

    private IEnumerable<Expression> AdjustTypes (ReadOnlyCollection<Expression> arguments, MemberInfo[] members)
    {
      // Because the arguments must have exactly the same types as the members (no assignment compatibility), this method is needed
      // to convert the expressions to the respective member types.

      Trace.Assert (arguments.Count == members.Length);
      
      for (int i = 0; i < arguments.Count; ++i)
      {
        var memberReturnType = ReflectionUtility.GetMemberReturnType (members[i]);
        if (arguments[i].Type == memberReturnType)
          yield return arguments[i];
        else
          yield return Expression.Convert (arguments[i], memberReturnType);
      }
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
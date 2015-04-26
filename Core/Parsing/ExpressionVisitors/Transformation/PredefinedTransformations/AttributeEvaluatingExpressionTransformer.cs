// Copyright (c) rubicon IT GmbH, www.rubicon.eu
//
// See the NOTICE file distributed with this work for additional information
// regarding copyright ownership.  rubicon licenses this file to you under 
// the Apache License, Version 2.0 (the "License"); you may not use this 
// file except in compliance with the License.  You may obtain a copy of the 
// License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, WITHOUT 
// WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.  See the 
// License for the specific language governing permissions and limitations
// under the License.
// 

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Utilities;

namespace Remotion.Linq.Parsing.ExpressionVisitors.Transformation.PredefinedTransformations
{
  /// <summary>
  /// Dynamically discovers attributes implementing the <see cref="IMethodCallExpressionTransformerAttribute"/> interface on methods and get accessors
  /// invoked by <see cref="MethodCallExpression"/> or <see cref="MemberExpression"/> instances and applies the respective 
  /// <see cref="IExpressionTransformer{T}"/>.
  /// </summary>
  public class AttributeEvaluatingExpressionTransformer : IExpressionTransformer<Expression>
  {
    /// <summary>
    /// Defines an interface for attributes providing an <see cref="IExpressionTransformer{T}"/> for a given <see cref="MethodCallExpression"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see cref="AttributeEvaluatingExpressionTransformer"/> detects attributes implementing this interface while expressions are parsed 
    /// and uses the <see cref="IExpressionTransformer{T}"/> returned by <see cref="GetExpressionTransformer"/> to modify the expressions.
    /// </para>
    /// <para>
    /// Only one attribute instance implementing <see cref="IMethodCallExpressionTransformerAttribute"/> must be applied to a single method or property
    /// get accessor.
    /// </para>
    /// </remarks>
    public interface IMethodCallExpressionTransformerAttribute
    {
      IExpressionTransformer<MethodCallExpression> GetExpressionTransformer (MethodCallExpression expression);
    }

    public ExpressionType[] SupportedExpressionTypes
    {
      get { return new[] { ExpressionType.Call, ExpressionType.MemberAccess }; }
    }

    public Expression Transform (Expression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var memberExpression = expression as MemberExpression;
      if (memberExpression != null && memberExpression.Member is PropertyInfo)
      {
        var property = (PropertyInfo) memberExpression.Member;
        var getter = property.GetGetMethod (true);
        Assertion.IsNotNull (getter, "No get-method was found for property '{0}' declared on type '{1}'.", property.Name, property.DeclaringType);

        var methodCallExpressionTransformerProvider = GetTransformerProvider (getter);
        if (methodCallExpressionTransformerProvider != null)
          return ApplyTransformer (methodCallExpressionTransformerProvider, Expression.Call (memberExpression.Expression, getter));
      }

      var methodCallExpression = expression as MethodCallExpression;
      if (methodCallExpression != null)
      {
        var methodCallExpressionTransformerProvider = GetTransformerProvider (methodCallExpression.Method);
        if (methodCallExpressionTransformerProvider != null)
            return ApplyTransformer (methodCallExpressionTransformerProvider, methodCallExpression);
      }

      return expression;
    }

    private static IMethodCallExpressionTransformerAttribute GetTransformerProvider (MethodInfo methodInfo)
    {
      var attributes = methodInfo.GetCustomAttributes (true).OfType<IMethodCallExpressionTransformerAttribute>().ToArray();

      if (attributes.Length > 1)
      {
        var message = string.Format (
            "There is more than one attribute providing transformers declared for method '{0}.{1}'.",
            methodInfo.DeclaringType,
            methodInfo.Name);
        throw new InvalidOperationException (message);
      }

      return attributes.SingleOrDefault();
    }

    private static Expression ApplyTransformer (IMethodCallExpressionTransformerAttribute provider, MethodCallExpression methodCallExpression)
    {
      var expressionTransformer = provider.GetExpressionTransformer (methodCallExpression);
      if (expressionTransformer == null)
      {
        var message = string.Format (
            "The '{0}' on method '{1}.{2}' returned 'null' instead of a transformer.",
            provider.GetType().Name,
            methodCallExpression.Method.DeclaringType,
            methodCallExpression.Method.Name);
        throw new InvalidOperationException (message);
      }

      return expressionTransformer.Transform (methodCallExpression);
    }
  }
}
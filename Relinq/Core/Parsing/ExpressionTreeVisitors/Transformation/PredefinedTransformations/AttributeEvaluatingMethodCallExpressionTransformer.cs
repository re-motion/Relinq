// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (c) rubicon IT GmbH, www.rubicon.eu
// 
// re-linq is free software; you can redistribute it and/or modify it under 
// the terms of the GNU Lesser General Public License as published by the 
// Free Software Foundation; either version 2.1 of the License, 
// or (at your option) any later version.
// 
// re-linq is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-linq; if not, see http://www.gnu.org/licenses.
// 

using System;
using System.Linq.Expressions;
using Remotion.Linq.Utilities;

namespace Remotion.Linq.Parsing.ExpressionTreeVisitors.Transformation.PredefinedTransformations
{
  /// <summary>
  /// Dynamically discovers attributes implementing the <see cref="IMethodCallExpressionTransformerProvider"/> interface on methods and get accessors
  /// invoked by <see cref="MethodCallExpression"/> or <see cref="MemberExpression"/> instances and applies the respective 
  /// <see cref="IExpressionTransformer{T}"/>.
  /// </summary>
  public class AttributeEvaluatingMethodCallExpressionTransformer : IExpressionTransformer<MethodCallExpression>
  {
    /// <summary>
    /// Defines an interface for attributes providing an <see cref="IExpressionTransformer{T}"/> for a given <see cref="MethodCallExpression"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see cref="AttributeEvaluatingMethodCallExpressionTransformer"/> detects attributes implementing this interface while expressions are parsed 
    /// and uses the <see cref="IExpressionTransformer{T}"/> returned by <see cref="GetExpressionTransformer"/> to modify the expressions.
    /// </para>
    /// <para>
    /// Only one attribute instance implementing <see cref="IMethodCallExpressionTransformerProvider"/> must be applied to a single method or property
    /// get accessor.
    /// </para>
    /// </remarks>
    public interface IMethodCallExpressionTransformerProvider
    {
      IExpressionTransformer<MethodCallExpression> GetExpressionTransformer (MethodCallExpression expression);
    }

    public ExpressionType[] SupportedExpressionTypes
    {
      get { return new[] { ExpressionType.Call }; }
    }

    public Expression Transform (MethodCallExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var attributes =
          (IMethodCallExpressionTransformerProvider[]) expression.Method.GetCustomAttributes (typeof (IMethodCallExpressionTransformerProvider), true);
      if (attributes.Length == 0)
        return expression;
      if (attributes.Length > 1)
      {
        var message = string.Format (
              "There is more than one attribute providing transformers declared for method '{0}.{1}'.",
              expression.Method.DeclaringType,
              expression.Method.Name);
        throw new InvalidOperationException (message);
      }

      var expressionTransformer = attributes[0].GetExpressionTransformer (expression);
      if (expressionTransformer == null)
      {
        var message = string.Format (
            "The '{0}' on method '{1}.{2}' returned 'null' instead of a transformer.",
            attributes[0].GetType().Name,
            expression.Method.DeclaringType,
            expression.Method.Name);
        throw new InvalidOperationException (message);
      }

      return expressionTransformer.Transform (expression);
    }
  }
}
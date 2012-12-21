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
  /// Chooses a given <see cref="IExpressionTransformer{T}"/> for a specific method (or property get accessor).
  /// </summary>
  /// <remarks>
  /// The <see cref="IExpressionTransformer{T}"/> must have a default constructor. To choose a transformer that does not have a default constructor,
  /// create your own custom attribute class implementing 
  /// <see cref="AttributeEvaluatingExpressionTransformer.IMethodCallExpressionTransformerAttribute"/>.
  /// </remarks>
  [AttributeUsage (AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
  public class MethodCallExpressionTransformerAttribute : Attribute, AttributeEvaluatingExpressionTransformer.IMethodCallExpressionTransformerAttribute
  {
    private readonly Type _transformerType;

    public MethodCallExpressionTransformerAttribute (Type transformerType)
    {
      ArgumentUtility.CheckNotNull ("transformerType", transformerType);
      ArgumentUtility.CheckTypeIsAssignableFrom ("transformerType", transformerType, typeof (IExpressionTransformer<MethodCallExpression>));

      _transformerType = transformerType;
    }

    public Type TransformerType
    {
      get { return _transformerType; }
    }

    public IExpressionTransformer<MethodCallExpression> GetExpressionTransformer (MethodCallExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      try
      {
        return (IExpressionTransformer<MethodCallExpression>) Activator.CreateInstance (_transformerType);
      }
      catch (MissingMethodException ex)
      {
        var message = String.Format (
            "The method call transformer '{0}' has no public default constructor and therefore cannot be used with the MethodCallExpressionTransformerAttribute.",
            _transformerType);
        throw new InvalidOperationException (message, ex);
      }
    }
  }
}
// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// as published by the Free Software Foundation; either version 2.1 of the 
// License, or (at your option) any later version.
// 
// re-motion is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-motion; if not, see http://www.gnu.org/licenses.
// 
using System;
using System.Linq.Expressions;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.Parsing.ExpressionTreeVisitors.Transformation.PredefinedTransformations
{
  /// <summary>
  /// Detects expressions invoking a <see cref="LambdaExpression"/> and replaces them with the body of that 
  /// <see cref="LambdaExpression"/> (with the parameter references replaced with the invocation arguments).
  /// Providers use this transformation to be able to handle queries with <see cref="InvocationExpression"/> instances.
  /// </summary>
  /// <remarks>
  /// When the <see cref="InvocationExpression"/> is applied to a delegate instance (rather than a 
  /// <see cref="LambdaExpression"/>), the <see cref="InvocationOfLambdaExpressionTransformer"/> ignores it.
  /// </remarks>
  public class InvocationOfLambdaExpressionTransformer : IExpressionTransformer<InvocationExpression>
  {
    public ExpressionType[] SupportedExpressionTypes
    {
      get { return new[] { ExpressionType.Invoke }; }
    }

    public Expression Transform (InvocationExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var innerExpressionAsLambda = expression.Expression as LambdaExpression;
      if (innerExpressionAsLambda!=null)
      {
        var body = innerExpressionAsLambda.Body;
        for (int i = 0; i < innerExpressionAsLambda.Parameters.Count; i++)
        {
          var parameter = innerExpressionAsLambda.Parameters[i];
          var argument = expression.Arguments[i];

          body = ReplacingExpressionTreeVisitor.Replace (parameter, argument, body);
        }
        return body;
      }
      return expression;
    }
  }
}
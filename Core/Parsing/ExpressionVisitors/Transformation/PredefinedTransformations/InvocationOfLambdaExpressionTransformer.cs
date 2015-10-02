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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using Remotion.Utilities;

namespace Remotion.Linq.Parsing.ExpressionVisitors.Transformation.PredefinedTransformations
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

      Expression invokedExpression = StripTrivialConversions (expression.Expression);

      var innerExpressionAsLambda = invokedExpression as LambdaExpression;
      if (innerExpressionAsLambda != null)
        return InlineLambdaExpression (innerExpressionAsLambda, expression.Arguments);
      else
        return expression;
    }

    private Expression StripTrivialConversions (Expression invokedExpression)
    {
      while (invokedExpression.NodeType == ExpressionType.Convert
             && invokedExpression.Type == ((UnaryExpression) invokedExpression).Operand.Type
             && ((UnaryExpression) invokedExpression).Method == null)
      {
        invokedExpression = ((UnaryExpression) invokedExpression).Operand;
      }
      return invokedExpression;
    }

    private Expression InlineLambdaExpression (LambdaExpression lambdaExpression, ReadOnlyCollection<Expression> arguments)
    {
      Assertion.DebugAssert (lambdaExpression.Parameters.Count == arguments.Count);

      var mapping = new Dictionary<Expression, Expression> (arguments.Count);
      
      var body = lambdaExpression.Body;
      for (int i = 0; i < lambdaExpression.Parameters.Count; i++)
        mapping.Add (lambdaExpression.Parameters[i], arguments[i]);

      return MultiReplacingExpressionVisitor.Replace (mapping, body);
    }
  }
}
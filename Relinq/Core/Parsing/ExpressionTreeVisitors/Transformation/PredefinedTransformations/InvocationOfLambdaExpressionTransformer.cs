// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq.Expressions;
using Remotion.Linq.Utilities;

namespace Remotion.Linq.Parsing.ExpressionTreeVisitors.Transformation.PredefinedTransformations
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
      Debug.Assert (lambdaExpression.Parameters.Count == arguments.Count);

      var mapping = new Dictionary<Expression, Expression> (arguments.Count);
      
      var body = lambdaExpression.Body;
      for (int i = 0; i < lambdaExpression.Parameters.Count; i++)
        mapping.Add (lambdaExpression.Parameters[i], arguments[i]);

      return MultiReplacingExpressionTreeVisitor.Replace (mapping, body);
    }
  }
}
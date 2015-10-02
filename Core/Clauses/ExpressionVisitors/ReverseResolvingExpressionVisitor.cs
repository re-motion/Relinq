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
using System.Linq.Expressions;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Parsing;
using Remotion.Linq.Parsing.Structure.IntermediateModel;
using Remotion.Linq.Utilities;
using Remotion.Utilities;

namespace Remotion.Linq.Clauses.ExpressionVisitors
{
  /// <summary>
  /// Performs a reverse <see cref="IExpressionNode.Resolve"/> operation, i.e. creates a <see cref="LambdaExpression"/> from a given resolved expression, 
  /// substituting all <see cref="QuerySourceReferenceExpression"/> objects by getting the referenced objects from the lambda's input parameter.
  /// </summary>
  /// <example>
  /// Given the following input:
  /// <list type="bullet">
  /// <item>ItemExpression: <c>new AnonymousType ( a = [s1], b = [s2] )</c></item>
  /// <item>ResolvedExpression: <c>[s1].ID + [s2].ID</c></item>
  /// </list> 
  /// The visitor generates the following <see cref="LambdaExpression"/>: <c>input => input.a.ID + input.b.ID</c>
  /// The lambda's input parameter has the same type as the ItemExpression.
  /// </example>
  public sealed class ReverseResolvingExpressionVisitor : RelinqExpressionVisitor
  {
    /// <summary>
    /// Performs a reverse <see cref="IExpressionNode.Resolve"/> operation, i.e. creates a <see cref="LambdaExpression"/> from a given resolved expression, 
    /// substituting all <see cref="QuerySourceReferenceExpression"/> objects by getting the referenced objects from the lambda's input parameter.
    /// </summary>
    /// <param name="itemExpression">The item expression representing the items passed to the generated <see cref="LambdaExpression"/> via its input 
    /// parameter.</param>
    /// <param name="resolvedExpression">The resolved expression for which to generate a reverse resolved <see cref="LambdaExpression"/>.</param>
    /// <returns>A <see cref="LambdaExpression"/> from the given resolved expression, substituting all <see cref="QuerySourceReferenceExpression"/> 
    /// objects by getting the referenced objects from the lambda's input parameter. The generated <see cref="LambdaExpression"/> has exactly one 
    /// parameter which is of the type defined by <paramref name="itemExpression"/>.</returns>
    public static LambdaExpression ReverseResolve (Expression itemExpression, Expression resolvedExpression)
    {
      ArgumentUtility.CheckNotNull ("itemExpression", itemExpression);
      ArgumentUtility.CheckNotNull ("resolvedExpression", resolvedExpression);

      var lambdaParameter = Expression.Parameter (itemExpression.Type, "input");
      var visitor = new ReverseResolvingExpressionVisitor (itemExpression, lambdaParameter);
      var result = visitor.Visit (resolvedExpression);
      return Expression.Lambda (result, lambdaParameter);
    }

    /// <summary>
    /// Performs a reverse <see cref="IExpressionNode.Resolve"/> operation on a <see cref="LambdaExpression"/>, i.e. creates a new 
    /// <see cref="LambdaExpression"/> with an additional parameter from a given resolved <see cref="LambdaExpression"/>, 
    /// substituting all <see cref="QuerySourceReferenceExpression"/> objects by getting the referenced objects from the new input parameter.
    /// </summary>
    /// <param name="itemExpression">The item expression representing the items passed to the generated <see cref="LambdaExpression"/> via its new
    /// input parameter.</param>
    /// <param name="resolvedExpression">The resolved <see cref="LambdaExpression"/> for which to generate a reverse resolved <see cref="LambdaExpression"/>.</param>
    /// <param name="parameterInsertionPosition">The position at which to insert the new parameter.</param>
    /// <returns>A <see cref="LambdaExpression"/> similar to the given resolved expression, substituting all <see cref="QuerySourceReferenceExpression"/> 
    /// objects by getting the referenced objects from an additional input parameter. The new input parameter is of the type defined by 
    /// <paramref name="itemExpression"/>.</returns>
    public static LambdaExpression ReverseResolveLambda (Expression itemExpression, LambdaExpression resolvedExpression, int parameterInsertionPosition)
    {
      ArgumentUtility.CheckNotNull ("itemExpression", itemExpression);
      ArgumentUtility.CheckNotNull ("resolvedExpression", resolvedExpression);

      if (parameterInsertionPosition < 0 || parameterInsertionPosition > resolvedExpression.Parameters.Count)
        throw new ArgumentOutOfRangeException ("parameterInsertionPosition");

      var lambdaParameter = Expression.Parameter (itemExpression.Type, "input");
      var visitor = new ReverseResolvingExpressionVisitor (itemExpression, lambdaParameter);
      var result = visitor.Visit (resolvedExpression.Body);
      
      var parameters = new List<ParameterExpression> (resolvedExpression.Parameters);
      parameters.Insert (parameterInsertionPosition, lambdaParameter);
      return Expression.Lambda (result, parameters.ToArray());
    }


    private readonly Expression _itemExpression;
    private readonly ParameterExpression _lambdaParameter;

    private ReverseResolvingExpressionVisitor (Expression itemExpression, ParameterExpression lambdaParameter)
    {
      ArgumentUtility.CheckNotNull ("itemExpression", itemExpression);
      ArgumentUtility.CheckNotNull ("lambdaParameter", lambdaParameter);

      _itemExpression = itemExpression;
      _lambdaParameter = lambdaParameter;
    }

    protected internal override Expression VisitQuerySourceReference (QuerySourceReferenceExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      try
      {
        var accessorLambda = AccessorFindingExpressionVisitor.FindAccessorLambda (expression, _itemExpression, _lambdaParameter);
        return accessorLambda.Body;
      }
      catch (ArgumentException ex)
      {
        var message = string.Format (
            "Cannot create a LambdaExpression that retrieves the value of '{0}' from items with a structure of '{1}'. The item expression does not "
            + "contain the value or it is too complex.",
            expression.BuildString(),
            _itemExpression.BuildString());
        throw new InvalidOperationException (message, ex);
      }
    }
  }
}

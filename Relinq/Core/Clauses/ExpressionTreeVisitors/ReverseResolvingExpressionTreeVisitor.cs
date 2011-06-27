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
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Parsing;
using Remotion.Linq.Parsing.Structure.IntermediateModel;
using Remotion.Linq.Utilities;

namespace Remotion.Linq.Clauses.ExpressionTreeVisitors
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
  public class ReverseResolvingExpressionTreeVisitor : ExpressionTreeVisitor
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
      var visitor = new ReverseResolvingExpressionTreeVisitor (itemExpression, lambdaParameter);
      var result = visitor.VisitExpression (resolvedExpression);
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
      var visitor = new ReverseResolvingExpressionTreeVisitor (itemExpression, lambdaParameter);
      var result = visitor.VisitExpression (resolvedExpression.Body);
      
      var parameters = new List<ParameterExpression> (resolvedExpression.Parameters);
      parameters.Insert (parameterInsertionPosition, lambdaParameter);
      return Expression.Lambda (result, parameters.ToArray());
    }


    private readonly Expression _itemExpression;
    private readonly ParameterExpression _lambdaParameter;

    private ReverseResolvingExpressionTreeVisitor (Expression itemExpression, ParameterExpression lambdaParameter)
    {
      ArgumentUtility.CheckNotNull ("itemExpression", itemExpression);
      ArgumentUtility.CheckNotNull ("lambdaParameter", lambdaParameter);

      _itemExpression = itemExpression;
      _lambdaParameter = lambdaParameter;
    }

    protected override Expression VisitQuerySourceReferenceExpression (QuerySourceReferenceExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      try
      {
        var accessorLambda = AccessorFindingExpressionTreeVisitor.FindAccessorLambda (expression, _itemExpression, _lambdaParameter);
        return accessorLambda.Body;
      }
      catch (ArgumentException ex)
      {
        var message = string.Format (
            "Cannot create a LambdaExpression that retrieves the value of '{0}' from items with a structure of '{1}'. The item expression does not "
            + "contain the value or it is too complex.",
            FormattingExpressionTreeVisitor.Format (expression),
            FormattingExpressionTreeVisitor.Format (_itemExpression));
        throw new InvalidOperationException (message, ex);
      }
    }
  }
}

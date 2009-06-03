// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// version 3.0 as published by the Free Software Foundation.
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

namespace Remotion.Data.Linq.Parsing.Structure.IntermediateModel
{
  /// <summary>
  /// Interface for classes representing structural parts of an <see cref="Expression"/> tree.
  /// </summary>
  public interface IExpressionNode
  {
    /// <summary>
    /// Resolves the specified <paramref name="expressionToBeResolved"/> by replacing any occurrence of <paramref name="inputParameter"/>
    /// by the result of the projection of this <see cref="IExpressionNode"/>. The result is an <see cref="Expression"/> that goes all the
    /// way to an <see cref="IdentifierReferenceExpression"/>.
    /// </summary>
    /// <param name="inputParameter">The parameter representing the input data streaming into an <see cref="IExpressionNode"/>. This is replaced
    /// by the projection data coming out of this <see cref="IExpressionNode"/>.</param>
    /// <param name="expressionToBeResolved">The expression to be resolved. Any occurrence of <paramref name="inputParameter"/> in this expression
    /// is replaced.</param>
    /// <returns>An equivalent of <paramref name="expressionToBeResolved"/> with each occurrence of <paramref name="inputParameter"/> replaced by
    /// the projection data streaming out of this <see cref="IExpressionNode"/>.</returns>
    /// <exception cref="InvalidOperationException">
    /// This node does not support this operation because it does not stream any data to subsequent nodes.
    /// </exception>
    Expression Resolve (ParameterExpression inputParameter, Expression expressionToBeResolved);

    /// <summary>
    /// Creates a <see cref="ParameterExpression"/> that can take elements of the output stream provided by this node. This 
    /// <see cref="ParameterExpression"/> can be used by the following <see cref="IExpressionNode"/> as an input to its selector or predicate
    /// <see cref="LambdaExpression"/>.
    /// </summary>
    /// <returns>A <see cref="ParameterExpression"/> whose name and type corresponds to the elements streamed out of this node.</returns>
    /// <exception cref="InvalidOperationException">
    /// This node does not support this operation because it does not stream any data to subsequent nodes.
    /// </exception>
    ParameterExpression CreateParameterForOutput ();
  }
}
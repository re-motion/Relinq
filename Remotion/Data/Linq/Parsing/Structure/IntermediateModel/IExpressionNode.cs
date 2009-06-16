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
using Remotion.Data.Linq.Clauses;

namespace Remotion.Data.Linq.Parsing.Structure.IntermediateModel
{
  /// <summary>
  /// Interface for classes representing structural parts of an <see cref="Expression"/> tree.
  /// </summary>
  public interface IExpressionNode
  {
    /// <summary>
    /// Gets the source <see cref="IExpressionNode"/> that streams data into this node.
    /// </summary>
    /// <value>The source <see cref="IExpressionNode"/>, or <see langword="null"/> if this node is the end of the chain.</value>
    IExpressionNode Source { get; }

    /// <summary>
    /// Resolves the specified <paramref name="expressionToBeResolved"/> by replacing any occurrence of <paramref name="inputParameter"/>
    /// by the result of the projection of this <see cref="IExpressionNode"/>. The result is an <see cref="Expression"/> that goes all the
    /// way to an <see cref="QuerySourceReferenceExpression"/>.
    /// </summary>
    /// <param name="inputParameter">The parameter representing the input data streaming into an <see cref="IExpressionNode"/>. This is replaced
    /// by the projection data coming out of this <see cref="IExpressionNode"/>.</param>
    /// <param name="expressionToBeResolved">The expression to be resolved. Any occurrence of <paramref name="inputParameter"/> in this expression
    /// is replaced.</param>
    /// <param name="querySourceClauseMapping">The <see cref="QuerySourceClauseMapping"/>, which maps <see cref="IQuerySourceExpressionNode"/>s 
    /// to the clauses created from them. Implementers that also implement <see cref="IQuerySourceExpressionNode"/> (such as 
    /// <see cref="ConstantExpressionNode"/> or <see cref="SelectManyExpressionNode"/>) must add their clauses to the mapping in 
    /// <see cref="CreateClause"/> if they want to be able to implement <see cref="Resolve"/> correctly.</param>
    /// <returns>An equivalent of <paramref name="expressionToBeResolved"/> with each occurrence of <paramref name="inputParameter"/> replaced by
    /// the projection data streaming out of this <see cref="IExpressionNode"/>.</returns>
    /// <exception cref="InvalidOperationException">
    /// This node does not support this operation because it does not stream any data to subsequent nodes.
    /// </exception>
    Expression Resolve (ParameterExpression inputParameter, Expression expressionToBeResolved, QuerySourceClauseMapping querySourceClauseMapping);

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

    /// <summary>
    /// Creates an instance of <see cref="IClause"/> that can represent the current <see cref="IExpressionNode"/> in a <see cref="QueryModel"/>.
    /// All expressions of the <see cref="IExpressionNode"/> are passed to the new <see cref="IClause"/> as required, and the 
    /// <see cref="IClause.PreviousClause"/> of the new <see cref="IClause"/> is set to the given <paramref name="previousClause"/>.
    /// </summary>
    /// <param name="previousClause">The previous clause the new <see cref="IClause"/> should link to. For <see cref="IExpressionNode"/>
    /// instances representing the end of a query chain (e.g. <see cref="ConstantExpressionNode"/>), this must be <see langword="null"/>.</param>
    /// <param name="querySourceClauseMapping">The <see cref="QuerySourceClauseMapping"/>, which maps <see cref="IQuerySourceExpressionNode"/>s 
    /// to the clauses created from them. Implementers that also implement <see cref="IQuerySourceExpressionNode"/> (such as 
    /// <see cref="ConstantExpressionNode"/> or <see cref="SelectManyExpressionNode"/>) must add their clauses to the mapping in 
    /// <see cref="CreateClause"/> if they want to be able to implement <see cref="Resolve"/> correctly.</param>
    /// <returns>A new <see cref="IClause"/> instance representing this <see cref="IExpressionNode"/>.</returns>
    IClause CreateClause (IClause previousClause, QuerySourceClauseMapping querySourceClauseMapping);
  }
}
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
using System.Linq.Expressions;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;

namespace Remotion.Linq.Parsing.Structure.IntermediateModel
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
    /// Gets the identifier associated with this <see cref="IExpressionNode"/>. <see cref="ExpressionTreeParser"/> tries to find the identifier
    /// that was originally associated with this node in the query written by the user by analyzing the parameter names of the next expression in the 
    /// method call chain.
    /// </summary>
    /// <value>The associated identifier.</value>
    string AssociatedIdentifier { get; }

    /// <summary>
    /// Resolves the specified <paramref name="expressionToBeResolved"/> by replacing any occurrence of <paramref name="inputParameter"/>
    /// by the result of the projection of this <see cref="IExpressionNode"/>. The result is an <see cref="Expression"/> that goes all the
    /// way to an <see cref="QuerySourceReferenceExpression"/>.
    /// </summary>
    /// <param name="inputParameter">The parameter representing the input data streaming into an <see cref="IExpressionNode"/>. This is replaced
    /// by the projection data coming out of this <see cref="IExpressionNode"/>.</param>
    /// <param name="expressionToBeResolved">The expression to be resolved. Any occurrence of <paramref name="inputParameter"/> in this expression
    /// is replaced.</param>
    /// <param name="clauseGenerationContext">Context information used during the current parsing process. This structure maps 
    /// <see cref="IQuerySourceExpressionNode"/>s  to the clauses created from them. Implementers that also implement 
    /// <see cref="IQuerySourceExpressionNode"/> (such as  <see cref="MainSourceExpressionNode"/> or <see cref="SelectManyExpressionNode"/>) must add 
    /// their clauses to the mapping in <see cref="Apply"/> if they want to be able to implement <see cref="Resolve"/> correctly.</param>
    /// <returns>An equivalent of <paramref name="expressionToBeResolved"/> with each occurrence of <paramref name="inputParameter"/> replaced by
    /// the projection data streaming out of this <see cref="IExpressionNode"/>.</returns>
    /// <exception cref="InvalidOperationException">
    /// This node does not support this operation because it does not stream any data to subsequent nodes.
    /// </exception>
    Expression Resolve (ParameterExpression inputParameter, Expression expressionToBeResolved, ClauseGenerationContext clauseGenerationContext);

    /// <summary>
    /// Applies this <see cref="IExpressionNode"/> to the specified query model. Nodes can add or replace clauses, add or replace expressions, 
    /// add or replace <see cref="ResultOperatorBase"/> objects, or even create a completely new <see cref="QueryModel"/>, depending on their semantics.
    /// </summary>
    /// <param name="queryModel">The query model this node should be applied to.</param>
    /// <param name="clauseGenerationContext">Context information used during the current parsing process. This structure maps 
    /// <see cref="IQuerySourceExpressionNode"/>s to the clauses created from them. Implementers that 
    /// also implement <see cref="IQuerySourceExpressionNode"/> (such as 
    /// <see cref="MainSourceExpressionNode"/> or <see cref="SelectManyExpressionNode"/>) must add their clauses to the mapping in 
    /// <see cref="Apply"/> in order to be able to implement <see cref="Resolve"/> correctly.</param>
    /// <returns>The modified <paramref name="queryModel"/> or a new <see cref="QueryModel"/> that reflects the changes made by this node.</returns>
    /// <remarks>
    /// For <see cref="MainSourceExpressionNode"/> objects, which mark the end of an <see cref="IExpressionNode"/> chain, this method must not be called.
    /// Instead, use <see cref="MainSourceExpressionNode.CreateMainFromClause"/> to generate a <see cref="MainFromClause"/> and instantiate a new 
    /// <see cref="QueryModel"/> with that clause.
    /// </remarks>
    QueryModel Apply (QueryModel queryModel, ClauseGenerationContext clauseGenerationContext);
  }
}

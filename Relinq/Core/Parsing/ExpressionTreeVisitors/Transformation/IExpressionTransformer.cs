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
using System.Linq.Expressions;

namespace Remotion.Linq.Parsing.ExpressionTreeVisitors.Transformation
{
  /// <summary>
  /// <see cref="IExpressionTransformer{T}"/> is implemented by classes that transform <see cref="Expression"/> instances. The 
  /// <see cref="ExpressionTransformerRegistry"/> manages registration of <see cref="IExpressionTransformer{T}"/> instances, and the 
  /// <see cref="TransformingExpressionTreeVisitor"/> applies the transformations.
  /// </summary>
  /// <typeparam name="T">The type of expressions handled by this <see cref="IExpressionTransformer{T}"/> implementation.</typeparam>
  /// <remarks>
  /// <para>
  /// <see cref="IExpressionTransformer{T}"/> is a convenience interface that provides strong typing, whereas 
  /// <see cref="ExpressionTransformation"/> only operates on <see cref="Expression"/> instances. 
  /// </para>
  /// <para>
  /// <see cref="IExpressionTransformer{T}"/> can be used together with the <see cref="TransformingExpressionTreeVisitor"/> class by using the 
  /// <see cref="ExpressionTransformerRegistry"/> class as the transformation provider. <see cref="ExpressionTransformerRegistry"/> converts 
  /// strongly typed <see cref="IExpressionTransformer{T}"/> instances to weakly typed <see cref="ExpressionTransformation"/> delegate instances.
  /// </para>
  /// </remarks>
  public interface IExpressionTransformer<T> where T : Expression
  {
    /// <summary>
    /// Gets the expression types supported by this <see cref="IExpressionTransformer{T}"/>.
    /// </summary>
    /// <value>The supported expression types. Return <see langword="null" /> to support all expression types. (This is only sensible when
    /// <typeparamref name="T"/> is <see cref="Expression"/>.)
    /// </value>
    ExpressionType[] SupportedExpressionTypes { get; }

    /// <summary>
    /// Transforms a given <see cref="Expression"/>. If the implementation can handle the <see cref="Expression"/>,
    /// it should return a new, transformed <see cref="Expression"/> instance. Otherwise, it should return the input
    /// <paramref name="expression"/> instance.
    /// </summary>
    /// <param name="expression">The expression to be transformed.</param>
    /// <returns>The result of the transformation, or <paramref name="expression"/> if no transformation was applied.</returns>
    Expression Transform (T expression);
  }
}
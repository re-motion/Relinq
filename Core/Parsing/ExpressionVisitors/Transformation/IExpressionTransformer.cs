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
using System.Linq.Expressions;

namespace Remotion.Linq.Parsing.ExpressionVisitors.Transformation
{
  /// <summary>
  /// <see cref="IExpressionTransformer{T}"/> is implemented by classes that transform <see cref="Expression"/> instances. The 
  /// <see cref="ExpressionTransformerRegistry"/> manages registration of <see cref="IExpressionTransformer{T}"/> instances, and the 
  /// <see cref="TransformingExpressionVisitor"/> applies the transformations.
  /// </summary>
  /// <typeparam name="T">The type of expressions handled by this <see cref="IExpressionTransformer{T}"/> implementation.</typeparam>
  /// <remarks>
  /// <para>
  /// <see cref="IExpressionTransformer{T}"/> is a convenience interface that provides strong typing, whereas 
  /// <see cref="ExpressionTransformation"/> only operates on <see cref="Expression"/> instances. 
  /// </para>
  /// <para>
  /// <see cref="IExpressionTransformer{T}"/> can be used together with the <see cref="TransformingExpressionVisitor"/> class by using the 
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
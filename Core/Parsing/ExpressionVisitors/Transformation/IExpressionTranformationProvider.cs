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

namespace Remotion.Linq.Parsing.ExpressionVisitors.Transformation
{
  /// <summary>
  /// <see cref="IExpressionTranformationProvider"/> defines an API for classes returning <see cref="ExpressionTransformation"/> instances for specific 
  /// <see cref="Expression"/> objects. Usually, the <see cref="ExpressionTransformerRegistry"/> will be used when an implementation of this
  /// interface is needed.
  /// </summary>
  public interface IExpressionTranformationProvider
  {
    /// <summary>
    /// Gets the transformers for the given <see cref="Expression"/>.
    /// </summary>
    /// <param name="expression">The <see cref="Expression"/> to be transformed.</param>
    /// <returns>
    /// A sequence containing <see cref="ExpressionTransformation"/> objects that should be applied to the <paramref name="expression"/>. Must not
    /// be <see langword="null" />.
    /// </returns>
    IEnumerable<ExpressionTransformation> GetTransformations (Expression expression);
  }
}
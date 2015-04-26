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
using Remotion.Linq.Parsing.ExpressionVisitors;
using Remotion.Linq.Parsing.ExpressionVisitors.Transformation;
using Remotion.Utilities;

namespace Remotion.Linq.Parsing.Structure.ExpressionTreeProcessors
{
  /// <summary>
  /// Applies a given set of transformations to an <see cref="Expression"/> tree. The transformations are provided by an instance of
  /// <see cref="IExpressionTranformationProvider"/> (eg., <see cref="ExpressionTransformerRegistry"/>).
  /// </summary>
  /// <remarks>
  /// The <see cref="TransformingExpressionTreeProcessor"/> uses the <see cref="TransformingExpressionVisitor"/> to apply the transformations.
  /// It performs a single visiting run over the <see cref="Expression"/> tree.
  /// </remarks>
  public sealed class TransformingExpressionTreeProcessor : IExpressionTreeProcessor
  {
    private readonly IExpressionTranformationProvider _provider;

    /// <summary>
    /// Initializes a new instance of the <see cref="TransformingExpressionTreeProcessor"/> class.
    /// </summary>
    /// <param name="provider">A class providing the transformations to apply to the tree, eg., an instance of 
    /// <see cref="ExpressionTransformerRegistry"/>.</param>
    public TransformingExpressionTreeProcessor (IExpressionTranformationProvider provider)
    {
      ArgumentUtility.CheckNotNull ("provider", provider);

      _provider = provider;
    }

    public IExpressionTranformationProvider Provider
    {
      get { return _provider; }
    }

    public Expression Process (Expression expressionTree)
    {
      ArgumentUtility.CheckNotNull ("expressionTree", expressionTree);

      return TransformingExpressionVisitor.Transform (expressionTree, _provider);
    }
  }
}
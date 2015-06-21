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
using Remotion.Linq.Parsing.ExpressionVisitors.TreeEvaluation;
using Remotion.Utilities;

namespace Remotion.Linq.Parsing.Structure.ExpressionTreeProcessors
{
  /// <summary>
  /// Analyzes an <see cref="Expression"/> tree for sub-trees that are evaluatable in-memory, and evaluates those sub-trees.
  /// </summary>
  /// <remarks>
  /// The <see cref="PartialEvaluatingExpressionTreeProcessor"/> uses the <see cref="PartialEvaluatingExpressionVisitor"/> for partial evaluation.
  /// It performs two visiting runs over the <see cref="Expression"/> tree.
  /// </remarks>
  public sealed class PartialEvaluatingExpressionTreeProcessor : IExpressionTreeProcessor
  {
    private readonly IEvaluatableExpressionFilter _filter;

    public PartialEvaluatingExpressionTreeProcessor (IEvaluatableExpressionFilter filter)
    {
      ArgumentUtility.CheckNotNull ("filter", filter);

      _filter = filter;
    }

    public IEvaluatableExpressionFilter Filter
    {
      get { return _filter; }
    }

    public Expression Process (Expression expressionTree)
    {
      ArgumentUtility.CheckNotNull ("expressionTree", expressionTree);

      return PartialEvaluatingExpressionVisitor.EvaluateIndependentSubtrees (expressionTree, _filter);
    }
  }
}
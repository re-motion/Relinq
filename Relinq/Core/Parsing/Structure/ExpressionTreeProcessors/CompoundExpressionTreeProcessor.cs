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
using System.Linq;
using System.Linq.Expressions;
using Remotion.Utilities;

namespace Remotion.Linq.Parsing.Structure.ExpressionTreeProcessors
{
  /// <summary>
  /// Implements <see cref="IExpressionTreeProcessor"/> by storing a list of inner <see cref="IExpressionTreeProcessor"/> instances.
  /// The <see cref="Process"/> method calls each inner instance in the order defined by the <see cref="InnerProcessors"/> property. This is an
  /// implementation of the Composite Pattern.
  /// </summary>
  public sealed class CompoundExpressionTreeProcessor : IExpressionTreeProcessor
  {
    private readonly List<IExpressionTreeProcessor> _innerProcessors;

    public CompoundExpressionTreeProcessor (IEnumerable<IExpressionTreeProcessor> innerProcessors)
    {
      ArgumentUtility.CheckNotNull ("innerProcessors", innerProcessors);
      _innerProcessors = new List<IExpressionTreeProcessor> (innerProcessors);
    }

    public IList<IExpressionTreeProcessor> InnerProcessors
    {
      get { return _innerProcessors; }
    }
    
    public Expression Process (Expression expressionTree)
    {
      ArgumentUtility.CheckNotNull ("expressionTree", expressionTree);
      return _innerProcessors.Aggregate (expressionTree, (expr, processor) => processor.Process (expr));
    }
  }
}
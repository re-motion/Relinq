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
using Remotion.Linq.Parsing.ExpressionTreeVisitors;
using Remotion.Utilities;

namespace Remotion.Linq.Parsing.Structure.IntermediateModel
{
  /// <summary>
  /// Resolves an expression using <see cref="IExpressionNode.Resolve"/>, removing transparent identifiers and detecting subqueries
  /// in the process. This is used by methods such as <see cref="SelectExpressionNode.GetResolvedSelector"/>, which are
  /// used when a clause is created from an <see cref="IExpressionNode"/>.
  /// </summary>
  public class ExpressionResolver
  {
    public ExpressionResolver (IExpressionNode currentNode)
    {
      ArgumentUtility.CheckNotNull ("currentNode", currentNode);

      CurrentNode = currentNode;
    }

    public IExpressionNode CurrentNode { get; set; }

    public Expression GetResolvedExpression (
        Expression unresolvedExpression, ParameterExpression parameterToBeResolved, ClauseGenerationContext clauseGenerationContext)
    {
      ArgumentUtility.CheckNotNull ("unresolvedExpression", unresolvedExpression);
      ArgumentUtility.CheckNotNull ("parameterToBeResolved", parameterToBeResolved);

      var sourceNode = CurrentNode.Source;
      var resolvedExpression = sourceNode.Resolve (parameterToBeResolved, unresolvedExpression, clauseGenerationContext);
      resolvedExpression = TransparentIdentifierRemovingExpressionTreeVisitor.ReplaceTransparentIdentifiers (resolvedExpression);
      return resolvedExpression;
    }
  }
}

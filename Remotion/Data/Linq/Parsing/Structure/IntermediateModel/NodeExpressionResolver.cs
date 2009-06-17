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
using System.Collections.Generic;
using System.Linq.Expressions;
using Remotion.Data.Linq.Parsing.ExpressionTreeVisitors;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.Structure.IntermediateModel
{
  public class NodeExpressionResolver
  {
    private Expression _cachedResolvedExpression;

    public NodeExpressionResolver (IExpressionNode sourceNode)
    {
      ArgumentUtility.CheckNotNull ("sourceNode", sourceNode);

      SourceNode = sourceNode;
    }

    public IExpressionNode SourceNode { get; set; }

    public Expression GetResolvedExpression (Expression unresolvedExpression, ParameterExpression parameterToBeResolved, ClauseGenerationContext clauseGenerationContext)
    {
      ArgumentUtility.CheckNotNull ("unresolvedExpression", unresolvedExpression);
      ArgumentUtility.CheckNotNull ("parameterToBeResolved", parameterToBeResolved);

      if (_cachedResolvedExpression == null)
      {
        _cachedResolvedExpression = SourceNode.Resolve (parameterToBeResolved, unresolvedExpression, clauseGenerationContext);
        _cachedResolvedExpression = TransparentIdentifierRemovingVisitor.ReplaceTransparentIdentifiers (_cachedResolvedExpression);
        _cachedResolvedExpression = SubQueryFindingVisitor.ReplaceSubQueries (
            _cachedResolvedExpression, 
            clauseGenerationContext.NodeTypeRegistry, 
            clauseGenerationContext.SubQueryRegistry);
      }

      return _cachedResolvedExpression;
    }

    public Expression GetResolvedExpression (Func<Expression> unresolvedExpressionGenerator, ParameterExpression parameterToBeResolved, ClauseGenerationContext clauseGenerationContext)
    {
      ArgumentUtility.CheckNotNull ("unresolvedExpressionGenerator", unresolvedExpressionGenerator);
      ArgumentUtility.CheckNotNull ("parameterToBeResolved", parameterToBeResolved);

      if (_cachedResolvedExpression == null)
        return GetResolvedExpression (unresolvedExpressionGenerator (), parameterToBeResolved, clauseGenerationContext);

      return _cachedResolvedExpression;
    }
  }
}
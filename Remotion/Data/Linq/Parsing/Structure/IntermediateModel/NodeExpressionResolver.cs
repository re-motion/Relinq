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

    public Expression GetResolvedExpression (Expression unresolvedExpression, ParameterExpression parameterToBeResolved, QuerySourceClauseMapping querySourceClauseMapping)
    {
      ArgumentUtility.CheckNotNull ("unresolvedExpression", unresolvedExpression);
      ArgumentUtility.CheckNotNull ("parameterToBeResolved", parameterToBeResolved);
      ArgumentUtility.CheckNotNull ("querySourceClauseMapping", querySourceClauseMapping);

      if (_cachedResolvedExpression == null)
      {
        _cachedResolvedExpression = SourceNode.Resolve (parameterToBeResolved, unresolvedExpression, querySourceClauseMapping);
        _cachedResolvedExpression = TransparentIdentifierRemovingVisitor.ReplaceTransparentIdentifiers (_cachedResolvedExpression);
      }

      return _cachedResolvedExpression;
    }

    public Expression GetResolvedExpression (Func<Expression> unresolvedExpressionGenerator, ParameterExpression parameterToBeResolved, QuerySourceClauseMapping querySourceClauseMapping)
    {
      ArgumentUtility.CheckNotNull ("unresolvedExpressionGenerator", unresolvedExpressionGenerator);
      ArgumentUtility.CheckNotNull ("parameterToBeResolved", parameterToBeResolved);
      ArgumentUtility.CheckNotNull ("querySourceClauseMapping", querySourceClauseMapping);

      if (_cachedResolvedExpression == null)
        return GetResolvedExpression (unresolvedExpressionGenerator(), parameterToBeResolved, querySourceClauseMapping);

      return _cachedResolvedExpression;
    }
  }
}
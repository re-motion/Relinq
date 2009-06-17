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
  /// <summary>
  /// Resolves an expression using <see cref="IExpressionNode.Resolve"/>, removing transparent identifiers and detecting subqueries
  /// in the process. This is used by methods such as <see cref="SelectExpressionNode.GetResolvedSelector"/>, which are
  /// used when a clause is created from an <see cref="IExpressionNode"/>.
  /// </summary>
  public class ExpressionResolver
  {
    public ExpressionResolver (IExpressionNode sourceNode)
    {
      ArgumentUtility.CheckNotNull ("sourceNode", sourceNode);

      SourceNode = sourceNode;
    }

    public IExpressionNode SourceNode { get; set; }

    public Expression GetResolvedExpression (Expression unresolvedExpression, ParameterExpression parameterToBeResolved, ClauseGenerationContext clauseGenerationContext)
    {
      ArgumentUtility.CheckNotNull ("unresolvedExpression", unresolvedExpression);
      ArgumentUtility.CheckNotNull ("parameterToBeResolved", parameterToBeResolved);

      var resolvedExpression = SourceNode.Resolve (parameterToBeResolved, unresolvedExpression, clauseGenerationContext);
      resolvedExpression = TransparentIdentifierRemovingVisitor.ReplaceTransparentIdentifiers (resolvedExpression);
      resolvedExpression = SubQueryFindingVisitor.ReplaceSubQueries (
          resolvedExpression, 
          clauseGenerationContext.NodeTypeRegistry, 
          clauseGenerationContext.SubQueryRegistry);

      return resolvedExpression;
    }
  }
}
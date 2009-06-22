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
using System.Collections.Generic;
using Remotion.Data.Linq.Clauses;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.Structure.IntermediateModel
{
  /// <summary>
  /// Encapsulates contextual information used while generating clauses from <see cref="IExpressionNode"/> instances.
  /// </summary>
  public struct ClauseGenerationContext
  {
    public ClauseGenerationContext (
        QuerySourceClauseMapping clauseMapping, 
        MethodCallExpressionNodeTypeRegistry nodeTypeRegistry, 
        SubQueryRegistry subQueryRegistry,
        ResultModificationExpressionNodeRegistry resultModificationNodeRegistry)
      : this()
    {
      ArgumentUtility.CheckNotNull ("clauseMapping", clauseMapping);
      ArgumentUtility.CheckNotNull ("nodeTypeRegistry", nodeTypeRegistry);
      ArgumentUtility.CheckNotNull ("subQueryRegistry", subQueryRegistry);
      ArgumentUtility.CheckNotNull ("resultModificationNodeRegistry", resultModificationNodeRegistry);

      ClauseMapping = clauseMapping;
      NodeTypeRegistry = nodeTypeRegistry;
      SubQueryRegistry = subQueryRegistry;
      ResultModificationNodeRegistry = resultModificationNodeRegistry;
    }

    public QuerySourceClauseMapping ClauseMapping { get; private set; }
    public MethodCallExpressionNodeTypeRegistry NodeTypeRegistry { get; private set; }
    public SubQueryRegistry SubQueryRegistry { get; private set; }
    public ResultModificationExpressionNodeRegistry ResultModificationNodeRegistry { get; private set; }
  }
}
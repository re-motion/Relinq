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
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.Structure.IntermediateModel
{
  /// <summary>
  /// Encapsulates contextual information used while generating clauses from <see cref="IExpressionNode"/> instances.
  /// </summary>
  public struct ClauseGenerationContext
  {
    private readonly Dictionary<IExpressionNode, object> _lookup;

    public ClauseGenerationContext (
        MethodCallExpressionNodeTypeRegistry nodeTypeRegistry)
        : this()
    {
      ArgumentUtility.CheckNotNull ("nodeTypeRegistry", nodeTypeRegistry);

      NodeTypeRegistry = nodeTypeRegistry;
      _lookup = new Dictionary<IExpressionNode, object> ();
    }

    public MethodCallExpressionNodeTypeRegistry NodeTypeRegistry { get; private set; }

    public int Count
    {
      get { return _lookup.Count; }
    }

    public void AddContextInfo (IExpressionNode node, object contextInfo)
    {
      ArgumentUtility.CheckNotNull ("node", node);
      ArgumentUtility.CheckNotNull ("contextInfo", contextInfo);

      try
      {
        _lookup.Add (node, contextInfo);
      }
      catch (ArgumentException)
      {
        throw new InvalidOperationException ("Node already has associated context info.");
      }
    }

    public object GetContextInfo (IExpressionNode node)
    {
      ArgumentUtility.CheckNotNull ("node", node);

      object contextInfo;
      if (!_lookup.TryGetValue (node, out contextInfo))
        throw new KeyNotFoundException ("Node has no associated context info.");

      return contextInfo;
    }
  }
}
// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// re-linq is free software; you can redistribute it and/or modify it under 
// the terms of the GNU Lesser General Public License as published by the 
// Free Software Foundation; either version 2.1 of the License, 
// or (at your option) any later version.
// 
// re-linq is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-linq; if not, see http://www.gnu.org/licenses.
// 
using System;
using System.Collections.Generic;
using Remotion.Linq.Utilities;

namespace Remotion.Linq.Parsing.Structure.IntermediateModel
{
  /// <summary>
  /// Encapsulates contextual information used while generating clauses from <see cref="IExpressionNode"/> instances.
  /// </summary>
  public struct ClauseGenerationContext
  {
    private readonly Dictionary<IExpressionNode, object> _lookup;
    private readonly INodeTypeProvider _nodeTypeProvider;

    public ClauseGenerationContext (
        INodeTypeProvider nodeTypeProvider)
        : this()
    {
      ArgumentUtility.CheckNotNull ("nodeTypeProvider", nodeTypeProvider);

      _lookup = new Dictionary<IExpressionNode, object> ();
      _nodeTypeProvider = nodeTypeProvider;
    }

    public INodeTypeProvider NodeTypeProvider
    {
      get { return _nodeTypeProvider; }
    }

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

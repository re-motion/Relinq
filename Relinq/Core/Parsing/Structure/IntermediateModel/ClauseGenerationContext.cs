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
using Remotion.Utilities;

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

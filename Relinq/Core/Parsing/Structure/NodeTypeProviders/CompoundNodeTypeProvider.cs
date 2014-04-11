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
using System.Reflection;
using Remotion.Utilities;

namespace Remotion.Linq.Parsing.Structure.NodeTypeProviders
{
  /// <summary>
  /// Implements <see cref="INodeTypeProvider"/> by storing a list of inner <see cref="INodeTypeProvider"/> instances.
  /// The <see cref="IsRegistered"/> and <see cref="GetNodeType"/> methods delegate to these inner instances. This is an
  /// implementation of the Composite Pattern.
  /// </summary>
  public sealed class CompoundNodeTypeProvider : INodeTypeProvider
  {
    private readonly List<INodeTypeProvider> _innerProviders;

    public CompoundNodeTypeProvider (IEnumerable<INodeTypeProvider> innerProviders)
    {
      ArgumentUtility.CheckNotNull ("innerProviders", innerProviders);
      _innerProviders = new List<INodeTypeProvider> (innerProviders);
    }

    public IList<INodeTypeProvider> InnerProviders
    {
      get { return _innerProviders; }
    }

    public bool IsRegistered (MethodInfo method)
    {
      ArgumentUtility.CheckNotNull ("method", method);

      return _innerProviders.Any (p => p.IsRegistered (method));
    }

    public Type GetNodeType (MethodInfo method)
    {
      ArgumentUtility.CheckNotNull ("method", method);

      return InnerProviders.Select (p => p.GetNodeType (method)).FirstOrDefault (t => t != null);
    }
  }
}
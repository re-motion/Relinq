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
using System.Reflection;
using Remotion.Linq.Collections;
using Remotion.Linq.Parsing.Structure.IntermediateModel;
using Remotion.Utilities;

namespace Remotion.Linq.Parsing.Structure.NodeTypeProviders
{
  /// <summary>
  /// Maps the <see cref="MethodInfo"/> objects used in <see cref="MethodCallExpression"/> objects to the respective <see cref="IExpressionNode"/>
  /// types based on the method names and a filter (as defined by <see cref="NameBasedRegistrationInfo"/>). 
  /// This is used by <see cref="ExpressionTreeParser"/> when a <see cref="MethodCallExpression"/> is encountered to instantiate the right 
  /// <see cref="IExpressionNode"/> for the given method.
  /// </summary>
  public sealed class MethodNameBasedNodeTypeRegistry : INodeTypeProvider
  {
    /// <summary>
    /// Creates a <see cref="MethodNameBasedNodeTypeRegistry"/> and registers all relevant <see cref="IExpressionNode"/> implementations in the <b>Remotion.Linq</b> assembly.
    /// </summary>
    /// <returns>
    /// A <see cref="MethodInfoBasedNodeTypeRegistry"/> with all <see cref="IExpressionNode"/> types in the <b>Remotion.Linq</b> assembly registered.
    /// </returns>
    public static MethodNameBasedNodeTypeRegistry CreateFromRelinqAssembly ()
    {
      var registry = new MethodNameBasedNodeTypeRegistry();

      registry.Register (ContainsExpressionNode.GetSupportedMethodNames(), typeof (ContainsExpressionNode));

      return registry;
    }

    private readonly IDictionary<string, ICollection<KeyValuePair<NameBasedRegistrationInfo, Type>>> _registeredNamedTypes =
        new Dictionary<string, ICollection<KeyValuePair<NameBasedRegistrationInfo, Type>>> ();

    /// <summary>
    /// Returns the count of the registered method names.
    /// </summary>
    public int RegisteredNamesCount
    {
      get { return _registeredNamedTypes.CountValues(); }
    }

    /// <summary>
    /// Registers the given <paramref name="nodeType"/> for the query operator methods defined by the given <see cref="NameBasedRegistrationInfo"/>
    /// objects.
    /// </summary>
    /// <param name="registrationInfo">A sequence of objects defining the methods to register the node type for.</param>
    /// <param name="nodeType">The type of the <see cref="IExpressionNode"/> to register.</param>
    public void Register (IEnumerable<NameBasedRegistrationInfo> registrationInfo, Type nodeType)
    {
      ArgumentUtility.CheckNotNull ("registrationInfo", registrationInfo);
      ArgumentUtility.CheckNotNull ("nodeType", nodeType);

      foreach (var info in registrationInfo)
        _registeredNamedTypes.Add (info.Name, new KeyValuePair<NameBasedRegistrationInfo, Type> (info, nodeType));
    }

   /// <summary>
    /// Determines whether the specified method was registered with this <see cref="MethodInfoBasedNodeTypeRegistry"/>.
    /// </summary>
    public bool IsRegistered (MethodInfo method)
    {
      ArgumentUtility.CheckNotNull ("method", method);

     return GetNodeType (method) != null;
    }
    
    /// <summary>
    /// Gets the type of <see cref="IExpressionNode"/> registered with this <see cref="MethodInfoBasedNodeTypeRegistry"/> instance that
    /// matches the given <paramref name="method"/>, returning <see langword="null" /> if none can be found.
    /// </summary>
    public Type GetNodeType (MethodInfo method)
    {
      ArgumentUtility.CheckNotNull ("method", method);

      ICollection<KeyValuePair<NameBasedRegistrationInfo, Type>> result;
      if (!_registeredNamedTypes.TryGetValue (method.Name, out result))
        return null;
      
      return result.Where (info => info.Key.Filter (method)).Select (info => info.Value).FirstOrDefault();
    }
  }
}

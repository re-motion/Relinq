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
using Remotion.Linq.Parsing.Structure.IntermediateModel;
using Remotion.Utilities;

namespace Remotion.Linq.Parsing.Structure.NodeTypeProviders
{
  /// <summary>
  /// Maps the <see cref="MethodInfo"/> objects used in <see cref="MethodCallExpression"/> objects to the respective <see cref="IExpressionNode"/>
  /// types. This is used by <see cref="ExpressionTreeParser"/> when a <see cref="MethodCallExpression"/> is encountered to instantiate the
  /// right <see cref="IExpressionNode"/> for the given method.
  /// </summary>
  public sealed class MethodInfoBasedNodeTypeRegistry : INodeTypeProvider
  {
    /// <summary>
    /// Creates a <see cref="MethodInfoBasedNodeTypeRegistry"/> and automatically registers all types implementing <see cref="IExpressionNode"/> 
    /// from a given type sequence that offer a public static <c>SupportedMethods</c> field.
    /// </summary>
    /// <returns>A <see cref="MethodInfoBasedNodeTypeRegistry"/> with all <see cref="IExpressionNode"/> types with a <c>SupportedMethods</c>
    /// field registered.</returns>
    public static MethodInfoBasedNodeTypeRegistry CreateFromTypes (IEnumerable<Type> searchedTypes)
    {
      ArgumentUtility.CheckNotNull ("searchedTypes", searchedTypes);

      var expressionNodeTypes = from t in searchedTypes
                                where typeof (IExpressionNode).IsAssignableFrom (t)
                                select t;

      var supportedMethodsForTypes =
          from t in expressionNodeTypes
          let supportedMethodsField = t.GetRuntimeField ("SupportedMethods")
          select new
                 {
                     Type = t,
                     Methods =
                         supportedMethodsField != null && supportedMethodsField.IsStatic
                             ? (IEnumerable<MethodInfo>) supportedMethodsField.GetValue (null)
                             : Enumerable.Empty<MethodInfo>()
                 };

      var registry = new MethodInfoBasedNodeTypeRegistry();

      foreach (var methodsForType in supportedMethodsForTypes)
      {
        registry.Register (methodsForType.Methods, methodsForType.Type);
      }

      return registry;
    }

    /// <summary>
    /// Gets the registerable method definition from a given <see cref="MethodInfo"/>. A registerable method is a <see cref="MethodInfo"/> object
    /// that can be registered via a call to <see cref="Register"/>. When the given <paramref name="method"/> is passed to 
    /// <see cref="GetNodeType"/> and its corresponding registerable method was registered, the correct node type is returned.
    /// </summary>
    /// <param name="method">The method for which the registerable method should be retrieved.</param>
    /// <returns><paramref name="method"/> itself, unless it is a closed generic method or declared in a closed generic type. In the latter cases,
    /// the corresponding generic method definition respectively the method declared in a generic type definition is returned.</returns>
    public static MethodInfo GetRegisterableMethodDefinition (MethodInfo method)
    {
      var genericMethodDefinition = method.IsGenericMethod ? method.GetGenericMethodDefinition () : method;
      if (genericMethodDefinition.DeclaringType.GetTypeInfo().IsGenericType)
      {
        var declaringTypeDefinition = genericMethodDefinition.DeclaringType.GetGenericTypeDefinition ();

        // find corresponding method on the generic type definition

        // TODO RM-6131: New implementation still needs to check parameter types for exact match.
        // TODO RM-6131: New implementation will need to be cached (probably).
        // Original: return (MethodInfo) MethodBase.GetMethodFromHandle (genericMethodDefinition.MethodHandle, declaringTypeDefinition.TypeHandle);
        return declaringTypeDefinition.GetRuntimeMethods()
            .Single (mi => mi.Name == genericMethodDefinition.Name && mi.GetParameters().Length == genericMethodDefinition.GetParameters().Length);
      }
      else
      {
        return genericMethodDefinition;
      }
    }

    private readonly Dictionary<MethodInfo, Type> _registeredMethodInfoTypes = new Dictionary<MethodInfo, Type>();

    /// <summary>
    /// Returns the count of the registered <see cref="MethodInfo"/>s.
    /// </summary>
    public int RegisteredMethodInfoCount
    {
      get { return _registeredMethodInfoTypes.Count; }
    }

    /// <summary>
    /// Registers the specific <paramref name="methods"/> with the given <paramref name="nodeType"/>. The given methods must either be non-generic
    /// or open generic method definitions. If a method has already been registered before, the later registration overwrites the earlier one.
    /// </summary>
    public void Register (IEnumerable<MethodInfo> methods, Type nodeType)
    {
      ArgumentUtility.CheckNotNull ("methods", methods);
      ArgumentUtility.CheckNotNull ("nodeType", nodeType);
      ArgumentUtility.CheckTypeIsAssignableFrom ("nodeType", nodeType, typeof (IExpressionNode));

      foreach (var method in methods)
      {
        if (method.IsGenericMethod && !method.IsGenericMethodDefinition)
        {
          var message = string.Format (
              "Cannot register closed generic method '{0}', try to register its generic method definition instead.", method.Name);
          throw new InvalidOperationException (message);
        }

        if (method.DeclaringType.GetTypeInfo().IsGenericType && !method.DeclaringType.GetTypeInfo().IsGenericTypeDefinition)
        {
          var message = string.Format (
              "Cannot register method '{0}' in closed generic type '{1}', try to register its equivalent in the generic type definition instead.", 
              method.Name,
              method.DeclaringType);
          throw new InvalidOperationException (message);
        }

        _registeredMethodInfoTypes[method] = nodeType;
      }
    }

   /// <summary>
    /// Determines whether the specified method was registered with this <see cref="MethodInfoBasedNodeTypeRegistry"/>.
    /// </summary>
    public bool IsRegistered (MethodInfo method)
    {
      ArgumentUtility.CheckNotNull ("method", method);

      var methodDefinition = GetRegisterableMethodDefinition (method);
      return _registeredMethodInfoTypes.ContainsKey (methodDefinition);
    }
    
    /// <summary>
    /// Gets the type of <see cref="IExpressionNode"/> registered with this <see cref="MethodInfoBasedNodeTypeRegistry"/> instance that
    /// matches the given <paramref name="method"/>, returning <see langword="null" /> if none can be found.
    /// </summary>
    public Type GetNodeType (MethodInfo method)
    {
      ArgumentUtility.CheckNotNull ("method", method);

      var methodDefinition = GetRegisterableMethodDefinition (method);
      
      Type result;
      _registeredMethodInfoTypes.TryGetValue (methodDefinition, out result);
      return result;
    }
  }
}

// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// as published by the Free Software Foundation; either version 2.1 of the 
// License, or (at your option) any later version.
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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Data.Linq.Parsing.Structure.IntermediateModel;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.Parsing.Structure
{
  /// <summary>
  /// Maps the <see cref="MethodInfo"/> objects used in <see cref="MethodCallExpression"/> objects to the respective <see cref="IExpressionNode"/>
  /// types. This is used by <see cref="ExpressionTreeParser"/> when a <see cref="MethodCallExpression"/> is encountered to instantiate the
  /// right <see cref="IExpressionNode"/> for the given method.
  /// </summary>
  public class MethodCallExpressionNodeTypeRegistry : IMethodCallExpressionNodeTypeProvider
  {
    /// <summary>
    /// Creates a default <see cref="MethodCallExpressionNodeTypeRegistry"/>, which has all types implementing <see cref="IExpressionNode"/> from the
    /// re-linq assembly automatically registered, as long as they offer a public static <c>SupportedMethods</c> field.
    /// </summary>
    /// <returns>A default <see cref="MethodCallExpressionNodeTypeRegistry"/> with all <see cref="IExpressionNode"/> types with a <c>SupportedMethods</c>
    /// field registered.</returns>
    public static MethodCallExpressionNodeTypeRegistry CreateDefault ()
    {
      var expressionNodeTypes = from t in typeof (MethodCallExpressionNodeTypeRegistry).Assembly.GetTypes()
                                where typeof (IExpressionNode).IsAssignableFrom (t)
                                select t;

      var supportedMethodsForTypes = from t in expressionNodeTypes
                                     let supportedMethodsField = t.GetField ("SupportedMethods", BindingFlags.Static | BindingFlags.Public)
                                     let supportedMethodNamesField = t.GetField ("SupportedMethodNames", BindingFlags.Static | BindingFlags.Public)
                                     select new { 
                                        Type = t,
                                        Methods = 
                                            supportedMethodsField != null
                                               ? (IEnumerable<MethodInfo>) supportedMethodsField.GetValue (null)
                                               : Enumerable.Empty<MethodInfo>(),
                                        MethodNames =
                                            supportedMethodNamesField != null
                                                ? ((IEnumerable<string>) supportedMethodNamesField.GetValue (null))
                                                : Enumerable.Empty<string>()
                                     };

      var registry = new MethodCallExpressionNodeTypeRegistry();

      foreach (var methodsForType in supportedMethodsForTypes)
      {
        registry.Register (methodsForType.Methods, methodsForType.Type);
        registry.Register (methodsForType.MethodNames, methodsForType.Type);
      }

      return registry;
    }

    /// <summary>
    /// Gets the registerable method definition from a given <see cref="MethodInfo"/>. A registerable method is a <see cref="MethodInfo"/> object
    /// that can be registered via a call to <see cref="O:Register"/>. When the given <paramref name="method"/> is passed to 
    /// <see cref="GetNodeType"/> and its corresponding registerable method was registered, the correct node type is returned.
    /// </summary>
    /// <param name="method">The method for which the registerable method should be retrieved.</param>
    /// <returns><paramref name="method"/> itself, unless it is a closed generic method or declared in a closed generic type. In the latter cases,
    /// the corresponding generic method definition respectively the method declared in a generic type definition is returned.</returns>
    public static MethodInfo GetRegisterableMethodDefinition (MethodInfo method)
    {
      var genericMethodDefinition = method.IsGenericMethod ? method.GetGenericMethodDefinition () : method;
      if (genericMethodDefinition.DeclaringType.IsGenericType)
      {
        var declaringTypeDefinition = genericMethodDefinition.DeclaringType.GetGenericTypeDefinition ();

        // find corresponding method on the generic type definition
        return (MethodInfo) MethodBase.GetMethodFromHandle (genericMethodDefinition.MethodHandle, declaringTypeDefinition.TypeHandle);
      }
      else
      {
        return genericMethodDefinition;
      }
    }

    private readonly Dictionary<MethodInfo, Type> _registeredMethodInfoTypes = new Dictionary<MethodInfo, Type>();
    private readonly Dictionary<string, Type> _registeredNamedTypes = new Dictionary<string, Type>();

    /// <summary>
    /// Returns the count of the registered <see cref="MethodInfo"/>s.
    /// </summary>
    public int RegisteredMethodInfoCount
    {
      get { return _registeredMethodInfoTypes.Count; }
    }

    /// <summary>
    /// Returns the count of the registered method names.
    /// </summary>
    public int RegisteredNamesCount
    {
      get { return _registeredNamedTypes.Count; }
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

        if (method.DeclaringType.IsGenericType && !method.DeclaringType.IsGenericTypeDefinition)
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
    /// Registers the specific method names with the givven <paramref name="nodeType"/>. If a nethod has already been registered before, the later
    /// registrations overwrites the earlier one.
    /// </summary>
    /// <param name="methodNames"></param>
    /// <param name="nodeType"></param>
    public void Register (IEnumerable<string> methodNames, Type nodeType)
    {
      ArgumentUtility.CheckNotNull ("methodNames", methodNames);
      ArgumentUtility.CheckNotNull ("nodeType", nodeType);

      foreach (var methodName in methodNames)
        _registeredNamedTypes[methodName] = nodeType;
    }

   /// <summary>
    /// Determines whether the specified method was registered with this <see cref="MethodCallExpressionNodeTypeRegistry"/>.
    /// </summary>
    public bool IsRegistered (MethodInfo method)
    {
      ArgumentUtility.CheckNotNull ("method", method);

      var methodDefinition = GetRegisterableMethodDefinition (method);
      return _registeredMethodInfoTypes.ContainsKey (methodDefinition) || _registeredNamedTypes.ContainsKey(methodDefinition.Name);
    }
    
    /// <summary>
    /// Gets the type of <see cref="IExpressionNode"/> registered with this <see cref="MethodCallExpressionNodeTypeRegistry"/> instance that
    /// matches the given <paramref name="method"/>, throwing a <see cref="KeyNotFoundException"/> if none can be found.
    /// </summary>
    public Type GetNodeType (MethodInfo method)
    {
      ArgumentUtility.CheckNotNull ("method", method);

      var methodDefinition = GetRegisterableMethodDefinition (method);
      
      Type result;
      if (_registeredMethodInfoTypes.TryGetValue (methodDefinition, out result))
        return result;

      if (_registeredNamedTypes.TryGetValue (methodDefinition.Name, out result))
        return result;

      string message = string.Format (
          "No corresponding expression node type was registered for method '{0}.{1}'.",
          methodDefinition.DeclaringType.FullName,
          methodDefinition.Name);
      throw new KeyNotFoundException (message);
    }
  }
}

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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Data.Linq.Parsing.Structure.IntermediateModel;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.Structure
{
  /// <summary>
  /// Maps the <see cref="MethodInfo"/> objects used in <see cref="MethodCallExpression"/> objects to the respective <see cref="IExpressionNode"/>
  /// types. This is used by <see cref="ExpressionTreeParser"/> when a <see cref="MethodCallExpression"/> is encountered to instantiate the
  /// right <see cref="IExpressionNode"/> for the given method.
  /// </summary>
  public class MethodCallExpressionNodeTypeRegistry
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
                                     where supportedMethodsField != null
                                     select new { Type = t, Methods = (IEnumerable<MethodInfo>) supportedMethodsField.GetValue (null) };

      var registry = new MethodCallExpressionNodeTypeRegistry();
      foreach (var methodsForType in supportedMethodsForTypes)
        registry.Register (methodsForType.Methods, methodsForType.Type);

      return registry;
    }

    private readonly Dictionary<MethodInfo, Type> _registeredTypes = new Dictionary<MethodInfo, Type>();

    public int Count
    {
      get { return _registeredTypes.Count; }
    }

    /// <summary>
    /// Registers the specified <paramref name="methods"/> with the given <paramref name="nodeType"/>. The given methods must either be non-generic
    /// or open generic method definitions. If a method has already been registered before, the later registration overwrites the earlier one.
    /// </summary>
    public void Register (IEnumerable<MethodInfo> methods, Type nodeType)
    {
      ArgumentUtility.CheckNotNull ("methods", methods);
      ArgumentUtility.CheckNotNullAndTypeIsAssignableFrom ("nodeType", nodeType, typeof (IExpressionNode));

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

        _registeredTypes[method] = nodeType;
      }
    }

    /// <summary>
    /// Determines whether the specified method was registered with this <see cref="MethodCallExpressionNodeTypeRegistry"/>.
    /// </summary>
    public bool IsRegistered (MethodInfo method)
    {
      ArgumentUtility.CheckNotNull ("method", method);

      var methodDefinition = GetMethodDefinition (method);
      return _registeredTypes.ContainsKey (methodDefinition);
    }

    /// <summary>
    /// Gets the type of <see cref="IExpressionNode"/> registered with this <see cref="MethodCallExpressionNodeTypeRegistry"/> instance that
    /// matches the given <paramref name="method"/>, throwing a <see cref="KeyNotFoundException"/> if none can be found.
    /// </summary>
    public Type GetNodeType (MethodInfo method)
    {
      ArgumentUtility.CheckNotNull ("method", method);

      var methodDefinition = GetMethodDefinition (method);
      try
      {
        return _registeredTypes[methodDefinition];
      }
      catch (KeyNotFoundException ex)
      {
        string message = string.Format (
            "No corresponding expression node type was registered for method '{0}.{1}'.",
            methodDefinition.DeclaringType.FullName,
            methodDefinition.Name);
        throw new KeyNotFoundException (message, ex);
      }
    }

    private MethodInfo GetMethodDefinition (MethodInfo method)
    {
      var genericMethodDefinition = method.IsGenericMethod ? method.GetGenericMethodDefinition() : method;
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
  }
}
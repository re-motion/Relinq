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
  public class MethodCallExpressionNodeTypeRegistry : RegistryBase<MethodCallExpressionNodeTypeRegistry, MethodInfo, Type, IExpressionNode>
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="MethodCallExpressionNodeTypeRegistry"/> class. Use 
    /// <see cref="RegistryBase{TRegistry,TKey,TItem,TAssignable}.CreateDefault"/> to create an instance pre-initialized with the default types instead.
    /// </summary>
    public MethodCallExpressionNodeTypeRegistry ()
    {
      
    }

    /// <summary>
    /// Gets the registerable method definition from a given <see cref="MethodInfo"/>. A registerable method is a <see cref="MethodInfo"/> object
    /// that can be registered via a call to <see cref="Register"/>. When the given <paramref name="method"/> is passed to 
    /// <see cref="GetItem"/> and its corresponding registerable method was registered, the correct node type is returned.
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

    /// <summary>
    /// Registers the specified <paramref name="methods"/> with the given <paramref name="nodeType"/>. The given methods must either be non-generic
    /// or open generic method definitions. If a method has already been registered before, the later registration overwrites the earlier one.
    /// </summary>
    public override void Register (IEnumerable<MethodInfo> methods, Type nodeType)
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

        Register (method, nodeType);
      }
    }

    /// <summary>
    /// Gets the type of <see cref="IExpressionNode"/> registered with this <see cref="MethodCallExpressionNodeTypeRegistry"/> instance that
    /// matches the given <paramref name="key"/>, throwing a <see cref="KeyNotFoundException"/> if none can be found.
    /// </summary>
    public override Type GetItem (MethodInfo key)
    {
      ArgumentUtility.CheckNotNull ("key", key);

      var methodDefinition = GetRegisterableMethodDefinition (key);
      var nodeType = GetItemExact (methodDefinition);
      if (nodeType != null)
        return nodeType;

      string message = string.Format (
            "No corresponding expression node type was registered for method '{0}.{1}'.",
            methodDefinition.DeclaringType.FullName,
            methodDefinition.Name);
        throw new KeyNotFoundException (message);
    }

    protected override void RegisterForTypes (IEnumerable<Type> itemTypes)
    {
      var supportedMethodsForTypes = from t in itemTypes
                                     let supportedMethodsField = t.GetField ("SupportedMethods", BindingFlags.Static | BindingFlags.Public)
                                     where supportedMethodsField != null
                                     select new { Type = t, Methods = (IEnumerable<MethodInfo>) supportedMethodsField.GetValue (null) };

      foreach (var methodsForType in supportedMethodsForTypes)
        Register (methodsForType.Methods, methodsForType.Type);
    }

    /// <summary>
    /// Determines whether the specified method was registered with this <see cref="MethodCallExpressionNodeTypeRegistry"/>.
    /// </summary>
    public bool IsRegistered (MethodInfo method)
    {
      ArgumentUtility.CheckNotNull ("method", method);

      var methodDefinition = GetRegisterableMethodDefinition (method);
      return GetItemExact (methodDefinition) != null;
    }
  }
}

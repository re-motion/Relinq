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
  /// types based on the method names. This is used by <see cref="ExpressionTreeParser"/> when a <see cref="MethodCallExpression"/> is encountered to 
  /// instantiate the right <see cref="IExpressionNode"/> for the given method.
  /// </summary>
  public class MethodNameBasedNodeTypeRegistry : INodeTypeProvider
  {
    /// <summary>
    /// Creates a default <see cref="MethodInfoBasedNodeTypeRegistry"/>, which has all types implementing <see cref="IExpressionNode"/> from the
    /// re-linq assembly automatically registered, as long as they offer a public static <c>SupportedMethodNames</c> field.
    /// </summary>
    /// <returns>A default <see cref="MethodInfoBasedNodeTypeRegistry"/> with all <see cref="IExpressionNode"/> types with a <c>SupportedMethodNames</c>
    /// field registered.</returns>
    public static MethodNameBasedNodeTypeRegistry CreateDefault ()
    {
      var expressionNodeTypes = from t in typeof (MethodInfoBasedNodeTypeRegistry).Assembly.GetTypes()
                                where typeof (IExpressionNode).IsAssignableFrom (t)
                                select t;

      var supportedMethodsForTypes = from t in expressionNodeTypes
                                     let supportedMethodNamesField = t.GetField ("SupportedMethodNames", BindingFlags.Static | BindingFlags.Public)
                                     select new { 
                                        Type = t,
                                        MethodNames =
                                            supportedMethodNamesField != null
                                                ? ((IEnumerable<string>) supportedMethodNamesField.GetValue (null))
                                                : Enumerable.Empty<string>()
                                     };

      var registry = new MethodNameBasedNodeTypeRegistry();

      foreach (var methodsForType in supportedMethodsForTypes)
      {
        registry.Register (methodsForType.MethodNames, methodsForType.Type);
      }

      return registry;
    }

    private readonly Dictionary<string, Type> _registeredNamedTypes = new Dictionary<string, Type>();

    /// <summary>
    /// Returns the count of the registered method names.
    /// </summary>
    public int RegisteredNamesCount
    {
      get { return _registeredNamedTypes.Count; }
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
    /// Determines whether the specified method was registered with this <see cref="MethodInfoBasedNodeTypeRegistry"/>.
    /// </summary>
    public bool IsRegistered (MethodInfo method)
    {
      ArgumentUtility.CheckNotNull ("method", method);

      return _registeredNamedTypes.ContainsKey(method.Name);
    }
    
    /// <summary>
    /// Gets the type of <see cref="IExpressionNode"/> registered with this <see cref="MethodInfoBasedNodeTypeRegistry"/> instance that
    /// matches the given <paramref name="method"/>, throwing a <see cref="KeyNotFoundException"/> if none can be found.
    /// </summary>
    public Type GetNodeType (MethodInfo method)
    {
      ArgumentUtility.CheckNotNull ("method", method);

      Type result;
      if (_registeredNamedTypes.TryGetValue (method.Name, out result))
        return result;

      string message = string.Format (
          "No corresponding expression node type was registered for method '{0}.{1}'.",
          method.DeclaringType.FullName,
          method.Name);
      throw new KeyNotFoundException (message);
    }
  }
}

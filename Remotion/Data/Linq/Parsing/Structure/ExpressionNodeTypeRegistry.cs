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
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Data.Linq.Parsing.Structure.IntermediateModel;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.Structure
{
  /// <summary>
  /// Maps the <see cref="MethodInfo"/> objects used in <see cref="MethodCallExpression"/> objects to the respective <see cref="IExpressionNode"/>
  /// types.
  /// </summary>
  public class ExpressionNodeTypeRegistry
  {
    private readonly Dictionary<MethodInfo, Type> _registeredTypes = new Dictionary<MethodInfo, Type>();

    public int Count
    {
      get { return _registeredTypes.Count; }
    }

    public void Register (IEnumerable<MethodInfo> methods, Type nodeType)
    {
      ArgumentUtility.CheckNotNull ("methods", methods);
      ArgumentUtility.CheckNotNull ("nodeType", nodeType);

      foreach (var method in methods)
      {
        if (method.IsGenericMethod && !method.IsGenericMethodDefinition)
        {
          var message = string.Format (
              "Cannot register closed generic method '{0}', try to register its generic method definition instead.", method.Name);
          throw new InvalidOperationException (message);
        }

        _registeredTypes[method] = nodeType;
      }
    }

    public Type GetNodeType (MethodInfo method)
    {
      ArgumentUtility.CheckNotNull ("method", method);

      var methodDefinition = method.IsGenericMethod ? method.GetGenericMethodDefinition() : method;
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
  }
}
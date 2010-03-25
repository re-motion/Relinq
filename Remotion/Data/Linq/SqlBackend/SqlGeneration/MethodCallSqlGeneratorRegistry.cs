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
using System.Reflection;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.SqlGeneration
{
  /// <summary>
  /// <see cref="MethodCallSqlGeneratorRegistry"/> is used to register and get <see cref="IMethodCallSqlGenerator"/> instances.
  /// </summary>
  public class MethodCallSqlGeneratorRegistry
  {
    private readonly Dictionary<MethodInfo, IMethodCallSqlGenerator> _generators;

    public MethodCallSqlGeneratorRegistry ()
    {
      _generators = new Dictionary<MethodInfo, IMethodCallSqlGenerator> ();
    }

    public void Register (MethodInfo methodInfo, IMethodCallSqlGenerator generator)
    {
      ArgumentUtility.CheckNotNull ("methodInfo", methodInfo);
      ArgumentUtility.CheckNotNull ("generator", generator);

      // TODO Review 2364: Just use "_generators[methodInfo] = generator", this works in all cases.
      if (!_generators.ContainsKey (methodInfo))
        _generators.Add (methodInfo, generator);
      else
        _generators[methodInfo] = generator;
    }

    public IMethodCallSqlGenerator GetGenerator (MethodInfo methodInfo)
    {
      ArgumentUtility.CheckNotNull ("methodInfo", methodInfo);

      // TODO Review 2364: Move down to throw statement.
      string message = string.Format (
          "The method '{0}.{1}' is not supported by this code generator, and no custom generator has been registered.",
          methodInfo.DeclaringType.FullName,
          methodInfo.Name);

      if (_generators.ContainsKey (methodInfo))
        return _generators[methodInfo];

      if (methodInfo.IsGenericMethod && !methodInfo.IsGenericMethodDefinition)
        return GetGenerator (methodInfo.GetGenericMethodDefinition ());

      // TODO Review 2364: To support generators registered for base methods, add the following:
      //var baseMethod = methodInfo.GetBaseDefinition ();
      //if (baseMethod != methodInfo)
      //  return GetGenerator (baseMethod);
      // TODO Review 2364: Test by registering a generator for object.ToString() and retrieving the generator using int.ToString(). This should not work without this code, but with the code, it should work.

      throw new NotSupportedException(message);
    }
  }
}
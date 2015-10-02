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
using Remotion.Utilities;

namespace Remotion.Linq.Parsing.Structure.IntermediateModel
{
  internal static class SupportedMethodSpecifications
  {
    public static IEnumerable<MethodInfo> WhereNameMatches (this IEnumerable<MethodInfo> input, string name)
    {
      ArgumentUtility.CheckNotNull ("input", input);
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);

      return input.Where (mi => mi.Name == name);
    }

    public static IEnumerable<MethodInfo> WithoutEqualityComparer (this IEnumerable<MethodInfo> input)
    {
      ArgumentUtility.CheckNotNull ("input", input);

      return input.Where (mi => !HasGenericDelegateOfType (mi, typeof (IEqualityComparer<>)));
    }

    public static IEnumerable<MethodInfo> WithoutComparer (this IEnumerable<MethodInfo> input)
    {
      ArgumentUtility.CheckNotNull ("input", input);

      return input.Where (mi => !HasGenericDelegateOfType (mi, typeof (IComparer<>)));
    }

    public static IEnumerable<MethodInfo> WithoutSeedParameter (this IEnumerable<MethodInfo> input)
    {
      ArgumentUtility.CheckNotNull ("input", input);

      return input.Where (mi => mi.GetParameters().Length == 2);
    }

    public static IEnumerable<MethodInfo> WithSeedParameter (this IEnumerable<MethodInfo> input)
    {
      ArgumentUtility.CheckNotNull ("input", input);

      return input.Where (mi => mi.GetParameters().Length == 3 || mi.GetParameters().Length == 4);
    }

    public static IEnumerable<MethodInfo> WithoutResultSelector (this IEnumerable<MethodInfo> input)
    {
      ArgumentUtility.CheckNotNull ("input", input);

      return input.Where (mi => mi.GetParameters().All (p => p.Name != "resultSelector"));
    }

    public static IEnumerable<MethodInfo> WithResultSelector (this IEnumerable<MethodInfo> input)
    {
      ArgumentUtility.CheckNotNull ("input", input);

      return input.Where (mi => mi.GetParameters().Any (p => p.Name == "resultSelector"));
    }

    public static IEnumerable<MethodInfo> WithoutIndexSelector (this IEnumerable<MethodInfo> input, int parameterPosition)
    {
      ArgumentUtility.CheckNotNull ("input", input);

      return input.Where (mi => !HasIndexSelectorParameter (mi, parameterPosition));
    }

    private static bool HasGenericDelegateOfType (MethodInfo methodInfo, Type genericDelegateType)
    {
      return methodInfo.GetParameters()
          .Select (p => p.ParameterType.GetTypeInfo())
          .Any (p => p.IsGenericType && genericDelegateType.GetTypeInfo().IsAssignableFrom (p.GetGenericTypeDefinition().GetTypeInfo()));
    }

    private static bool HasIndexSelectorParameter (MethodInfo methodInfo, int parameterPosition)
    {
      var parameters = methodInfo.GetParameters();
      if (parameters.Length <= parameterPosition)
        return false;

      var selectorType = parameters[parameterPosition].ParameterType.GetTypeInfo();
      // Enumerable taks a Func<...> but Querable takes an Expression<Func<...>>
      if (typeof (Expression).GetTypeInfo().IsAssignableFrom (selectorType))
        selectorType = selectorType.GenericTypeArguments[0].GetTypeInfo();

      return selectorType.GenericTypeArguments[1] == typeof (int);
    }
  }
}
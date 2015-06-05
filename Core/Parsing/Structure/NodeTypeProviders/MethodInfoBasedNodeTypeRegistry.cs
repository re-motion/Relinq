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
    private static readonly Dictionary<MethodInfo, Lazy<MethodInfo[]>> s_genericMethodDefinitionCandidates = 
        new Dictionary<MethodInfo, Lazy<MethodInfo[]>>();

    /// <summary>
    /// Creates a <see cref="MethodInfoBasedNodeTypeRegistry"/> and registers all relevant <see cref="IExpressionNode"/> implementations in the <b>Remotion.Linq</b> assembly.
    /// </summary>
    /// <returns>
    /// A <see cref="MethodInfoBasedNodeTypeRegistry"/> with all <see cref="IExpressionNode"/> types in the <b>Remotion.Linq</b> assembly registered.
    /// </returns>
    public static MethodInfoBasedNodeTypeRegistry CreateFromRelinqAssembly ()
    {
      var registry = new MethodInfoBasedNodeTypeRegistry();

      registry.Register (AggregateExpressionNode.GetSupportedMethods(), typeof (AggregateExpressionNode));
      registry.Register (AggregateFromSeedExpressionNode.GetSupportedMethods(), typeof (AggregateFromSeedExpressionNode));
      registry.Register (AllExpressionNode.GetSupportedMethods(), typeof (AllExpressionNode));
      registry.Register (AnyExpressionNode.GetSupportedMethods(), typeof (AnyExpressionNode));
      registry.Register (AverageExpressionNode.GetSupportedMethods(), typeof (AverageExpressionNode));
      registry.Register (CastExpressionNode.GetSupportedMethods(), typeof (CastExpressionNode));
      registry.Register (ConcatExpressionNode.GetSupportedMethods(), typeof (ConcatExpressionNode));
      registry.Register (ContainsExpressionNode.GetSupportedMethods(), typeof (ContainsExpressionNode));
      registry.Register (CountExpressionNode.GetSupportedMethods(), typeof (CountExpressionNode));
      registry.Register (DefaultIfEmptyExpressionNode.GetSupportedMethods(), typeof (DefaultIfEmptyExpressionNode));
      registry.Register (DistinctExpressionNode.GetSupportedMethods(), typeof (DistinctExpressionNode));
      registry.Register (ExceptExpressionNode.GetSupportedMethods(), typeof (ExceptExpressionNode));
      registry.Register (FirstExpressionNode.GetSupportedMethods(), typeof (FirstExpressionNode));
      registry.Register (GroupByExpressionNode.GetSupportedMethods(), typeof (GroupByExpressionNode));
      registry.Register (GroupByWithResultSelectorExpressionNode.GetSupportedMethods(), typeof (GroupByWithResultSelectorExpressionNode));
      registry.Register (GroupJoinExpressionNode.GetSupportedMethods(), typeof (GroupJoinExpressionNode));
      registry.Register (IntersectExpressionNode.GetSupportedMethods(), typeof (IntersectExpressionNode));
      registry.Register (JoinExpressionNode.GetSupportedMethods(), typeof (JoinExpressionNode));
      registry.Register (LastExpressionNode.GetSupportedMethods(), typeof (LastExpressionNode));
      registry.Register (LongCountExpressionNode.GetSupportedMethods(), typeof (LongCountExpressionNode));
      registry.Register (MaxExpressionNode.GetSupportedMethods(), typeof (MaxExpressionNode));
      registry.Register (MinExpressionNode.GetSupportedMethods(), typeof (MinExpressionNode));
      registry.Register (OfTypeExpressionNode.GetSupportedMethods(), typeof (OfTypeExpressionNode));
      registry.Register (OrderByDescendingExpressionNode.GetSupportedMethods(), typeof (OrderByDescendingExpressionNode));
      registry.Register (OrderByExpressionNode.GetSupportedMethods(), typeof (OrderByExpressionNode));
      registry.Register (ReverseExpressionNode.GetSupportedMethods(), typeof (ReverseExpressionNode));
      registry.Register (SelectExpressionNode.GetSupportedMethods(), typeof (SelectExpressionNode));
      registry.Register (SelectManyExpressionNode.GetSupportedMethods(), typeof (SelectManyExpressionNode));
      registry.Register (SingleExpressionNode.GetSupportedMethods(), typeof (SingleExpressionNode));
      registry.Register (SkipExpressionNode.GetSupportedMethods(), typeof (SkipExpressionNode));
      registry.Register (SumExpressionNode.GetSupportedMethods(), typeof (SumExpressionNode));
      registry.Register (TakeExpressionNode.GetSupportedMethods(), typeof (TakeExpressionNode));
      registry.Register (ThenByDescendingExpressionNode.GetSupportedMethods(), typeof (ThenByDescendingExpressionNode));
      registry.Register (ThenByExpressionNode.GetSupportedMethods(), typeof (ThenByExpressionNode));
      registry.Register (UnionExpressionNode.GetSupportedMethods(), typeof (UnionExpressionNode));
      registry.Register (WhereExpressionNode.GetSupportedMethods(), typeof (WhereExpressionNode));

      return registry;
    }

    /// <summary>
    /// Gets the registerable method definition from a given <see cref="MethodInfo"/>. A registerable method is a <see cref="MethodInfo"/> object
    /// that can be registered via a call to <see cref="Register"/>. When the given <paramref name="method"/> is passed to 
    /// <see cref="GetNodeType"/> and its corresponding registerable method was registered, the correct node type is returned.
    /// </summary>
    /// <param name="method">The method for which the registerable method should be retrieved. Must not be <see langword="null" />.</param>
    /// <param name="throwOnAmbiguousMatch">
    ///   <see langword="true" /> to throw a <see cref="NotSupportedException"/> if the method cannot be matched to a distinct generic method definition, 
    ///   <see langword="false" /> to return <see langword="null" /> if an unambiguous match is not possible.
    /// </param>
    /// <returns>
    /// <para>
    ///   <paramref name="method"/> itself, unless it is a closed generic method or declared in a closed generic type. In the latter cases,
    ///   the corresponding generic method definition respectively the method declared in a generic type definition is returned.
    /// </para><para>
    ///   If no generic method definition could be matched and <paramref name="throwOnAmbiguousMatch"/> was set to <see langword="false" />, 
    ///   <see langword="null" /> is returned.
    /// </para>
    /// </returns>
    /// <exception cref="NotSupportedException">
    /// Thrown if <paramref name="throwOnAmbiguousMatch"/> is set to <see langword="true" /> and no distinct generic method definition could be resolved.
    /// </exception>
    public static MethodInfo GetRegisterableMethodDefinition (MethodInfo method, bool throwOnAmbiguousMatch)
    {
      ArgumentUtility.CheckNotNull ("method", method);

      var genericMethodDefinition = method.IsGenericMethod ? method.GetGenericMethodDefinition() : method;
      if (!genericMethodDefinition.DeclaringType.GetTypeInfo().IsGenericType)
        return genericMethodDefinition;

      // Simple, fast solution, not possible in PCL because of missing MethodHandle property on MethodInfo type:
      // var declaringTypeDefinition = genericMethodDefinition.DeclaringType.GetGenericTypeDefinition();
      // return (MethodInfo) MethodBase.GetMethodFromHandle (genericMethodDefinition.MethodHandle, declaringTypeDefinition.TypeHandle);

      Lazy<MethodInfo[]> candidates;
      lock (s_genericMethodDefinitionCandidates)
      {
        if (!s_genericMethodDefinitionCandidates.TryGetValue (method, out candidates))
        {
          candidates = new Lazy<MethodInfo[]> (() => GetGenericMethodDefinitionCandidates (genericMethodDefinition));
          s_genericMethodDefinitionCandidates.Add (method, candidates);
        }
      }

      if (candidates.Value.Length == 1)
        return candidates.Value.Single();

      if (!throwOnAmbiguousMatch)
        return null;

      throw new NotSupportedException (
          string.Format (
              "A generic method definition cannot be resolved for method '{0}' on type '{1}' because a distinct match is not possible. "
              + @"The method can still be registered using the following syntax:

public static readonly NameBasedRegistrationInfo[] SupportedMethodNames = 
    new[] {{
        new NameBasedRegistrationInfo (
            ""{2}"", 
            mi => /* match rule based on MethodInfo */
        )
    }};",
              method,
              genericMethodDefinition.DeclaringType.GetGenericTypeDefinition(),
              method.Name));
    }

    private static MethodInfo[] GetGenericMethodDefinitionCandidates (MethodInfo referenceMethodDefinition)
    {
      var declaringTypeDefinition = referenceMethodDefinition.DeclaringType.GetGenericTypeDefinition();

      var referenceMethodSignature =
          new[] { new { Name = "returnValue", Type = referenceMethodDefinition.ReturnType } }
              .Concat (referenceMethodDefinition.GetParameters().Select (p => new { Name = p.Name, Type = p.ParameterType }))
              .ToArray();

      var candidates = declaringTypeDefinition.GetRuntimeMethods()
          .Select (
              m => new
                   {
                       Method = m,
                       SignatureNames = new[] { "returnValue" }.Concat (m.GetParameters().Select (p => p.Name)).ToArray(),
                       SignatureTypes = new[] { m.ReturnType }.Concat (m.GetParameters().Select (p => p.ParameterType)).ToArray()
                   })
          .Where (c => c.Method.Name == referenceMethodDefinition.Name && c.SignatureTypes.Length == referenceMethodSignature.Length)
          .ToArray();

      for (int i = 0; i < referenceMethodSignature.Length; i++)
      {
        candidates = candidates
            .Where (c => c.SignatureNames[i] == referenceMethodSignature[i].Name)
            .Where (c => c.SignatureTypes[i] == referenceMethodSignature[i].Type || c.SignatureTypes[i].GetTypeInfo().ContainsGenericParameters)
            .ToArray();
      }

      return candidates.Select (c => c.Method).ToArray();
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

      return GetNodeType (method) != null;
    }

    /// <summary>
    /// Gets the type of <see cref="IExpressionNode"/> registered with this <see cref="MethodInfoBasedNodeTypeRegistry"/> instance that
    /// matches the given <paramref name="method"/>, returning <see langword="null" /> if none can be found.
    /// </summary>
    public Type GetNodeType (MethodInfo method)
    {
      ArgumentUtility.CheckNotNull ("method", method);

      var methodDefinition = GetRegisterableMethodDefinition (method, throwOnAmbiguousMatch: false);
      if (methodDefinition == null)
        return null;

      Type result;
      _registeredMethodInfoTypes.TryGetValue (methodDefinition, out result);
      return result;
    }
  }
}

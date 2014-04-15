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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Utilities;

namespace Remotion.Linq.Parsing.Structure.IntermediateModel
{
  /// <summary>
  /// Creates instances of classes implementing the <see cref="IExpressionNode"/> interface via Reflection.
  /// </summary>
  /// <remarks>
  /// The classes implementing <see cref="IExpressionNode"/> instantiated by this factory must implement a single constructor. The source and 
  /// constructor parameters handed to the <see cref="CreateExpressionNode"/> method are passed on to the constructor; for each argument where no 
  /// parameter is passed, <see langword="null"/> is passed to the constructor.
  /// </remarks>
  public static class MethodCallExpressionNodeFactory
  {
    /// <summary>
    /// Creates an instace of type <paramref name="nodeType"/>.
    /// </summary>
    /// <exception cref="ExpressionNodeInstantiationException">
    /// Thrown if the <paramref name="parseInfo"/> or the <paramref name="additionalConstructorParameters"/> 
    /// do not match expected constructor parameters of the <paramref name="nodeType"/>.
    /// </exception>
    public static IExpressionNode CreateExpressionNode (
        Type nodeType, MethodCallExpressionParseInfo parseInfo, object[] additionalConstructorParameters)
    {
      ArgumentUtility.CheckNotNull ("nodeType", nodeType);
      ArgumentUtility.CheckTypeIsAssignableFrom ("nodeType", nodeType, typeof (IExpressionNode));
      ArgumentUtility.CheckNotNull ("additionalConstructorParameters", additionalConstructorParameters);

      var constructors = nodeType.GetTypeInfo().DeclaredConstructors.Where (c => c.IsPublic).ToArray();
      if (constructors.Length > 1)
      {
        var message = string.Format (
            "Expression node type '{0}' contains too many constructors. It must only contain a single constructor, allowing null to be passed for any optional arguments.",
            nodeType.FullName);
        throw new ArgumentException (message, "nodeType");
      }

      object[] constructorParameterArray = GetParameterArray (constructors[0], parseInfo, additionalConstructorParameters);
      try
      {
        return (IExpressionNode) constructors[0].Invoke (constructorParameterArray);
      }
      catch (ArgumentException ex)
      {
        var message = GetArgumentMismatchMessage (ex);
        throw new ExpressionNodeInstantiationException (message);
      }
    }

    private static string GetArgumentMismatchMessage (ArgumentException ex)
    {
      if (ex.Message.Contains (typeof (LambdaExpression).Name) && ex.Message.Contains (typeof (ConstantExpression).Name))
      {
        return string.Format (
            "{0} If you tried to pass a delegate instead of a LambdaExpression, this is not supported because delegates are not parsable expressions.",
            ex.Message);
      }
      else
      {
        return  string.Format ("The given arguments did not match the expected arguments: {0}", ex.Message);
      }
    }

    private static object[] GetParameterArray (
        ConstructorInfo nodeTypeConstructor, MethodCallExpressionParseInfo parseInfo, object[] additionalConstructorParameters)
    {
      var parameterInfos = nodeTypeConstructor.GetParameters();
      if (additionalConstructorParameters.Length > parameterInfos.Length - 1)
      {
        string message = string.Format (
            "The constructor of expression node type '{0}' only takes {1} parameters, but you specified {2} (including the parse info parameter).",
            nodeTypeConstructor.DeclaringType.FullName,
            parameterInfos.Length,
            additionalConstructorParameters.Length + 1);
        throw new ExpressionNodeInstantiationException (message);
      }

      var constructorParameters = new object[parameterInfos.Length];
      constructorParameters[0] = parseInfo;
      additionalConstructorParameters.CopyTo (constructorParameters, 1);
      return constructorParameters;
    }
  }
}

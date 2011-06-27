// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// re-linq is free software; you can redistribute it and/or modify it under 
// the terms of the GNU Lesser General Public License as published by the 
// Free Software Foundation; either version 2.1 of the License, 
// or (at your option) any later version.
// 
// re-linq is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-linq; if not, see http://www.gnu.org/licenses.
// 
using System;
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Linq.Utilities;

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
  public class MethodCallExpressionNodeFactory
  {
    public static IExpressionNode CreateExpressionNode (
        Type nodeType, MethodCallExpressionParseInfo parseInfo, object[] additionalConstructorParameters)
    {
      ArgumentUtility.CheckNotNull ("nodeType", nodeType);
      ArgumentUtility.CheckTypeIsAssignableFrom ("nodeType", nodeType, typeof (IExpressionNode));
      ArgumentUtility.CheckNotNull ("additionalConstructorParameters", additionalConstructorParameters);

      var constructors = nodeType.GetConstructors();
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
        throw new ExpressionNodeInstantiationException (message, ex);
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

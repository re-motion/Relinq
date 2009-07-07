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
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.Structure.IntermediateModel
{
  /// <summary>
  /// Base class for <see cref="IExpressionNode"/> implementations that represent instantiations of <see cref="MethodCallExpression"/>.
  /// </summary>
  public abstract class MethodCallExpressionNodeBase : IExpressionNode
  {
    /// <summary>
    /// Gets the <see cref="MethodInfo"/> from a given <see cref="LambdaExpression"/> that has to wrap a <see cref="MethodCallExpression"/>.
    /// If the method is a generic method, its open generic method definition is returned.
    /// This method can be used for registration of the node type with an <see cref="MethodCallExpressionNodeTypeRegistry"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="methodCall">The method call.</param>
    /// <returns></returns>
    protected static MethodInfo GetSupportedMethod<T> (Expression<Func<T>> methodCall)
    {
      ArgumentUtility.CheckNotNull ("methodCall", methodCall);

      var method = ParserUtility.GetMethod (methodCall);
      return method.IsGenericMethod ? method.GetGenericMethodDefinition() : method;
    }

    protected MethodCallExpressionNodeBase (MethodCallExpressionParseInfo parseInfo)
    {
      AssociatedIdentifier = parseInfo.AssociatedIdentifier;
      Source = parseInfo.Source;
      ParsedExpression = parseInfo.ParsedExpression;
    }


    public string AssociatedIdentifier { get; set; }
    public IExpressionNode Source { get; private set; }
    public MethodCallExpression ParsedExpression { get; private set; }

    public abstract Expression Resolve (ParameterExpression inputParameter, Expression expressionToBeResolved, ClauseGenerationContext clauseGenerationContext);
    public abstract QueryModel Apply (QueryModel queryModel, ClauseGenerationContext clauseGenerationContext);

    protected InvalidOperationException CreateResolveNotSupportedException ()
    {
      return
          new InvalidOperationException (
              GetType().Name + " does not support resolving of expressions, because it does not stream any data to the following node.");
    }

    protected InvalidOperationException CreateOutputParameterNotSupportedException ()
    {
      return
          new InvalidOperationException (
              GetType().Name + " does not support creating a parameter for its output because it does not stream any data to the following node.");
    }
  }
}
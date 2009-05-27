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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.Structure.IntermediateModel
{
  /// <summary>
  /// Represents a <see cref="MethodCallExpression"/> for <see cref="Queryable.First{TSource}(System.Linq.IQueryable{TSource})"/>
  /// or <see cref="Queryable.First{TSource}(System.Linq.IQueryable{TSource},System.Linq.Expressions.Expression{System.Func{TSource,bool}})"/>.
  /// </summary>
  public class FirstExpressionNode : ExpressionNodeBase
  {
    public static readonly MethodInfo[] SupportedMethods = new[]
                                                           {
                                                               GetSupportedMethod (() => Queryable.First<object> (null)),
                                                               GetSupportedMethod (() => Queryable.First<object> (null, null))
                                                           };

    public FirstExpressionNode (IExpressionNode source, LambdaExpression optionalPredicate)
    {
      ArgumentUtility.CheckNotNull ("source", source);
      
      Source = source;
      OptionalPredicate = optionalPredicate;
    }

    public IExpressionNode Source { get; private set; }
    public LambdaExpression OptionalPredicate { get; private set; }

    public override Expression Resolve (ParameterExpression inputParameter, Expression expressionToBeResolved)
    {
      throw CreateResolveNotSupportedException();
    }
  }
}
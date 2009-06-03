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
  /// Represents a <see cref="MethodCallExpression"/> for <see cref="Queryable.Min{TSource}"/> or <see cref="Queryable.Min{TSource,TResult}"/>.
  /// </summary>
  public class MinExpressionNode : ExpressionNodeBase
  {
    public static readonly MethodInfo[] SupportedMethods = new[]
                                                           {
                                                               GetSupportedMethod (() => Queryable.Min<object> (null)),
                                                               GetSupportedMethod (() => Queryable.Min<object, object> (null, null))
                                                           };

    private Expression _cachedSelector;

    public MinExpressionNode (IExpressionNode source, LambdaExpression optionalSelector)
      : base (ArgumentUtility.CheckNotNull ("source", source))
    {
      if (optionalSelector != null && optionalSelector.Parameters.Count != 1)
        throw new ArgumentException ("OptionalSelector must have exactly one parameter.", "optionalSelector");

      OptionalSelector = optionalSelector;
    }

    public LambdaExpression OptionalSelector { get; private set; }

    public Expression GetResolvedOptionalSelector ()
    {
      if (OptionalSelector == null)
        return null;

      if (_cachedSelector == null)
        _cachedSelector = Source.Resolve (OptionalSelector.Parameters[0], OptionalSelector.Body);

      return _cachedSelector;
    }

    public override Expression Resolve (ParameterExpression inputParameter, Expression expressionToBeResolved)
    {
      throw CreateResolveNotSupportedException ();
    }

    public override ParameterExpression CreateParameterForOutput ()
    {
      throw CreateOutputParameterNotSupportedException ();
    }
  }
}
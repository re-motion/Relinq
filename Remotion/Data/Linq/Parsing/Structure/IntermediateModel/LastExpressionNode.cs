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
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.ResultModifications;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.Structure.IntermediateModel
{
  /// <summary>
  /// Represents a <see cref="MethodCallExpression"/> for <see cref="Queryable.Last{TSource}(System.Linq.IQueryable{TSource})"/>
  /// or <see cref="Queryable.Last{TSource}(System.Linq.IQueryable{TSource},System.Linq.Expressions.Expression{System.Func{TSource,bool}})"/>.
  /// </summary>
  public class LastExpressionNode : ExpressionNodeBase
  {
    public static readonly MethodInfo[] SupportedMethods = new[]
                                                           {
                                                               GetSupportedMethod (() => Queryable.Last<object> (null)),
                                                               GetSupportedMethod (() => Queryable.Last<object> (null, null))
                                                           };

    private Expression _cachedPredicate;

    public LastExpressionNode (IExpressionNode source, LambdaExpression optionalPredicate)
        : base (ArgumentUtility.CheckNotNull ("source", source))
    {
      if (optionalPredicate != null && optionalPredicate.Parameters.Count != 1)
        throw new ArgumentException ("OptionalPredicate must have exactly one parameter.", "optionalPredicate");

      OptionalPredicate = optionalPredicate;
    }

    public LambdaExpression OptionalPredicate { get; private set; }

    public Expression GetResolvedOptionalPredicate ()
    {
      if (OptionalPredicate == null)
        return null;

      if (_cachedPredicate == null)
        _cachedPredicate = Source.Resolve (OptionalPredicate.Parameters[0], OptionalPredicate.Body);

      return _cachedPredicate;
    }

    public override Expression Resolve (ParameterExpression inputParameter, Expression expressionToBeResolved)
    {
      throw CreateResolveNotSupportedException ();
    }

    public override ParameterExpression CreateParameterForOutput ()
    {
      throw CreateOutputParameterNotSupportedException ();
    }

    public override IClause CreateClause (IClause previousClause)
    {
      ArgumentUtility.CheckNotNull ("previousClause", previousClause);

      SelectClause selectClause = GetSelectClauseForResultModification (Source, previousClause);
      selectClause.AddResultModification (new LastResultModification (selectClause));
      CreateWhereClauseForResultModification (selectClause, OptionalPredicate);

      return selectClause;
    }
  }
}
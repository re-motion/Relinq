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
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.Structure.IntermediateModel
{
  /// <summary>
  /// Represents a <see cref="MethodCallExpression"/> for 
  /// <see cref="Queryable.OrderBy{TSource,TKey}(System.Linq.IQueryable{TSource},System.Linq.Expressions.Expression{System.Func{TSource,TKey}})"/>.
  /// </summary>
  public class OrderByExpressionNode : ExpressionNodeBase
  {
    public static readonly MethodInfo[] SupportedMethods = new[]
                                                           {
                                                               GetSupportedMethod (() => Queryable.OrderBy<object, object>(null, null))
                                                           };

    private Expression _cachedSelector;

    public OrderByExpressionNode (IExpressionNode source, LambdaExpression keySelector)
    {
      ArgumentUtility.CheckNotNull ("source", source);
      ArgumentUtility.CheckNotNull ("keySelector", keySelector);

      if (keySelector != null && keySelector.Parameters.Count != 1)
        throw new ArgumentException ("KeySelector must have exactly one parameter.", "keySelector");

      Source = source;
      KeySelector = keySelector;
    }

    public IExpressionNode Source { get; private set; }
    public LambdaExpression KeySelector { get; private set; }

    public Expression GetResolvedKeySelector ()
    {
      if (_cachedSelector == null)
        _cachedSelector = Source.Resolve (KeySelector.Parameters[0], KeySelector.Body);

      return _cachedSelector;
    }

    public override Expression Resolve (ParameterExpression inputParameter, Expression expressionToBeResolved)
    {
      return Source.Resolve (inputParameter, expressionToBeResolved);
    }

    public IClause CreateClause (IClause previousClause)
    {
      ArgumentUtility.CheckNotNull ("previousClause", previousClause);
      var clause = new OrderByClause (previousClause);
      clause.AddOrdering (new Ordering (clause, KeySelector, OrderingDirection.Asc));
      return clause;
    }
  }
}
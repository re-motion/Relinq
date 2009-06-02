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
using Remotion.Data.Linq.Parsing.ExpressionTreeVisitors;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.Structure.IntermediateModel
{
  /// <summary>
  /// Represents a <see cref="MethodCallExpression"/> for 
  /// <see cref="Queryable.Select{TSource,TResult}(System.Linq.IQueryable{TSource},System.Linq.Expressions.Expression{System.Func{TSource,TResult}})"/>.
  /// </summary>
  public class SelectExpressionNode : ExpressionNodeBase
  {
    public static readonly MethodInfo[] SupportedMethods = new[]
                                                           {
                                                               GetSupportedMethod (() => Queryable.Select<object, object>(null, o => null))
                                                           };

    private Expression _cachedSelector;

    public SelectExpressionNode (IExpressionNode source, LambdaExpression selector)
    {
      ArgumentUtility.CheckNotNull ("source", source);
      ArgumentUtility.CheckNotNull ("selector", selector);

      if (selector != null && selector.Parameters.Count != 1)
        throw new ArgumentException ("Selector must have exactly one parameter.", "selector");

      Source = source;
      Selector = selector;
    }

    public IExpressionNode Source { get; private set; }
    public LambdaExpression Selector { get; private set; }

    public Expression GetResolvedSelector ()
    {
      if (_cachedSelector == null)
        _cachedSelector = Source.Resolve (Selector.Parameters[0], Selector.Body);

      return _cachedSelector;
    }

    public override Expression Resolve (ParameterExpression inputParameter, Expression expressionToBeResolved)
    {
      ArgumentUtility.CheckNotNull ("inputParameter", inputParameter);
      ArgumentUtility.CheckNotNull ("expressionToBeResolved", expressionToBeResolved);

      var resolvedSelector = Source.Resolve (Selector.Parameters[0], Selector.Body);
      return ReplacingVisitor.Replace (inputParameter, resolvedSelector, expressionToBeResolved);
    }

    public IClause CreateClause (IClause previousClause)
    {
      ArgumentUtility.CheckNotNull ("previousClause", previousClause);
      return new SelectClause (previousClause, Selector);
    }
  }
}
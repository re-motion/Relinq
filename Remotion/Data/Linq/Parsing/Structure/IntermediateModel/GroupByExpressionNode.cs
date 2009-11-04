// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// as published by the Free Software Foundation; either version 2.1 of the 
// License, or (at your option) any later version.
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
using Remotion.Data.Linq.Clauses.ResultOperators;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.Structure.IntermediateModel
{
  public class GroupByExpressionNode : ResultOperatorExpressionNodeBase, IQuerySourceExpressionNode
  {
    public static readonly MethodInfo[] SupportedMethods = new[]
                                                           {
                                                               GetSupportedMethod (() => Queryable.GroupBy<object, object> (null, o => null)),
                                                               GetSupportedMethod (() => Enumerable.GroupBy<object, object> (null, o => null)),
                                                               GetSupportedMethod (() => Queryable.GroupBy<object, object, object> (null, o => null, o => null)),
                                                               GetSupportedMethod (() => Enumerable.GroupBy<object, object, object> (null, o => null, o => null)),
                                                           };

    private readonly ResolvedExpressionCache _cachedKeySelector;
    private readonly ResolvedExpressionCache _cachedElementSelector;

    public GroupByExpressionNode (MethodCallExpressionParseInfo parseInfo, LambdaExpression keySelector, LambdaExpression optionalElementSelector)
        : base (parseInfo, null, null)
    {
      ArgumentUtility.CheckNotNull ("keySelector", keySelector);

      if (keySelector.Parameters.Count != 1)
        throw new ArgumentException ("KeySelector must have exactly one parameter.", "keySelector");

      if (optionalElementSelector != null && optionalElementSelector.Parameters.Count != 1)
        throw new ArgumentException ("ElementSelector must have exactly one parameter.", "optionalElementSelector");

      KeySelector = keySelector;
      OptionalElementSelector = optionalElementSelector;

      _cachedKeySelector = new ResolvedExpressionCache (this);

      if (optionalElementSelector != null)
        _cachedElementSelector = new ResolvedExpressionCache (this);
    }

    public LambdaExpression KeySelector { get; private set; }
    public LambdaExpression OptionalElementSelector { get; private set; }

    public Expression GetResolvedKeySelector (ClauseGenerationContext clauseGenerationContext)
    {
      return _cachedKeySelector.GetOrCreate (r => r.GetResolvedExpression (KeySelector.Body, KeySelector.Parameters[0], clauseGenerationContext));
    }

    public Expression GetResolvedOptionalElementSelector (ClauseGenerationContext clauseGenerationContext)
    {
      if (OptionalElementSelector == null)
        return null;

      return
          _cachedElementSelector.GetOrCreate (
              r => r.GetResolvedExpression (OptionalElementSelector.Body, OptionalElementSelector.Parameters[0], clauseGenerationContext));
    }

    public override Expression Resolve (
        ParameterExpression inputParameter, Expression expressionToBeResolved, ClauseGenerationContext clauseGenerationContext)
    {
      return QuerySourceExpressionNodeUtility.ReplaceParameterWithReference (
          this, 
          inputParameter, 
          expressionToBeResolved, 
          clauseGenerationContext);
    }

    protected override ResultOperatorBase CreateResultOperator (ClauseGenerationContext clauseGenerationContext)
    {
      var resolvedKeySelector = GetResolvedKeySelector (clauseGenerationContext);

      var resolvedElementSelector = GetResolvedOptionalElementSelector (clauseGenerationContext);
      if (resolvedElementSelector == null)
      {
        // supply a default element selector if none is given
        // just resolve KeySelector.Parameters[0], that's the input data flowing in from the source node
        resolvedElementSelector = Source.Resolve (KeySelector.Parameters[0], KeySelector.Parameters[0], clauseGenerationContext);
      }
      
      var resultOperator = new GroupResultOperator (AssociatedIdentifier, resolvedKeySelector, resolvedElementSelector);
      clauseGenerationContext.AddContextInfo (this, resultOperator);
      return resultOperator;
    }
  }
}

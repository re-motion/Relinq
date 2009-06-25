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
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Parsing.ExpressionTreeVisitors;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.Structure.IntermediateModel
{
  /// <summary>
  /// Acts as a base class for <see cref="IExpressionNode"/>s standing for <see cref="MethodCallExpression"/>s that modify the result of the query
  /// rather than representing actual clauses, such as <see cref="CountExpressionNode"/> or <see cref="DistinctExpressionNode"/>.
  /// </summary>
  public abstract class ResultModificationExpressionNodeBase : MethodCallExpressionNodeBase
  {
    private readonly ResolvedExpressionCache _cachedPredicate;
    private readonly ResolvedExpressionCache _cachedSelector;
    
    protected ResultModificationExpressionNodeBase (
        MethodCallExpressionParseInfo parseInfo, LambdaExpression optionalPredicate, LambdaExpression optionalSelector)
        : base (parseInfo)
    {
      if (optionalPredicate != null && optionalPredicate.Parameters.Count != 1)
        throw new ArgumentException ("OptionalPredicate must have exactly one parameter.", "optionalPredicate");

      if (optionalSelector != null && optionalSelector.Parameters.Count != 1)
        throw new ArgumentException ("OptionalSelector must have exactly one parameter.", "optionalSelector");

      OptionalPredicate = optionalPredicate;
      OptionalSelector = optionalSelector;

      if (optionalPredicate != null)
        _cachedPredicate = new ResolvedExpressionCache (Source);

      if (optionalSelector != null)
        _cachedSelector = new ResolvedExpressionCache (Source);
    }

    protected abstract ResultModificationBase CreateResultModification (SelectClause selectClause);

    public LambdaExpression OptionalPredicate { get; private set; }
    public LambdaExpression OptionalSelector { get; private set; }

    public Expression GetResolvedOptionalPredicate (ClauseGenerationContext clauseGenerationContext)
    {
      if (OptionalPredicate == null)
        return null;

      return _cachedPredicate.GetOrCreate (r => r.GetResolvedExpression (OptionalPredicate.Body, OptionalPredicate.Parameters[0], clauseGenerationContext));
    }

    public Expression GetResolvedOptionalSelector (ClauseGenerationContext clauseGenerationContext)
    {
      if (OptionalSelector == null)
        return null;

      return _cachedSelector.GetOrCreate (r => r.GetResolvedExpression (OptionalSelector.Body, OptionalSelector.Parameters[0], clauseGenerationContext));
    }

    public override IClause CreateClause (IClause previousClause, ClauseGenerationContext clauseGenerationContext)
    {
      ArgumentUtility.CheckNotNull ("previousClause", previousClause);
      clauseGenerationContext.ResultModificationNodeRegistry.AddResultModificationNode (this);
      return previousClause;
    }

    public override SelectClause CreateSelectClause (IClause previousClause, ClauseGenerationContext clauseGenerationContext)
    {
      return Source.CreateSelectClause (previousClause, clauseGenerationContext);
    }

    /// <summary>
    /// Applies a <see cref="ResultModificationBase"/> representing this <see cref="ResultModificationExpressionNodeBase"/> to the given 
    /// <paramref name="queryModel"/>, adding a <see cref="ResultModificationBase"/>, adding a <see cref="WhereClause"/> for the 
    /// <see cref="OptionalPredicate"/> if present and adjusting the
    /// <see cref="SelectClause.Selector"/> for the <see cref="OptionalSelector"/> if present.
    /// </summary>
    /// <param name="queryModel">The <see cref="QueryModel"/> to apply the changes to.</param>
    /// <param name="clauseGenerationContext">The clause generation context.</param>
    public virtual void Apply (QueryModel queryModel, ClauseGenerationContext clauseGenerationContext)
    {
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);

      var selectClause = ((SelectClause) queryModel.SelectOrGroupClause);
      selectClause.ResultModifications.Add (CreateResultModification (selectClause));

      if (OptionalPredicate != null)
      {
        var whereClause = new WhereClause (null, GetResolvedOptionalPredicate (clauseGenerationContext));
        selectClause.PreviousClause = whereClause;
        queryModel.BodyClauses.Add (whereClause);
      }

      if (OptionalSelector != null)
      {
        // for a selectClause.Selector of x => x.Property1
        // and an OptionalSelector of a => a.Property2
        // make x => x.Property1.Property2 by replacing a (OptionalSelector.Parameters[0]) with the body of selectClause.Selector

        // we use OptionalSelector instead of GetResolvedOptionalSelector because we are substituting the selector's parameter with
        // selectClause.Selector (which is already resolved)

        var newSelector = ReplacingVisitor.Replace (OptionalSelector.Parameters[0], selectClause.Selector, OptionalSelector.Body);
        selectClause.Selector = newSelector;
      }
    }
  }
}
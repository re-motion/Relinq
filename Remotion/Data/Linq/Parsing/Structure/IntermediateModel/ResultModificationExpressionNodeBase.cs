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

      SelectClause selectClause = GetSelectClauseForResultModification (previousClause, clauseGenerationContext);
      selectClause.AddResultModification (CreateResultModification (selectClause));
      CreateWhereClauseForResultModification (selectClause, clauseGenerationContext);
      AdjustSelectorForResultModification (selectClause);

      return selectClause;
    }

    /// <summary>
    /// Gets the <see cref="SelectClause"/> needed when implementing the <see cref="MethodCallExpressionNodeBase.CreateClause"/> method for a result modification node.
    /// </summary>
    /// <returns>The previous clause if it is a <see cref="SelectClause"/>, or a new clause with the given <paramref name="previousClause"/>
    /// and an identity projection if it is not.</returns>
    /// <remarks>
    /// Result modification nodes such as <see cref="CountExpressionNode"/> or <see cref="DistinctExpressionNode"/> do not identify real 
    /// clauses, they represent result modifications in the preceding <see cref="SelectClause"/>.
    /// Therefore, implementations of <see cref="MethodCallExpressionNodeBase.CreateClause"/> will usually not add new clauses, but instead call 
    /// <see cref="SelectClause.AddResultModification"/> on the <paramref name="previousClause"/>. If, however, the <paramref name="previousClause"/>
    /// is not a <see cref="SelectClause"/> because it was optimized away, a new trivial <see cref="SelectClause"/> must be added by the node. This
    /// is implemented by this method.
    /// </remarks>
    private SelectClause GetSelectClauseForResultModification (IClause previousClause, ClauseGenerationContext clauseGenerationContext)
    {
      var selectClause = previousClause as SelectClause;

      if (selectClause == null)
      {
        var selectorParameter = Source.CreateParameterForOutput();
        var resolvedSelectorParameter = Source.Resolve (selectorParameter, selectorParameter, clauseGenerationContext);
        selectClause = new SelectClause (previousClause, resolvedSelectorParameter);
      }

      return selectClause;
    }

    /// <summary>
    /// Gets and injects the <see cref="WhereClause"/> when implementing the <see cref="MethodCallExpressionNodeBase.CreateClause"/> method for a result modification node with an 
    /// optional predicate if that predicate is not <see langword="null"/>.
    /// </summary>
    /// <remarks>
    /// Result modification nodes such as <see cref="CountExpressionNode"/> or <see cref="DistinctExpressionNode"/>
    /// do not identify real clauses, they represent result modifications in the preceding <see cref="SelectClause"/>.
    /// Some of them contain optional predicates, which need to be transformed into <see cref="WhereClause"/> in the <see cref="MethodCallExpressionNodeBase.CreateClause"/> method.
    /// That <see cref="WhereClause"/> will be inserted before the <paramref name="selectClause"/> modified by the result modification node.
    /// Creation and insertion of this <see cref="WhereClause"/> is implemented by this method.
    /// </remarks>
    private void CreateWhereClauseForResultModification (SelectClause selectClause, ClauseGenerationContext clauseGenerationContext)
    {
      ArgumentUtility.CheckNotNull ("selectClause", selectClause);

      if (OptionalPredicate != null)
      {
        var whereClause = new WhereClause (selectClause.PreviousClause, GetResolvedOptionalPredicate (clauseGenerationContext));
        selectClause.PreviousClause = whereClause;
      }
    }

    /// <summary>
    /// Adjusts the <see cref="SelectClause.Selector"/> of the <paramref name="selectClause"/> modified by a result modification node for a nodes with an 
    /// optional selector if that selector is not <see langword="null"/>.
    /// </summary>
    /// <remarks>
    /// Result modification nodes such as <see cref="MinExpressionNode"/> or <see cref="SumExpressionNode"/>
    /// do not identify real clauses, they represent result modifications in the preceding <see cref="SelectClause"/>.
    /// Some of them contain optional selectors, which need to be combined with the <see cref="SelectClause.Selector"/> of the 
    /// <paramref name="selectClause"/> modified by the node.
    /// This process of adjusting the selector is implemented by this method.
    /// </remarks>
    private void AdjustSelectorForResultModification (SelectClause selectClause)
    {
      ArgumentUtility.CheckNotNull ("selectClause", selectClause);

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
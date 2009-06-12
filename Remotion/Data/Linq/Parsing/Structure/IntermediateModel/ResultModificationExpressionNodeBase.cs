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
    private Expression _cachedPredicate;
    private Expression _cachedSelector;

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
    }

    protected abstract ResultModificationBase CreateResultModification (SelectClause selectClause);

    public LambdaExpression OptionalPredicate { get; private set; }
    public LambdaExpression OptionalSelector { get; private set; }

    public Expression GetResolvedOptionalPredicate ()
    {
      if (OptionalPredicate == null)
        return null;

      if (_cachedPredicate == null)
        _cachedPredicate = Source.Resolve (OptionalPredicate.Parameters[0], OptionalPredicate.Body);

      return _cachedPredicate;
    }

    public Expression GetResolvedOptionalSelector ()
    {
      if (OptionalSelector == null)
        return null;

      if (_cachedSelector == null)
        _cachedSelector = Source.Resolve (OptionalSelector.Parameters[0], OptionalSelector.Body);

      return _cachedSelector;
    }

    public override IClause CreateClause (IClause previousClause)
    {
      ArgumentUtility.CheckNotNull ("previousClause", previousClause);

      SelectClause selectClause = GetSelectClauseForResultModification (previousClause);
      selectClause.AddResultModification (CreateResultModification (selectClause));
      CreateWhereClauseForResultModification (selectClause, OptionalPredicate);
      AdjustSelectorForResultModification (selectClause, OptionalSelector);

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
    protected SelectClause GetSelectClauseForResultModification (IClause previousClause)
    {
      ArgumentUtility.CheckNotNull ("previousClause", previousClause);

      var selectClause = previousClause as SelectClause;

      if (selectClause == null)
      {
        var selectorParameter = Source.CreateParameterForOutput();
        selectClause = new SelectClause (previousClause, Expression.Lambda (selectorParameter, selectorParameter));
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
    protected void CreateWhereClauseForResultModification (SelectClause selectClause, LambdaExpression optionalPredicate)
    {
      ArgumentUtility.CheckNotNull ("selectClause", selectClause);

      if (optionalPredicate != null)
      {
        var whereClause = new WhereClause (selectClause.PreviousClause, optionalPredicate);
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
    protected void AdjustSelectorForResultModification (SelectClause selectClause, LambdaExpression optionalSelector)
    {
      ArgumentUtility.CheckNotNull ("selectClause", selectClause);

      if (optionalSelector != null)
      {
        // for a selectClause.Selector of x => x.Property1
        // and an OptionalSelector of a => a.Property2
        // make x => x.Property1.Property2 by replacing a (OptionalSelector.Parameters[0]) with the body of selectClause.Selector
        var newSelectorBody = ReplacingVisitor.Replace (optionalSelector.Parameters[0], selectClause.Selector.Body, optionalSelector.Body);
        var newSelector = Expression.Lambda (newSelectorBody, selectClause.Selector.Parameters[0]);
        selectClause.Selector = newSelector;
      }
    }
  }
}
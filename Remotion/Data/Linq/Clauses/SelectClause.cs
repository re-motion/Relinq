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
using System.Diagnostics;
using Remotion.Collections;
using Remotion.Data.Linq.Clauses.ExecutionStrategies;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Clauses.ExpressionTreeVisitors;
using Remotion.Data.Linq.StringBuilding;
using Remotion.Utilities;
using System.Linq.Expressions;
using System.Linq;

namespace Remotion.Data.Linq.Clauses
{
  /// <summary>
  /// Represents the select part of a query, projecting data items according to some <see cref="Selector"/>.
  /// </summary>
  /// <example>
  /// In C#, the "select" clause in the following sample corresponds to a <see cref="SelectClause"/>. "s" (a reference to the query source "s", see
  /// <see cref="QuerySourceReferenceExpression"/>) is the <see cref="Selector"/> expression:
  /// <ode>
  /// var query = from s in Students
  ///             where s.First == "Hugo"
  ///             select s;
  /// </ode>
  /// </example>
  public class SelectClause : ISelectGroupClause
  {
    private Expression _selector;

    /// <summary>
    /// Initializes a new instance of the <see cref="SelectClause"/> class.
    /// </summary>
    /// <param name="selector">The selector that projects the data items.</param>
    public SelectClause (Expression selector)
    {
      ArgumentUtility.CheckNotNull ("selector", selector);

      _selector = selector;

      ResultModifications = new ObservableCollection<ResultModificationBase> ();
      ResultModifications.ItemInserted += ResultModifications_ItemAdded;
      ResultModifications.ItemSet += ResultModifications_ItemAdded;
    }

    /// <summary>
    /// Gets the selector defining what parts of the data items are returned by the query.
    /// </summary>
    [DebuggerDisplay ("{Remotion.Data.Linq.StringBuilding.FormattingExpressionTreeVisitor.Format (Selector),nq}")]
    public Expression Selector 
    {
      get { return _selector; }
      set { _selector = ArgumentUtility.CheckNotNull ("value", value); }
    }

    /// <summary>
    /// Gets the result modifications attached to this <see cref="SelectClause"/>. Result modifications modify the query's result set, aggregating,
    /// filtering, or otherwise processing the result before it is returned.
    /// </summary>
    public ObservableCollection<ResultModificationBase> ResultModifications { get; private set; }

    /// <summary>
    /// Accepts the specified visitor by calling its <see cref="IQueryModelVisitor.VisitSelectClause"/> method.
    /// </summary>
    /// <param name="visitor">The visitor to accept.</param>
    /// <param name="queryModel">The query model in whose context this clause is visited.</param>
    public virtual void Accept (IQueryModelVisitor visitor, QueryModel queryModel)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);

      visitor.VisitSelectClause (this, queryModel);
    }

    /// <summary>
    /// Clones this clause, adjusting all <see cref="QuerySourceReferenceExpression"/> instances held by it as defined by
    /// <paramref name="cloneContext"/>.
    /// </summary>
    /// <param name="cloneContext">The clone context to use for replacing <see cref="QuerySourceReferenceExpression"/> objects.</param>
    /// <returns>A clone of this clause.</returns>
    public SelectClause Clone (CloneContext cloneContext)
    {
      ArgumentUtility.CheckNotNull ("cloneContext", cloneContext);

      var result = new SelectClause (Selector);
      result.TransformExpressions (ex => ReferenceReplacingExpressionTreeVisitor.ReplaceClauseReferences (ex, cloneContext.ClauseMapping));
      foreach (var resultModification in ResultModifications)
      {
        var resultModificationClone = resultModification.Clone (cloneContext);
        result.ResultModifications.Add (resultModificationClone);
      }

      return result;
    }

    ISelectGroupClause ISelectGroupClause.Clone (CloneContext cloneContext)
    {
      return Clone (cloneContext);
    }

    /// <summary>
    /// Gets the execution strategy to use for this clause. The execution strategy defines how to dispatch a query
    /// to an implementation of <see cref="IQueryExecutor"/> when the <see cref="QueryProviderBase"/> needs to execute a query.
    /// By default, it is <see cref="CollectionExecutionStrategy"/>, but this can be modified by the <see cref="ResultModifications"/>.
    /// </summary>
    public IExecutionStrategy GetExecutionStrategy ()
    {
      if (ResultModifications.Count > 0)
        return ResultModifications[ResultModifications.Count - 1].ExecutionStrategy;
      else
        return CollectionExecutionStrategy.Instance;
    }

    private void ResultModifications_ItemAdded (object sender, ObservableCollectionChangedEventArgs<ResultModificationBase> e)
    {
      ArgumentUtility.CheckNotNull ("e.Item", e.Item);
    }

    /// <summary>
    /// Transforms all the expressions in this clause and its child objects via the given <paramref name="transformation"/> delegate.
    /// </summary>
    /// <param name="transformation">The transformation object. This delegate is called for each <see cref="Expression"/> within this
    /// clause, and those expressions will be replaced with what the delegate returns.</param>
    public void TransformExpressions (Func<Expression, Expression> transformation)
    {
      ArgumentUtility.CheckNotNull ("transformation", transformation);
      Selector = transformation (Selector);
      foreach (var resultModification in ResultModifications)
      {
        resultModification.TransformExpressions (transformation);
      }
    }

    public override string ToString ()
    {
      var result = "select " + FormattingExpressionTreeVisitor.Format (Selector);
      return ResultModifications.Aggregate (result, (s, r) => s + " => " + r);
    }
  }
}

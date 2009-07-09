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
using System.Linq.Expressions;
using Remotion.Data.Linq.Clauses.ExecutionStrategies;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Clauses.ExpressionTreeVisitors;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Clauses
{
  /// <summary>
  /// Represents the group part of a query, grouping items given by an <see cref="ElementSelector"/> according to some key retrieved by a
  /// <see cref="KeySelector"/>.
  /// </summary>
  /// <example>
  /// In C#, the "group by" clause in the following sample corresponds to a <see cref="GroupClause"/>. "s" (a reference to the query source "s", see
  /// <see cref="QuerySourceReferenceExpression"/>) is the <see cref="ElementSelector"/> expression, "s.Country" is the <see cref="KeySelector"/>
  /// expression:
  /// <ode>
  /// var query = from s in Students
  ///             where s.First == "Hugo"
  ///             group s by s.Country;
  /// </ode>
  /// </example>
  public class GroupClause : ISelectGroupClause
  {
    private Expression _keySelector;
    private Expression _elementSelector;

    /// <summary>
    /// Initializes a new instance of the <see cref="GroupClause"/> class.
    /// </summary>
    /// <param name="keySelector">The selector retrieving the key by which to group items.</param>
    /// <param name="elementSelector">The selector retrieving the elements to group.</param>
    public GroupClause (Expression keySelector, Expression elementSelector)
    {
      ArgumentUtility.CheckNotNull ("elementSelector", elementSelector);
      ArgumentUtility.CheckNotNull ("keySelector", keySelector);

      _elementSelector = elementSelector;
      _keySelector = keySelector;
    }

    /// <summary>
    /// Gets or sets the selector retrieving the key by which to group items.
    /// </summary>
    /// <value>The key selector.</value>
    [DebuggerDisplay ("{Remotion.Data.Linq.StringBuilding.FormattingExpressionTreeVisitor.Format (KeySelector),nq}")]
    public Expression KeySelector
    {
      get { return _keySelector; }
      set { _keySelector = ArgumentUtility.CheckNotNull ("value", value); }
    }

    /// <summary>
    /// Gets or sets the selector retrieving the elements to group.
    /// </summary>
    /// <value>The element selector.</value>
    [DebuggerDisplay ("{Remotion.Data.Linq.StringBuilding.FormattingExpressionTreeVisitor.Format (ElementSelector),nq}")]
    public Expression ElementSelector
    {
      get { return _elementSelector; }
      set { _elementSelector = ArgumentUtility.CheckNotNull ("value", value); }
    }

    /// <summary>
    /// Accepts the specified visitor by calling one its <see cref="IQueryModelVisitor.VisitGroupClause"/> method.
    /// </summary>
    /// <param name="visitor">The visitor to accept.</param>
    /// <param name="queryModel">The query model in whose context this clause is visited.</param>
    public void Accept (IQueryModelVisitor visitor, QueryModel queryModel)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);

      visitor.VisitGroupClause (this, queryModel);
    }

    /// <summary>
    /// Clones this clause, adjusting all <see cref="QuerySourceReferenceExpression"/> instances held by it as defined by
    /// <paramref name="cloneContext"/>.
    /// </summary>
    /// <param name="cloneContext">The clone context to use for replacing <see cref="QuerySourceReferenceExpression"/> objects.</param>
    /// <returns>A clone of this clause.</returns>
    public GroupClause Clone (CloneContext cloneContext)
    {
      ArgumentUtility.CheckNotNull ("cloneContext", cloneContext);

      var clone = new GroupClause (KeySelector, ElementSelector);
      clone.TransformExpressions (ex => ReferenceReplacingExpressionTreeVisitor.ReplaceClauseReferences (ex, cloneContext.ClauseMapping));
      return clone;
    }

    ISelectGroupClause ISelectGroupClause.Clone (CloneContext cloneContext)
    {
      return Clone (cloneContext);
    }

    /// <summary>
    /// Gets the execution strategy to use for the given select or group clause. The execution strategy defines how to dispatch a query
    /// to an implementation of <see cref="IQueryExecutor"/> when the <see cref="QueryProviderBase"/> needs to execute a query.
    /// </summary>
    /// <returns>
    /// <see cref="CollectionExecutionStrategy.Instance"/> because <see cref="GroupClause"/> always selects a collection of groupings.
    /// </returns>
    public IExecutionStrategy GetExecutionStrategy ()
    {
      return CollectionExecutionStrategy.Instance;
    }

    /// <summary>
    /// Transforms all the expressions in this clause and its child objects via the given <paramref name="transformation"/> delegate.
    /// </summary>
    /// <param name="transformation">The transformation object. This delegate is called for each <see cref="Expression"/> within this
    /// clause, and those expressions will be replaced with what the delegate returns.</param>
    public void TransformExpressions (Func<Expression, Expression> transformation)
    {
      ArgumentUtility.CheckNotNull ("transformation", transformation);

      ElementSelector = transformation (ElementSelector);
      KeySelector = transformation (KeySelector);
    }

    public override string ToString ()
    {
      return string.Format (
          "group {0} by {1}",
          FormattingExpressionTreeVisitor.Format (ElementSelector),
          FormattingExpressionTreeVisitor.Format (KeySelector));
    }
  }
}
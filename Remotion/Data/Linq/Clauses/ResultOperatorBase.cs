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
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Clauses
{
  /// <summary>
  /// Represents an operation that is executed on the result set of the query, aggregating, filtering, or restricting the number of result items
  /// before the query result is returned.
  /// </summary>
  public abstract class ResultOperatorBase
  {
    protected ResultOperatorBase (IExecutionStrategy executionStrategy)
    {
      ArgumentUtility.CheckNotNull ("executionStrategy", executionStrategy);
      ExecutionStrategy = executionStrategy;
    }

    /// <summary>
    /// Gets the execution strategy to use for this <see cref="ResultOperatorBase"/>. The execution strategy defines how to dispatch a query
    /// to an implementation of <see cref="IQueryExecutor"/> when the <see cref="QueryProviderBase"/> needs to execute a query.
    /// </summary>
    public IExecutionStrategy ExecutionStrategy { get; private set; }

    /// <summary>
    /// Executes this result modification in memory, on a given enumeration of items. Executing result modifications in memory should only be 
    /// performed if the target query system does not support the modification.
    /// </summary>
    /// <returns>An enumerable containing the results of the modiciation. This is either a filtered version of <param name="items"/> or a
    /// new <see cref="IEnumerable"/> containing exactly one value or item, depending on the modification.</returns>
    public abstract IEnumerable ExecuteInMemory<T> (IEnumerable<T> items);

    /// <summary>
    /// Clones this item, adjusting all <see cref="QuerySourceReferenceExpression"/> instances held by it as defined by
    /// <paramref name="cloneContext"/>.
    /// </summary>
    /// <param name="cloneContext">The clone context to use for replacing <see cref="QuerySourceReferenceExpression"/> objects.</param>
    /// <returns>A clone of this item.</returns>
    public abstract ResultOperatorBase Clone (CloneContext cloneContext);

    /// <summary>
    /// Accepts the specified visitor by calling its <see cref="IQueryModelVisitor.VisitResultOperator"/> method.
    /// </summary>
    /// <param name="visitor">The visitor to accept.</param>
    /// <param name="queryModel">The query model in whose context this clause is visited.</param>
    /// <param name="selectClause">The <see cref="SelectClause"/> in whose context this item is visited.</param>
    /// <param name="index">The index of this item in the <paramref name="selectClause"/>'s <see cref="OrderByClause.Orderings"/> collection.</param>
    public virtual void Accept (IQueryModelVisitor visitor, QueryModel queryModel, SelectClause selectClause, int index)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);
      ArgumentUtility.CheckNotNull ("selectClause", selectClause);
      
      visitor.VisitResultOperator (this, queryModel, selectClause, index);
    }

    /// <summary>
    /// Transforms all the expressions in this item via the given <paramref name="transformation"/> delegate. Subclasses must override this method
    /// if they hold any expressions.
    /// </summary>
    /// <param name="transformation">The transformation object. This delegate is called for each <see cref="Expression"/> within this
    /// item, and those expressions will be replaced with what the delegate returns.</param>
    public virtual void TransformExpressions (Func<Expression, Expression> transformation)
    {
      //nothing to do here
    }

    
  }
}

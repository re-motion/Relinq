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
using Remotion.Utilities;

namespace Remotion.Data.Linq.Clauses
{
  /// <summary>
  /// Represents an operation that is executed on the result set of the query, aggregating, filtering, or restricting the number of result items
  /// before the query result is returned.
  /// </summary>
  public abstract class ResultModificationBase
  {
    protected ResultModificationBase (IExecutionStrategy executionStrategy)
    {
      ArgumentUtility.CheckNotNull ("executionStrategy", executionStrategy);
      ExecutionStrategy = executionStrategy;
    }

    public IExecutionStrategy ExecutionStrategy { get; private set; }

    /// <summary>
    /// Executes this result modification in memory, on a given enumeration of items. Executing result modifications in memory should only be 
    /// performed if the target query system does not support the modification.
    /// </summary>
    /// <returns>An enumerable containing the results of the modiciation. This is either a filtered version of <param name="items"/> or a
    /// new <see cref="IEnumerable"/> containing exactly one value or item, depending on the modification.</returns>
    public abstract IEnumerable ExecuteInMemory<T> (IEnumerable<T> items);

    public abstract ResultModificationBase Clone (CloneContext cloneContext);

    public virtual void Accept (IQueryModelVisitor visitor, QueryModel queryModel, SelectClause selectClause, int index)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);
      ArgumentUtility.CheckNotNull ("selectClause", selectClause);
      
      visitor.VisitResultModification (this, queryModel, selectClause, index);
    }

    public virtual void TransformExpressions (Func<Expression, Expression> transformation)
    {
      //nothing to do here
    }

    
  }
}

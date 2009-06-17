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
using Remotion.Utilities;

namespace Remotion.Data.Linq.Clauses
{
  // TODO MG: Unfinished Refactoring: test
  public abstract class ResultModificationBase
  {
    protected ResultModificationBase (SelectClause selectClause, IExecutionStrategy executionStrategy)
    {
      ArgumentUtility.CheckNotNull ("selectClause", selectClause);
      ArgumentUtility.CheckNotNull ("executionStrategy", executionStrategy);

      SelectClause = selectClause;
      ExecutionStrategy = executionStrategy;
    }

    public SelectClause SelectClause { get; private set; }
    public IExecutionStrategy ExecutionStrategy { get; private set; }

    // TODO MG: Unfinished Refactoring: test, implement, and adapt IQueryVisitor and its implementations
    public void Accept (IQueryVisitor visitor)
    {
      //ArgumentUtility.CheckNotNull ("visitor", visitor);
      //visitor.VisitResultModifierClause (this);
      throw new NotImplementedException();
    }

    public abstract ResultModificationBase Clone (SelectClause newSelectClause, FromClauseMapping fromClauseMapping);

    /// <summary>
    /// Executes this result modification in memory, on a given enumeration of items. Executing result modifications in memory should only be 
    /// performed if the target query system does not support the modification.
    /// </summary>
    /// <returns>An enumerable containing the results of the modiciation. This is either a filtered version of <param name="items"/> or a
    /// new <see cref="IEnumerable"/> containing exactly one value or item, depending on the modification.</returns>
    public abstract IEnumerable ExecuteInMemory<T> (IEnumerable<T> items);
  }
}

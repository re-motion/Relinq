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
using System.Collections.Generic;
using Remotion.Data.Linq.Clauses.ExecutionStrategies;

namespace Remotion.Data.Linq.Clauses.ResultOperators
{
  /// <summary>
  /// Represents a result operator that returns a scalar value, e.g. <see cref="CountResultOperator"/> or <see cref="SumResultOperator"/>.
  /// </summary>
  public abstract class ScalarResultOperatorBase : ResultOperatorBase
  {
    protected ScalarResultOperatorBase ()
        : base (ScalarExecutionStrategy.Instance)
    {
    }

    /// <summary>
    /// Executes this result operator in memory, on a given enumeration of items. Executing result operator in memory should only be 
    /// performed if the target query system does not support the operator.
    /// </summary>
    /// <returns>A scalar value representing the result of the operator. </returns>
    public abstract TScalar ExecuteInMemory<TItem, TScalar> (IEnumerable<TItem> items);
  }
}
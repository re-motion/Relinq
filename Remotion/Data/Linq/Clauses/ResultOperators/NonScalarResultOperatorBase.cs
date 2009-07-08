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
using Remotion.Utilities;

namespace Remotion.Data.Linq.Clauses.ResultOperators
{
  /// <summary>
  /// Represents a result operator that returns a non-scalar value, e.g. <see cref="DistinctResultOperator"/> or 
  /// <see cref="TakeResultOperator"/>. This kind of result operator must not change the item type of the items in the result set.
  /// </summary>
  public abstract class NonScalarResultOperatorBase : ResultOperatorBase
  {
    protected NonScalarResultOperatorBase (IExecutionStrategy executionStrategy)
        : base (ArgumentUtility.CheckNotNull ("executionStrategy", executionStrategy))
    {
    }

    /// <summary>
    /// Executes this result operator in memory, on a given enumeration of items. Executing result operator in memory should only be 
    /// performed if the target query system does not support the operator.
    /// </summary>
    /// <returns>An enumerable containing the results of the operator. This is either a filtered version of <param name="items"/> (see 
    /// <see cref="TakeResultOperator"/> or <see cref="DistinctResultOperator"/>) or a new <see cref="IEnumerable{T}"/> containing exactly one item, 
    /// depending on the operator (see <see cref="FirstResultOperator"/> or <see cref="SingleResultOperator"/>).</returns>
    public abstract IEnumerable<T> ExecuteInMemory<T> (IEnumerable<T> items);
  }
}
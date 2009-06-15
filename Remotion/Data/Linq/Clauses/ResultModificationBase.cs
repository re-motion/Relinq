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
using System.Linq;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Clauses
{
  // TODO MG: Unfinished Refactoring: test
  public abstract class ResultModificationBase
  {
    protected ResultModificationBase (SelectClause selectClause)
    {
      ArgumentUtility.CheckNotNull ("selectClause", selectClause);

      SelectClause = selectClause;
    }

    public SelectClause SelectClause { get; private set; }

    // TODO MG: Unfinished Refactoring: test, implement, and adapt IQueryVisitor and its implementations
    public void Accept (IQueryVisitor visitor)
    {
      //ArgumentUtility.CheckNotNull ("visitor", visitor);
      //visitor.VisitResultModifierClause (this);
      throw new NotImplementedException();
    }

    public abstract ResultModificationBase Clone (SelectClause newSelectClause);

    /// <summary>
    /// Executes this result modification in memory, on a given enumeration of items. Executing result modifications in memory should only be 
    /// performed if the target query system does not support the modification.
    /// </summary>
    /// <returns>An enumerable containing the results of the modiciation. This is either a filtered version of <param name="items"/> or a
    /// new <see cref="IEnumerable"/> containing exactly one value or item, depending on the modification.</returns>
    public abstract IEnumerable ExecuteInMemory<T> (IEnumerable<T> items);

    /// <summary>
    /// Extracts the result of this modification from the given stream. This method is used by <see cref="QueryProviderBase"/> to extract the
    /// value to be returned to the user from the stream returned by the <see cref="IQueryExecutor"/>.
    /// </summary>
    /// <returns>Either <param name="stream"/> or the value or item held by the single item of the stream, depending on the modification.</returns>
    public abstract object ConvertStreamToResult<T> (IEnumerable<T> stream);

    protected object ConvertStreamToSingleResult<T> (IEnumerable<T> stream)
    {
      ArgumentUtility.CheckNotNull ("stream", stream);

      try
      {
        return stream.Single ();
      }
      catch (InvalidOperationException ex)
      {
        var message = string.Format ("A query ending with a {0} must retrieve exactly one item.", GetType ().Name);
        throw new InvalidOperationException (message, ex);
      }
    }

    protected object ConvertStreamToValue<T> (IEnumerable<T> stream)
    {
      try
      {
        return stream.Single ();
      }
      catch (InvalidOperationException ex)
      {
        var message = string.Format ("A query ending with a {0} must retrieve exactly one value.", GetType ().Name);
        throw new InvalidOperationException (message, ex);
      }
    }
  }
}

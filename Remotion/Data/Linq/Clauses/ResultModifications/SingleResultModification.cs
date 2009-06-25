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
using Remotion.Data.Linq.Clauses.ExecutionStrategies;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Clauses.ResultModifications
{
  public class SingleResultModification : ResultModificationBase
  {
    public SingleResultModification (bool returnDefaultWhenEmpty)
        : base (returnDefaultWhenEmpty ? SingleExecutionStrategy.InstanceWithDefaultWhenEmpty : SingleExecutionStrategy.InstanceNoDefaultWhenEmpty)
    {
      ReturnDefaultWhenEmpty = returnDefaultWhenEmpty;
    }

    public bool ReturnDefaultWhenEmpty { get; set; }

    public override ResultModificationBase Clone (CloneContext cloneContext)
    {
      return new SingleResultModification (ReturnDefaultWhenEmpty);
    }

    public override IEnumerable ExecuteInMemory<T> (IEnumerable<T> items)
    {
      ArgumentUtility.CheckNotNull ("items", items);

      if (ReturnDefaultWhenEmpty)
        return new[] { items.SingleOrDefault() };
      else
        return new[] { items.Single() };
    }

    public override string ToString ()
    {
      if (ReturnDefaultWhenEmpty)
        return "SingleOrDefault()";
      else
        return "Single()";
    }
  }
}
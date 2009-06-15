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
using System.Linq;

namespace Remotion.Data.Linq.Clauses.ResultModifications
{
  public class TakeResultModification : ResultModificationBase
  {
    public int Count { get; set; }

    public TakeResultModification (SelectClause selectClause, int count)
        : base (selectClause)
    {
      Count = count;
    }

    public override ResultModificationBase Clone (SelectClause newSelectClause)
    {
      return new TakeResultModification (newSelectClause, Count);
    }

    public override IEnumerable ExecuteInMemory<T> (IEnumerable<T> items)
    {
      ArgumentUtility.CheckNotNull ("items", items);
      return items.Take (Count);
    }

    public override object ConvertStreamToResult<T> (IEnumerable<T> stream)
    {
      ArgumentUtility.CheckNotNull ("stream", stream);
      return stream;
    }
  }
}
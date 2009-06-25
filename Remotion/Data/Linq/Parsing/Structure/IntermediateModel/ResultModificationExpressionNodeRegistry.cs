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

namespace Remotion.Data.Linq.Parsing.Structure.IntermediateModel
{
  public class ResultModificationExpressionNodeRegistry : IEnumerable<ResultModificationExpressionNodeBase>
  {
    private readonly List<ResultModificationExpressionNodeBase> _resultModificationNodes = new List<ResultModificationExpressionNodeBase> ();

    public void AddResultModificationNode (ResultModificationExpressionNodeBase node)
    {
      ArgumentUtility.CheckNotNull ("node", node);
      _resultModificationNodes.Add (node);
    }

    public IEnumerator<ResultModificationExpressionNodeBase> GetEnumerator ()
    {
      return _resultModificationNodes.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator ()
    {
      return GetEnumerator();
    }

    public void ApplyAll (QueryModel queryModel, ClauseGenerationContext clauseGenerationContext)
    {
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);

      foreach (var node in _resultModificationNodes)
        node.Apply (queryModel, clauseGenerationContext);
    }
  }
}
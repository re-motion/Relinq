// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// re-linq is free software; you can redistribute it and/or modify it under 
// the terms of the GNU Lesser General Public License as published by the 
// Free Software Foundation; either version 2.1 of the License, 
// or (at your option) any later version.
// 
// re-linq is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-linq; if not, see http://www.gnu.org/licenses.
// 
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Utilities;

namespace Remotion.Linq.Clauses
{
  /// <summary>
  /// Aggregates all objects needed in the process of cloning a <see cref="QueryModel"/> and its clauses.
  /// </summary>
  public class CloneContext
  {
    public CloneContext (QuerySourceMapping querySourceMapping)
    {
      ArgumentUtility.CheckNotNull ("querySourceMapping", querySourceMapping);

      QuerySourceMapping = querySourceMapping;
    }

    /// <summary>
    /// Gets the clause mapping used during the cloning process. This is used to adjust the <see cref="QuerySourceReferenceExpression"/> instances
    /// of clauses to point to clauses in the cloned <see cref="QueryModel"/>.
    /// </summary>
    public QuerySourceMapping QuerySourceMapping { get; private set; }
  }
}

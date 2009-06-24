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
using System.Collections.Generic;
using System.Linq.Expressions;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Clauses
{
  /// <summary>
  /// Base class for all kinds of from clauses in <see cref="QueryModel"/>
  /// </summary>
  public abstract class FromClauseBase : IResolveableClause
  {
    private ParameterExpression _identifier;

    /// <summary>
    /// Initialize a new instance of <see cref="FromClauseBase"/>
    /// </summary>
    /// <param name="previousClause">The previous clause of the current from clause.</param>
    /// <param name="identifier">The identifier of the from clause</param>
    protected FromClauseBase (IClause previousClause, ParameterExpression identifier)
    {     
      ArgumentUtility.CheckNotNull ("identifier", identifier);

      _identifier = identifier;
      PreviousClause = previousClause;
      JoinClauses = new List<JoinClause>();
    }

    public IClause PreviousClause { get; private set; }

    public ParameterExpression Identifier
    {
      get { return _identifier; }
      set { _identifier = ArgumentUtility.CheckNotNull ("value", value); }
    }

    public List<JoinClause> JoinClauses {get;private set; }

    /// <summary>
    /// Method for getting source of a from clause.
    /// </summary>
    /// <param name="databaseInfo"></param>
    /// <returns><see cref="IColumnSource"/></returns>
    public virtual IColumnSource GetColumnSource (IDatabaseInfo databaseInfo)
    {
      ArgumentUtility.CheckNotNull ("databaseInfo", databaseInfo);
      return DatabaseInfoUtility.GetTableForFromClause (databaseInfo, this);
    }

    public abstract void Accept (IQueryVisitor visitor);
    public abstract Type GetQuerySourceType ();

    protected void AddClonedJoinClauses (IEnumerable<JoinClause> originalJoinClauses, CloneContext cloneContext)
    {
      ArgumentUtility.CheckNotNull ("originalJoinClauses", originalJoinClauses);
      ArgumentUtility.CheckNotNull ("cloneContext", cloneContext);

      foreach (var joinClause in originalJoinClauses)
      {
        var joinClauseClone = joinClause.Clone (cloneContext);
        JoinClauses.Add (joinClauseClone);
      }
    }
  }
}

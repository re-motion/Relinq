// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2008 rubicon informationstechnologie gmbh, www.rubicon.eu
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
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing.FieldResolving;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Clauses
{
  /// <summary>
  /// Base class for all kinds of from clauses in <see cref="QueryModel"/>
  /// </summary>
  public abstract class FromClauseBase : IResolveableClause
  {
    private readonly ParameterExpression _identifier;
    private readonly List<JoinClause> _joinClauses = new List<JoinClause>();

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
    }

    public IClause PreviousClause { get; private set; }

    public ParameterExpression Identifier
    {
      get { return _identifier; }
    }

    public ReadOnlyCollection<JoinClause> JoinClauses
    {
      get { return _joinClauses.AsReadOnly(); }
    }

    /// <summary>
    /// Method for adding a <see cref="JoinClause"/>
    /// </summary>
    /// <param name="joinClause"><see cref="JoinClause"/></param>
    public void Add (JoinClause joinClause)
    {
      ArgumentUtility.CheckNotNull ("joinClause", joinClause);
      _joinClauses.Add (joinClause);
    }

    /// <summary>
    /// Method for getting source of a from clause.
    /// </summary>
    /// <param name="databaseInfo"></param>
    /// <returns><see cref="IColumnSource"/></returns>
    public virtual IColumnSource GetFromSource (IDatabaseInfo databaseInfo)
    {
      ArgumentUtility.CheckNotNull ("databaseInfo", databaseInfo);
      return DatabaseInfoUtility.GetTableForFromClause (databaseInfo, this);
    }

    public FieldDescriptor ResolveField (ClauseFieldResolver resolver, Expression partialFieldExpression, Expression fullFieldExpression, JoinedTableContext joinedTableContext)
    {
      ArgumentUtility.CheckNotNull ("resolver", resolver);
      ArgumentUtility.CheckNotNull ("partialFieldExpression", partialFieldExpression);
      ArgumentUtility.CheckNotNull ("fullFieldExpression", fullFieldExpression);
      ArgumentUtility.CheckNotNull ("joinedTableContext", joinedTableContext);

      return resolver.ResolveField (GetFromSource(resolver.DatabaseInfo), Identifier, partialFieldExpression, fullFieldExpression, joinedTableContext);
    }

    public abstract void Accept (IQueryVisitor visitor);
    public abstract Type GetQuerySourceType ();

    protected void AddClonedJoinClauses (IEnumerable<JoinClause> originalJoinClauses)
    {
      ArgumentUtility.CheckNotNull ("originalJoinClauses", originalJoinClauses);

      IClause previousClause = this;
      foreach (var joinClause in originalJoinClauses)
      {
        var joinClauseClone = joinClause.Clone (previousClause, this);
        Add (joinClauseClone);
        previousClause = joinClauseClone;
      }
    }
  }
}

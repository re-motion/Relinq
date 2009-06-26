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
using Remotion.Data.Linq.Clauses;
using Remotion.Utilities;

namespace Remotion.Data.Linq
{
  /// <summary>
  /// Provides a default implementation of <see cref="IQueryModelVisitor"/> which automatically visits child items. That is, the default 
  /// implementation of <see cref="VisitQueryModel"/> automatically calls <see cref="IClause.Accept"/> on all clauses in the <see cref="QueryModel"/>,
  /// the default implementation of <see cref="VisitMainFromClause"/> automatically calls <see cref="JoinClause.Accept"/> on the 
  /// <see cref="JoinClause"/> instances in its <see cref="FromClauseBase.JoinClauses"/> collection, and so on.
  /// </summary>
  public abstract class QueryModelVisitorBase : IQueryModelVisitor
  {
    public virtual void VisitQueryModel (QueryModel queryModel)
    {
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);

      queryModel.MainFromClause.Accept (this);
      VisitBodyClauses (queryModel, queryModel.BodyClauses);
      queryModel.SelectOrGroupClause.Accept (this);
    }

    public virtual void VisitMainFromClause (MainFromClause fromClause)
    {
      ArgumentUtility.CheckNotNull ("fromClause", fromClause);
      VisitJoinClauses (fromClause, fromClause.JoinClauses);
    }

    public virtual void VisitAdditionalFromClause (AdditionalFromClause fromClause)
    {
      ArgumentUtility.CheckNotNull ("fromClause", fromClause);
      VisitJoinClauses (fromClause, fromClause.JoinClauses);
    }

    public virtual void VisitJoinClause (JoinClause joinClause)
    {
      // nothing to do here
    }

    public virtual void VisitWhereClause (WhereClause whereClause)
    {
      // nothing to do here
    }

    public virtual void VisitOrderByClause (OrderByClause orderByClause)
    {
      ArgumentUtility.CheckNotNull ("orderByClause", orderByClause);
      VisitOrderings (orderByClause, orderByClause.Orderings);
    }

    public virtual void VisitOrdering (Ordering ordering)
    {
      // nothing to do here
    }

    public virtual void VisitSelectClause (SelectClause selectClause)
    {
      ArgumentUtility.CheckNotNull ("selectClause", selectClause);
      VisitResultModifications (selectClause, selectClause.ResultModifications);
    }

    public virtual void VisitResultModification (ResultModificationBase resultModification)
    {
      // nothing to do here
    }

    public virtual void VisitGroupClause (GroupClause groupClause)
    {
      // nothing to do here
    }

    protected virtual void VisitBodyClauses (QueryModel queryModel, IList<IBodyClause> bodyClauses)
    {
      foreach (var bodyClause in queryModel.BodyClauses)
        bodyClause.Accept (this);
    }

    protected virtual void VisitJoinClauses (FromClauseBase fromClause, IList<JoinClause> joinClauses)
    {
      foreach (var joinClause in joinClauses)
        joinClause.Accept (this);
    }

    protected virtual void VisitOrderings (OrderByClause orderByClause, IList<Ordering> orderings)
    {
      foreach (var ordering in orderings)
        ordering.Accept (this);
    }

    protected virtual void VisitResultModifications (SelectClause selectClause, IList<ResultModificationBase> resultModifications)
    {
      foreach (var resultModification in resultModifications)
        resultModification.Accept (this);
    }
  }
}
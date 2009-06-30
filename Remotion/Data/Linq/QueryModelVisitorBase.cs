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
using Remotion.Collections;
using Remotion.Data.Linq.Clauses;
using Remotion.Utilities;

namespace Remotion.Data.Linq
{
  /// <summary>
  /// Provides a default implementation of <see cref="IQueryModelVisitor"/> which automatically visits child items. That is, the default 
  /// implementation of <see cref="VisitQueryModel"/> automatically calls <c>Accept</c> on all clauses in the <see cref="QueryModel"/>,
  /// the default implementation of <see cref="VisitMainFromClause"/> automatically calls <see cref="JoinClause.Accept"/> on the 
  /// <see cref="JoinClause"/> instances in its <see cref="FromClauseBase.JoinClauses"/> collection, and so on.
  /// </summary>
  public abstract class QueryModelVisitorBase : IQueryModelVisitor
  {
    private QueryModel _currentQueryModel;
    private int _currentJoinClauseIndex = -1;
    private int _currentOrderingIndex = -1;

    public QueryModel CurrentQueryModel
    {
      get
      {
        if (_currentQueryModel == null)
          throw new InvalidOperationException ("CurrentQueryModel can only be called after a QueryModel has accepted this visitor.");
        return _currentQueryModel;
      }
      private set { _currentQueryModel = value; }
    }

    public int CurrentJoinClauseIndex
    {
      get
      {
        if (_currentJoinClauseIndex == -1)
          throw new InvalidOperationException ("CurrentJoinClauseIndex can only be called while VisitJoinClauses is visiting join clauses.");
        return _currentJoinClauseIndex;
      }
      protected set { _currentJoinClauseIndex = value; }
    }

    public int CurrentOrderingIndex
    {
      get
      {
        if (_currentOrderingIndex == -1)
          throw new InvalidOperationException ("CurrentOrderingIndex can only be called while VisitCurrentOrderings is visiting orderings.");
        return _currentOrderingIndex;
      }
      protected set { _currentOrderingIndex = value; }
    }

    void IQueryModelVisitor.VisitQueryModel (QueryModel queryModel)
    {
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);

      CurrentQueryModel = queryModel;
      VisitQueryModel (queryModel);
    }

    public virtual void VisitQueryModel (QueryModel queryModel)
    {
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);

      queryModel.MainFromClause.Accept (this, queryModel);
      VisitBodyClauses (queryModel, queryModel.BodyClauses);
      queryModel.SelectOrGroupClause.Accept (this, queryModel);
    }

    public virtual void VisitMainFromClause (MainFromClause fromClause, QueryModel queryModel)
    {
      ArgumentUtility.CheckNotNull ("fromClause", fromClause);
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);

      VisitJoinClauses (queryModel, fromClause, fromClause.JoinClauses);
    }

    public virtual void VisitAdditionalFromClause (AdditionalFromClause fromClause, QueryModel queryModel, int index)
    {
      ArgumentUtility.CheckNotNull ("fromClause", fromClause);
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);

      VisitJoinClauses (queryModel, fromClause, fromClause.JoinClauses);
    }

    public virtual void VisitJoinClause (JoinClause joinClause, QueryModel queryModel, FromClauseBase fromClause, int index)
    {
      ArgumentUtility.CheckNotNull ("joinClause", joinClause);
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);
      ArgumentUtility.CheckNotNull ("fromClause", fromClause);

      // nothing to do here
    }

    public virtual void VisitWhereClause (WhereClause whereClause, QueryModel queryModel, int index)
    {
      ArgumentUtility.CheckNotNull ("whereClause", whereClause);
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);

      // nothing to do here
    }

    public virtual void VisitOrderByClause (OrderByClause orderByClause, QueryModel queryModel, int index)
    {
      ArgumentUtility.CheckNotNull ("orderByClause", orderByClause);
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);

      VisitOrderings (orderByClause, orderByClause.Orderings);
    }

    public virtual void VisitOrdering (Ordering ordering)
    {
      // nothing to do here
    }

    public virtual void VisitSelectClause (SelectClause selectClause, QueryModel queryModel)
    {
      ArgumentUtility.CheckNotNull ("selectClause", selectClause);
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);

      VisitResultModifications (queryModel, selectClause, selectClause.ResultModifications);
    }

    public virtual void VisitResultModification (ResultModificationBase resultModification, QueryModel queryModel, SelectClause selectClause, int index)
    {
      ArgumentUtility.CheckNotNull ("resultModification", resultModification);
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);
      ArgumentUtility.CheckNotNull ("selectClause", selectClause);

      // nothing to do here
    }

    public virtual void VisitGroupClause (GroupClause groupClause, QueryModel queryModel)
    {
      ArgumentUtility.CheckNotNull ("groupClause", groupClause);
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);

      // nothing to do here
    }

    protected virtual void VisitBodyClauses (QueryModel queryModel, ObservableCollection<IBodyClause> bodyClauses)
    {
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);
      ArgumentUtility.CheckNotNull ("bodyClauses", bodyClauses);

      foreach (var indexValuePair in bodyClauses.AsChangeResistantEnumerableWithIndex())
        indexValuePair.Value.Accept (this, queryModel, indexValuePair.Index);
    }

    protected virtual void VisitJoinClauses (QueryModel queryModel, FromClauseBase fromClause, ObservableCollection<JoinClause> joinClauses)
    {
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);
      ArgumentUtility.CheckNotNull ("fromClause", fromClause);
      ArgumentUtility.CheckNotNull ("joinClauses", joinClauses);

      foreach (var indexValuePair in joinClauses.AsChangeResistantEnumerableWithIndex ())
        indexValuePair.Value.Accept (this, queryModel, fromClause, indexValuePair.Index);
    }

    protected virtual void VisitOrderings (OrderByClause orderByClause, ObservableCollection<Ordering> orderings)
    {
      ArgumentUtility.CheckNotNull ("orderByClause", orderByClause);
      ArgumentUtility.CheckNotNull ("orderings", orderings);

      VisitCollection (orderings, ordering => ordering.Accept (this), index => CurrentOrderingIndex = index);
    }

    protected virtual void VisitResultModifications (QueryModel queryModel, SelectClause selectClause, ObservableCollection<ResultModificationBase> resultModifications)
    {
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);
      ArgumentUtility.CheckNotNull ("selectClause", selectClause);
      ArgumentUtility.CheckNotNull ("resultModifications", resultModifications);

      foreach (var indexValuePair in resultModifications.AsChangeResistantEnumerableWithIndex ())
        indexValuePair.Value.Accept (this, queryModel, selectClause, indexValuePair.Index);
    }

    private void VisitCollection<T> (ObservableCollection<T> collection, Action<T> acceptAction, Action<int> indexSetter)
    {
      try
      {
        foreach (var itemWithIndex in collection.AsChangeResistantEnumerableWithIndex())
        {
          indexSetter (itemWithIndex.Index);
          acceptAction (itemWithIndex.Value);
        }
      }
      finally
      {
        indexSetter (-1);
      }
    }
  }
}
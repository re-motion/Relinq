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
using Remotion.Linq.Clauses;
using Remotion.Linq.Collections;
using Remotion.Linq.Utilities;

namespace Remotion.Linq
{
  /// <summary>
  /// Provides a default implementation of <see cref="IQueryModelVisitor"/> which automatically visits child items. That is, the default 
  /// implementation of <see cref="VisitQueryModel"/> automatically calls <c>Accept</c> on all clauses in the <see cref="QueryModel"/>
  /// and the default implementation of <see cref="VisitOrderByClause"/> automatically calls <see cref="Ordering.Accept"/> on the 
  /// <see cref="Ordering"/> instances in its <see cref="OrderByClause.Orderings"/> collection, and so on.
  /// </summary>
  /// <remarks>
  /// This visitor is hardened against modifications performed on the visited <see cref="QueryModel"/> while the model is currently being visited.
  /// That is, if a the <see cref="QueryModel.BodyClauses"/> collection changes while a body clause (or a child item of a body clause) is currently 
  /// being processed, the visitor will handle that gracefully. The same applies to <see cref="QueryModel.ResultOperators"/> and
  /// <see cref="OrderByClause.Orderings"/>.
  /// </remarks>
  public abstract class QueryModelVisitorBase : IQueryModelVisitor
  {
    public virtual void VisitQueryModel (QueryModel queryModel)
    {
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);

      queryModel.MainFromClause.Accept (this, queryModel);
      VisitBodyClauses (queryModel.BodyClauses, queryModel);
      queryModel.SelectClause.Accept (this, queryModel);

      VisitResultOperators (queryModel.ResultOperators, queryModel);
    }

    public virtual void VisitMainFromClause (MainFromClause fromClause, QueryModel queryModel)
    {
      ArgumentUtility.CheckNotNull ("fromClause", fromClause);
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);

      // nothing to do here      
    }

    public virtual void VisitAdditionalFromClause (AdditionalFromClause fromClause, QueryModel queryModel, int index)
    {
      ArgumentUtility.CheckNotNull ("fromClause", fromClause);
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);

      // nothing to do here
    }

    public virtual void VisitJoinClause (JoinClause joinClause, QueryModel queryModel, int index)
    {
      ArgumentUtility.CheckNotNull ("joinClause", joinClause);
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);
      
      // nothing to do here
    }

    public virtual void VisitJoinClause (JoinClause joinClause, QueryModel queryModel, GroupJoinClause groupJoinClause)
    {
      ArgumentUtility.CheckNotNull ("joinClause", joinClause);
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);
      ArgumentUtility.CheckNotNull ("groupJoinClause", groupJoinClause);

      // nothing to do here
    }

    public virtual void VisitGroupJoinClause (GroupJoinClause groupJoinClause, QueryModel queryModel, int index)
    {
      ArgumentUtility.CheckNotNull ("groupJoinClause", groupJoinClause);
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);

      groupJoinClause.JoinClause.Accept (this, queryModel, groupJoinClause);
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

      VisitOrderings (orderByClause.Orderings, queryModel, orderByClause);
    }

    public virtual void VisitOrdering (Ordering ordering, QueryModel queryModel, OrderByClause orderByClause, int index)
    {
      ArgumentUtility.CheckNotNull ("ordering", ordering);
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);
      ArgumentUtility.CheckNotNull ("orderByClause", orderByClause);

      // nothing to do here
    }

    public virtual void VisitSelectClause (SelectClause selectClause, QueryModel queryModel)
    {
      ArgumentUtility.CheckNotNull ("selectClause", selectClause);
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);

      // nothing to do here
    }

    public virtual void VisitResultOperator (ResultOperatorBase resultOperator, QueryModel queryModel, int index)
    {
      ArgumentUtility.CheckNotNull ("resultOperator", resultOperator);
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);

      // nothing to do here
    }

    protected virtual void VisitBodyClauses (ObservableCollection<IBodyClause> bodyClauses, QueryModel queryModel)
    {
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);
      ArgumentUtility.CheckNotNull ("bodyClauses", bodyClauses);

      foreach (var indexValuePair in bodyClauses.AsChangeResistantEnumerableWithIndex())
        indexValuePair.Value.Accept (this, queryModel, indexValuePair.Index);
    }

    protected virtual void VisitOrderings (ObservableCollection<Ordering> orderings, QueryModel queryModel, OrderByClause orderByClause)
    {
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);
      ArgumentUtility.CheckNotNull ("orderByClause", orderByClause);
      ArgumentUtility.CheckNotNull ("orderings", orderings);

      foreach (var indexValuePair in orderings.AsChangeResistantEnumerableWithIndex())
        indexValuePair.Value.Accept (this, queryModel, orderByClause, indexValuePair.Index);
    }

    protected virtual void VisitResultOperators (ObservableCollection<ResultOperatorBase> resultOperators, QueryModel queryModel)
    {
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);
      ArgumentUtility.CheckNotNull ("resultOperators", resultOperators);

      foreach (var indexValuePair in resultOperators.AsChangeResistantEnumerableWithIndex())
        indexValuePair.Value.Accept (this, queryModel, indexValuePair.Index);
    }
  }
}

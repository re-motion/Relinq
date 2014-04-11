// Copyright (c) rubicon IT GmbH, www.rubicon.eu
//
// See the NOTICE file distributed with this work for additional information
// regarding copyright ownership.  rubicon licenses this file to you under 
// the Apache License, Version 2.0 (the "License"); you may not use this 
// file except in compliance with the License.  You may obtain a copy of the 
// License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, WITHOUT 
// WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.  See the 
// License for the specific language governing permissions and limitations
// under the License.
// 
using System;
using System.Collections.ObjectModel;
using Remotion.Linq.Clauses;
using Remotion.Linq.Collections;
using Remotion.Utilities;

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

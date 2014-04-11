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
using Remotion.Linq.Clauses;

namespace Remotion.Linq
{
  /// <summary>
  /// Defines an interface for visiting the clauses of a <see cref="QueryModel"/>.
  /// </summary>
  /// <remarks>
  /// <para>
  /// When implement this interface, implement <see cref="VisitQueryModel"/>, then call <c>Accept</c> on every clause that should
  /// be visited. Child clauses, joins, orderings, and result operators are not visited automatically; they always need to be explicitly visited 
  /// via <see cref="IBodyClause.Accept"/>, <see cref="JoinClause.Accept(IQueryModelVisitor, QueryModel, int)"/>, <see cref="Ordering.Accept"/>, 
  /// <see cref="ResultOperatorBase.Accept"/>, and so on.
  /// </para>
  /// <para>
  /// <see cref="QueryModelVisitorBase"/> provides a robust default implementation of this interface that can be used as a base for other visitors.
  /// </para>
  /// </remarks>
  public interface IQueryModelVisitor
  {
    void VisitQueryModel (QueryModel queryModel);
    void VisitMainFromClause (MainFromClause fromClause, QueryModel queryModel);
    void VisitAdditionalFromClause (AdditionalFromClause fromClause, QueryModel queryModel, int index);
    void VisitJoinClause (JoinClause joinClause, QueryModel queryModel, int index);
    void VisitJoinClause (JoinClause joinClause, QueryModel queryModel, GroupJoinClause groupJoinClause);
    void VisitGroupJoinClause (GroupJoinClause joinClause, QueryModel queryModel, int index);
    void VisitWhereClause (WhereClause whereClause, QueryModel queryModel, int index);
    void VisitOrderByClause (OrderByClause orderByClause, QueryModel queryModel, int index);
    void VisitOrdering (Ordering ordering, QueryModel queryModel, OrderByClause orderByClause, int index);
    void VisitSelectClause (SelectClause selectClause, QueryModel queryModel);
    void VisitResultOperator (ResultOperatorBase resultOperator, QueryModel queryModel, int index);
  }
}

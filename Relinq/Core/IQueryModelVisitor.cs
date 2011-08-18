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

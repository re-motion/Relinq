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
using Remotion.Data.Linq.Clauses;

namespace Remotion.Data.Linq
{
  /// <summary>
  /// Defines an interface for visiting the clauses of a <see cref="QueryModel"/>. 
  /// When implement this interface, implement <see cref="VisitQueryModel"/>, then call <c>Accept</c> on every clause that should
  /// be visited. Note that clauses, orderings, and result modifications are never visited automatically, they always need to be explicitly visited 
  /// via <see cref="JoinClause.Accept"/>, <see cref="Ordering.Accept"/>, and <see cref="ResultModificationBase.Accept"/>.
  /// </summary>
  public interface IQueryModelVisitor
  {
    void VisitQueryModel (QueryModel queryModel);
    void VisitMainFromClause (MainFromClause fromClause, QueryModel queryModel);
    void VisitAdditionalFromClause (AdditionalFromClause fromClause, QueryModel queryModel, int index);
    void VisitJoinClause (JoinClause joinClause);
    void VisitWhereClause (WhereClause whereClause, QueryModel queryModel, int index);
    void VisitOrderByClause (OrderByClause orderByClause, QueryModel queryModel, int index);
    void VisitOrdering (Ordering ordering);
    void VisitSelectClause (SelectClause selectClause, QueryModel queryModel);
    void VisitResultModification (ResultModificationBase resultModification, QueryModel queryModel, SelectClause selectClause, int index);
    void VisitGroupClause (GroupClause groupClause, QueryModel queryModel);
  }
}

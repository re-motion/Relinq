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
using System.Collections.Generic;
using System.Reflection;
using Remotion.Collections;
using System.Linq.Expressions;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.DataObjectModel;

namespace Remotion.Data.Linq.Parsing.FieldResolving
{
  /// <summary>
  /// Defines how members are resolved for a specific resolution use case.
  /// </summary>
  /// <remarks>
  /// <para>
  /// Resolution of fields in select, where, and orderby clauses works very similarly. For example, <c>customer.Order.OrderNumber</c> always results
  /// in a <see cref="Column"/> field representing the property <c>OrderNumber</c> with a <see cref="FieldSourcePath"/> of <c>customer.Order</c> (including
  /// a <see cref="SingleJoin"/> between <c>Customer</c> and <c>Order</c>).
  /// </para>
  /// <para>
  /// However, there are a few specifics to the way how expressions that access entities (such as in relations, e.g. <c>customer.Order</c>,
  /// or by directly accessing the identifier, e.g. <c>customer</c>) should be handled. By implementing this policy, a user of the resolver can
  /// influence how such cases are processed.
  /// </para>
  /// </remarks>
  public interface IResolveFieldAccessPolicy
  {
    Tuple<MemberInfo, IEnumerable<MemberInfo>> AdjustMemberInfosForDirectAccessOfQuerySource (QuerySourceReferenceExpression referenceExpression);
    Tuple<MemberInfo, IEnumerable<MemberInfo>> AdjustMemberInfosForRelation (MemberInfo accessedMember, IEnumerable<MemberInfo> joinMembers);
    bool OptimizeRelatedKeyAccess ();
  }
}

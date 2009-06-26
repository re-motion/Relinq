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
using System.Text;
using Remotion.Data.Linq.Clauses;
using Remotion.Utilities;

namespace Remotion.Data.Linq.StringBuilding
{
  public class StringBuildingQueryModelVisitor : QueryModelVisitorBase
  {
    private readonly StringBuilder _sb = new StringBuilder ();

    public override void VisitMainFromClause (MainFromClause fromClause)
    {
      ArgumentUtility.CheckNotNull ("fromClause", fromClause);

      _sb.AppendFormat ("from {0} {1} in {2} ", fromClause.ItemType.Name, fromClause.ItemName, FormatExpression (fromClause.FromExpression));
      base.VisitMainFromClause (fromClause);
    }

    public override void VisitAdditionalFromClause (AdditionalFromClause fromClause)
    {
      ArgumentUtility.CheckNotNull ("fromClause", fromClause);

      _sb.AppendFormat ("from {0} {1} in {2} ", fromClause.ItemType.Name, fromClause.ItemName, FormatExpression (fromClause.FromExpression));
      base.VisitAdditionalFromClause (fromClause);
    }

    public override void VisitMemberFromClause (MemberFromClause fromClause)
    {
      VisitAdditionalFromClause (fromClause);
    }

    public override void VisitSubQueryFromClause (SubQueryFromClause fromClause)
    {
      ArgumentUtility.CheckNotNull ("fromClause", fromClause);

      _sb.AppendFormat ("from {0} {1} in ({2}) ", fromClause.ItemType.Name, fromClause.ItemName, fromClause.SubQueryModel);
      base.VisitSubQueryFromClause (fromClause);
    }

    public override void VisitJoinClause (JoinClause joinClause)
    {
      ArgumentUtility.CheckNotNull ("joinClause", joinClause);

      _sb.AppendFormat (
          "join {0} {1} in {2} on {3} equals {4} ",
          joinClause.ItemName, 
          joinClause.ItemType, 
          FormatExpression (joinClause.InExpression),
          FormatExpression (joinClause.OnExpression), 
          FormatExpression (joinClause.EqualityExpression));
      base.VisitJoinClause (joinClause);
    }

    public override void VisitWhereClause (WhereClause whereClause)
    {
      ArgumentUtility.CheckNotNull ("whereClause", whereClause);

      _sb.AppendFormat ("where {0} ", FormatExpression (whereClause.Predicate));
      base.VisitWhereClause (whereClause);
    }

    public override void VisitOrderByClause (OrderByClause orderByClause)
    {
      ArgumentUtility.CheckNotNull ("orderByClause", orderByClause);

      _sb.Append ("orderby ");
      base.VisitOrderByClause (orderByClause);
    }

    public override void VisitOrdering (Ordering ordering)
    {
      ArgumentUtility.CheckNotNull ("ordering", ordering);

      switch (ordering.OrderingDirection)
      {
        case OrderingDirection.Asc:
          _sb.AppendFormat ("{0} ascending ", FormatExpression (ordering.Expression));
          break;
        case OrderingDirection.Desc:
          _sb.AppendFormat ("{0} descending ", FormatExpression (ordering.Expression));
          break;
      }
      base.VisitOrdering (ordering);
    }

    public override void VisitSelectClause (SelectClause selectClause)
    {
      ArgumentUtility.CheckNotNull ("selectClause", selectClause);
      
      _sb.AppendFormat ("select {0}", FormatExpression (selectClause.Selector));
      base.VisitSelectClause (selectClause);
    }

    protected override void VisitResultModifications (SelectClause selectClause, IList<ResultModificationBase> resultModifications)
    {
      ArgumentUtility.CheckNotNull ("resultModifications", resultModifications);

      if (resultModifications.Count > 0)
      {
        _sb.Insert (0, "(");
        _sb.Append (")");
      }

      base.VisitResultModifications (selectClause, resultModifications);
    }

    public override void VisitResultModification (ResultModificationBase resultModification)
    {
      ArgumentUtility.CheckNotNull ("resultModification", resultModification);

      _sb.Append (".");
      _sb.Append (resultModification.ToString ());
      base.VisitResultModification (resultModification);
    }

    public override void VisitGroupClause (GroupClause groupClause)
    {
      ArgumentUtility.CheckNotNull ("groupClause", groupClause);
      
      _sb.AppendFormat ("group {0} by {1}", FormatExpression (groupClause.GroupExpression), FormatExpression (groupClause.ByExpression));
      base.VisitGroupClause (groupClause);
    }

    public override string ToString ()
    {
      return _sb.ToString();
    }

    private string FormatExpression (Expression expression)
    {
      if (expression != null)
        return FormattingExpressionTreeVisitor.Format (expression);
      else
        return "<null>";
    }
  }
}
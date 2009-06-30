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
using System.Linq.Expressions;
using System.Text;
using Remotion.Collections;
using Remotion.Data.Linq.Clauses;
using Remotion.Utilities;

namespace Remotion.Data.Linq.StringBuilding
{
  public class StringBuildingQueryModelVisitor : QueryModelVisitorBase
  {
    private readonly StringBuilder _sb = new StringBuilder ();

    public override void VisitMainFromClause (MainFromClause fromClause, QueryModel queryModel)
    {
      ArgumentUtility.CheckNotNull ("fromClause", fromClause);
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);

      _sb.AppendFormat ("from {0} {1} in {2} ", fromClause.ItemType.Name, fromClause.ItemName, FormatExpression (fromClause.FromExpression));
      base.VisitMainFromClause (fromClause, queryModel);
    }

    public override void VisitAdditionalFromClause (AdditionalFromClause fromClause, QueryModel queryModel, int index)
    {
      ArgumentUtility.CheckNotNull ("fromClause", fromClause);
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);
      
      _sb.AppendFormat ("from {0} {1} in {2} ", fromClause.ItemType.Name, fromClause.ItemName, FormatExpression (fromClause.FromExpression));
      base.VisitAdditionalFromClause (fromClause, queryModel, index);
    }

    public override void VisitJoinClause (JoinClause joinClause, QueryModel queryModel, FromClauseBase fromClause, int index)
    {
      ArgumentUtility.CheckNotNull ("joinClause", joinClause);
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);
      ArgumentUtility.CheckNotNull ("fromClause", fromClause);

      _sb.AppendFormat (
          "join {0} {1} in {2} on {3} equals {4} ",
          joinClause.ItemName, 
          joinClause.ItemType, 
          FormatExpression (joinClause.InExpression),
          FormatExpression (joinClause.OnExpression), 
          FormatExpression (joinClause.EqualityExpression));
      base.VisitJoinClause (joinClause, queryModel, fromClause, index);
    }

    public override void VisitWhereClause (WhereClause whereClause, QueryModel queryModel, int index)
    {
      ArgumentUtility.CheckNotNull ("whereClause", whereClause);
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);

      _sb.AppendFormat ("where {0} ", FormatExpression (whereClause.Predicate));
      base.VisitWhereClause (whereClause, queryModel, index);
    }

    public override void VisitOrderByClause (OrderByClause orderByClause, QueryModel queryModel, int index)
    {
      ArgumentUtility.CheckNotNull ("orderByClause", orderByClause);
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);

      _sb.Append ("orderby ");
      base.VisitOrderByClause (orderByClause, queryModel, index);
    }

    public override void VisitOrdering (Ordering ordering, QueryModel queryModel, OrderByClause orderByClause, int index)
    {
      ArgumentUtility.CheckNotNull ("ordering", ordering);
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);
      ArgumentUtility.CheckNotNull ("orderByClause", orderByClause);
      
      switch (ordering.OrderingDirection)
      {
        case OrderingDirection.Asc:
          _sb.AppendFormat ("{0} ascending ", FormatExpression (ordering.Expression));
          break;
        case OrderingDirection.Desc:
          _sb.AppendFormat ("{0} descending ", FormatExpression (ordering.Expression));
          break;
      }
      base.VisitOrdering (ordering, queryModel, orderByClause, index);
    }

    public override void VisitSelectClause (SelectClause selectClause, QueryModel queryModel)
    {
      ArgumentUtility.CheckNotNull ("selectClause", selectClause);
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);

      _sb.AppendFormat ("select {0}", FormatExpression (selectClause.Selector));
      base.VisitSelectClause (selectClause, queryModel);
    }

    protected override void VisitResultModifications (ObservableCollection<ResultModificationBase> resultModifications, QueryModel queryModel, SelectClause selectClause)
    {
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);
      ArgumentUtility.CheckNotNull ("selectClause", selectClause);
      ArgumentUtility.CheckNotNull ("resultModifications", resultModifications);
      
      if (resultModifications.Count > 0)
      {
        _sb.Insert (0, "(");
        _sb.Append (")");
      }

      base.VisitResultModifications (resultModifications, queryModel, selectClause);
    }

    public override void VisitResultModification (ResultModificationBase resultModification, QueryModel queryModel, SelectClause selectClause, int index)
    {
      ArgumentUtility.CheckNotNull ("resultModification", resultModification);
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);
      ArgumentUtility.CheckNotNull ("selectClause", selectClause);

      _sb.Append (".");
      _sb.Append (resultModification.ToString ());
      base.VisitResultModification (resultModification, queryModel, selectClause, index);
    }

    public override void VisitGroupClause (GroupClause groupClause, QueryModel queryModel)
    {
      ArgumentUtility.CheckNotNull ("groupClause", groupClause);
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);
      
      _sb.AppendFormat ("group {0} by {1}", FormatExpression (groupClause.GroupExpression), FormatExpression (groupClause.ByExpression));
      base.VisitGroupClause (groupClause, queryModel);
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
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
using System.Linq.Expressions;
using System.Text;
using Remotion.Data.Linq.Clauses;
using Remotion.Utilities;

namespace Remotion.Data.Linq.StringBuilding
{
  public class StringBuildingQueryVisitor : IQueryVisitor
  {
    private readonly StringBuilder _sb = null;

    public StringBuildingQueryVisitor()
    {
      _sb = new StringBuilder();
    }

    public void VisitQueryModel (QueryModel queryModel)
    {
      ArgumentUtility.CheckNotNull ("queryExpression", queryModel);

      queryModel.MainFromClause.Accept (this);
      foreach (IBodyClause bodyClause in queryModel.BodyClauses)
        bodyClause.Accept (this);

      queryModel.SelectOrGroupClause.Accept (this);
    }

    public void VisitMainFromClause (MainFromClause fromClause)
    {
      ArgumentUtility.CheckNotNull ("fromClause", fromClause);
      _sb.AppendFormat ("from {0} {1} in {2} ", fromClause.Identifier.Type.Name, fromClause.Identifier.Name, FormatExpression (fromClause.QuerySource));

      foreach (JoinClause jc in fromClause.JoinClauses)
        jc.Accept (this);
    }

    public void VisitAdditionalFromClause (AdditionalFromClause fromClause)
    {
      ArgumentUtility.CheckNotNull ("fromClause", fromClause);

      _sb.AppendFormat ("from {0} {1} in {2} ", fromClause.Identifier.Type.Name, fromClause.Identifier.Name, FormatExpression (fromClause.FromExpression));
      foreach (JoinClause jc in fromClause.JoinClauses)
        jc.Accept (this);
    }

    public void VisitMemberFromClause (MemberFromClause fromClause)
    {
      VisitAdditionalFromClause (fromClause);
    }

    public void VisitSubQueryFromClause (SubQueryFromClause fromClause)
    {
      ArgumentUtility.CheckNotNull ("fromClause", fromClause);
      _sb.AppendFormat ("from {0} {1} in ({2}) ", fromClause.Identifier.Type.Name, fromClause.Identifier.Name, fromClause.SubQueryModel);
    }

    public void VisitJoinClause (JoinClause joinClause)
    {
      ArgumentUtility.CheckNotNull ("joinClause", joinClause);
      _sb.AppendFormat (
          "join {0} {1} in {2} on {3} equals {4} ",
          joinClause.ItemName, 
          joinClause.ItemType, 
          FormatExpression (joinClause.InExpression),
          FormatExpression (joinClause.OnExpression), 
          FormatExpression (joinClause.EqualityExpression));
    }

    public void VisitWhereClause (WhereClause whereClause)
    {
      ArgumentUtility.CheckNotNull ("whereClause", whereClause);

      _sb.AppendFormat ("where {0} ", FormatExpression (whereClause.Predicate));
    }

    public void VisitOrderByClause (OrderByClause orderByClause)
    {
      ArgumentUtility.CheckNotNull ("orderByClause", orderByClause);
      _sb.Append ("orderby ");
      foreach (Ordering oC in orderByClause.Orderings)
        oC.Accept (this);
    }

    public void VisitOrdering (Ordering ordering)
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
    }

    public void VisitSelectClause (SelectClause selectClause)
    {
      ArgumentUtility.CheckNotNull ("selectClause", selectClause);
      _sb.AppendFormat ("select {0}", FormatExpression (selectClause.Selector));

      if (selectClause.ResultModifications.Count > 0)
      {
        _sb.Insert (0, "(");
        _sb.Append (")");

        foreach (var resultModification in selectClause.ResultModifications)
        {
          _sb.Append (".");
          _sb.Append (resultModification.ToString ());
        }
      }
    }

    public void VisitGroupClause (GroupClause groupClause)
    {
      ArgumentUtility.CheckNotNull ("groupClause", groupClause);
      _sb.AppendFormat ("group {0} by {1}", FormatExpression (groupClause.GroupExpression), FormatExpression (groupClause.ByExpression));
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
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
using System.Runtime.CompilerServices;
using System.Text;
using Remotion.Data.Linq.Clauses;
using Remotion.Utilities;

namespace Remotion.Data.Linq.StringBuilding
{
  public class DebugStringBuildingQueryVisitor : IQueryVisitor
  {
    private readonly StringBuilder _sb = null;

    public DebugStringBuildingQueryVisitor()
    {
      _sb = new StringBuilder();
    }

    #region IQueryVisitor Members

    public void VisitQueryModel (QueryModel queryModel)
    {
      ArgumentUtility.CheckNotNull ("queryExpression", queryModel);
      _sb.Append ("\n");
      _sb.Append ("---QueryModel---");
      _sb.Append ("\n");
      _sb.AppendFormat ("ResultType: {0}", queryModel.ResultType);
      _sb.Append ("\n");

      queryModel.MainFromClause.Accept (this);
      foreach (IBodyClause bodyClause in queryModel.BodyClauses)
        bodyClause.Accept (this);

      queryModel.SelectOrGroupClause.Accept (this);
    }

    public void VisitMainFromClause (MainFromClause fromClause)
    {
      ArgumentUtility.CheckNotNull ("fromClause", fromClause);
      _sb.Append ("\n");
      _sb.Append ("---- MainFromClause ----");
      _sb.Append ("\n");
      _sb.AppendFormat ("Identifier: {0}",fromClause.Identifier.Name);
      _sb.Append ("\n");
      _sb.AppendFormat ("QuerySource: {0}", fromClause.QuerySource);
      _sb.Append ("\n");
      //_sb.AppendFormat ("from {0} {1} in {2} ", fromClause.Identifier.Type.Name, fromClause.Identifier.Name, fromClause.QuerySource);

      foreach (JoinClause jc in fromClause.JoinClauses)
        jc.Accept (this);
    }

    public void VisitAdditionalFromClause (AdditionalFromClause fromClause)
    {
      ArgumentUtility.CheckNotNull ("fromClause", fromClause);

      //string fromString;
      //MemberExpression memberExpression = fromClause.FromExpression.Body as MemberExpression;
      //if (memberExpression != null && IsCompilerGeneratedFromExpression (memberExpression))
      //  fromString = memberExpression.Member.Name;
      //else
      //  fromString = fromClause.FromExpression.Body.ToString();
      _sb.Append ("\n");
      if (fromClause.GetType () == typeof (MemberFromClause))
      {
        _sb.Append ("--- MemberFromClause (AdditionalFromClause) ---");
        _sb.Append ("\n");
        _sb.AppendFormat ("MemberExpression: {0}", ((MemberFromClause) fromClause).MemberExpression);
      }
      else
        _sb.Append ("--- AdditionalFromClause ---");
      _sb.Append ("\n");
      _sb.AppendFormat ("PreviousClause: {0}", fromClause.PreviousClause);
      _sb.Append ("\n");
      _sb.AppendFormat ("Identifier: {0}", fromClause.Identifier.Name);
      _sb.Append ("\n");
      _sb.AppendFormat ("FromExpression: {0}", fromClause.FromExpression);
      _sb.Append ("\n");
      _sb.AppendFormat ("ProjectionExpression: {0}", fromClause.ProjectionExpression);
      _sb.Append ("\n");
      //_sb.AppendFormat ("from {0} {1} in {2} ", fromClause.Identifier.Type.Name, fromClause.Identifier.Name, fromString);

      foreach (JoinClause jc in fromClause.JoinClauses)
        jc.Accept (this);
    }

    public void VisitMemberFromClause (MemberFromClause fromClause)
    {
      VisitAdditionalFromClause (fromClause);
    }

    //private bool IsCompilerGeneratedFromExpression (MemberExpression memberExpression)
    //{
    //  return memberExpression.Expression.NodeType == ExpressionType.Constant
    //      && memberExpression.Expression.Type.IsDefined (typeof (CompilerGeneratedAttribute), false);
    //}

    public void VisitSubQueryFromClause (SubQueryFromClause fromClause)
    {
      ArgumentUtility.CheckNotNull ("fromClause", fromClause);
      _sb.Append ("\n");
      _sb.Append ("---SubQueryFromClause---");
      _sb.Append ("\n");
      _sb.AppendFormat ("PreviousClause: {0}", fromClause.PreviousClause);
      _sb.Append ("\n");
      _sb.AppendFormat ("Identifier: {0}", fromClause.Identifier);
      _sb.Append ("\n");
      _sb.AppendFormat ("ProjectionExpression: {0}", fromClause.ProjectionExpression);
      _sb.Append ("\n");
      //_sb.AppendFormat ("from {0} {1} in ({2}) ", fromClause.Identifier.Type.Name, fromClause.Identifier.Name, fromClause.SubQueryModel);
    }

    public void VisitJoinClause (JoinClause joinClause)
    {
      ArgumentUtility.CheckNotNull ("joinClause", joinClause);
      _sb.Append ("\n");
      _sb.Append ("---JoinClause---");
      _sb.Append ("\n");
      _sb.AppendFormat ("PreviousClause: {0}", joinClause.PreviousClause);
      _sb.Append ("\n");
      _sb.AppendFormat ("FromClause: {0}", joinClause.FromClause);
      _sb.Append ("\n");
      _sb.AppendFormat ("Identifier: {0}", joinClause.Identifier);
      _sb.Append ("\n");
      _sb.AppendFormat ("InExpression: {0}", joinClause.InExpression);
      _sb.Append ("\n");
      _sb.AppendFormat ("OnExpression: {0}", joinClause.OnExpression);
      _sb.Append ("\n");
      _sb.AppendFormat ("Equality: {0}", joinClause.EqualityExpression);
      _sb.Append ("\n");
      _sb.AppendFormat ("IntoIdentifier: {0}", joinClause.IntoIdentifier);
      _sb.Append ("\n");
      
      //_sb.AppendFormat ("join {0} {1} in {2} on {3} equals {4} into {5} ",
      //    joinClause.Identifier.Type, joinClause.Identifier, joinClause.InExpression,
      //    joinClause.OnExpression, joinClause.EqualityExpression, joinClause.IntoIdentifier);
    }

    public void VisitLetClause (LetClause letClause)
    {
      ArgumentUtility.CheckNotNull ("letClause", letClause);
      _sb.Append ("\n");
      _sb.Append ("--- LetClause ---");
      _sb.Append ("\n");
      _sb.AppendFormat ("PreviousClause: {0}", letClause.PreviousClause);
      _sb.Append ("\n");
      _sb.AppendFormat ("Expression: {0}", letClause.Expression);
      _sb.Append ("\n");
      _sb.AppendFormat ("Identifier: {0}", letClause.Identifier);
      _sb.Append ("\n");
      _sb.AppendFormat ("ProjectionExpression: {0}", letClause.ProjectionExpression);
      _sb.Append ("\n");
      //_sb.AppendFormat ("let {0} = {1} ", letClause.Identifier, letClause.Expression);
    }

    public void VisitWhereClause (WhereClause whereClause)
    {
      ArgumentUtility.CheckNotNull ("whereClause", whereClause);
      _sb.Append ("\n");
      _sb.Append ("--- WhereClause ---");
      _sb.Append ("\n");
      _sb.AppendFormat ("PreviousClause: {0}", whereClause.PreviousClause);
      _sb.Append ("\n");
      _sb.AppendFormat ("Predicate: {0}", whereClause.Predicate);
      _sb.Append ("\n");
      //_sb.AppendFormat ("where {0} ", whereClause.Predicate.Body);
    }

    public void VisitOrderByClause (OrderByClause orderByClause)
    {
      ArgumentUtility.CheckNotNull ("orderByClause", orderByClause);
      _sb.Append ("\n");
      _sb.Append ("---OrderByClause---");
      _sb.Append ("\n");
      _sb.AppendFormat ("PreviousClause: {0}", orderByClause.PreviousClause);
      _sb.Append ("\n");
      //_sb.Append ("orderby ");
      foreach (Ordering oC in orderByClause.OrderingList)
      {
        oC.Accept (this);
      }
    }

    public void VisitOrdering (Ordering ordering)
    {
      ArgumentUtility.CheckNotNull ("ordering", ordering);

      _sb.Append ("\n");
      _sb.Append ("---Ordering---");
      _sb.Append ("\n");
      _sb.AppendFormat ("OrderByClause: {0}", ordering.OrderByClause);
      _sb.Append ("\n");
      _sb.AppendFormat ("Expression: {0}", ordering.Expression);
      _sb.Append ("\n");
      _sb.AppendFormat ("OrderingDirection: {0}", ordering.OrderingDirection);

      
      //switch (orderingClause.OrderingDirection)
      //{
      //  case OrderingDirection.Asc:
      //    _sb.AppendFormat ("{0} ascending ", orderingClause.Expression);
      //    break;
      //  case OrderingDirection.Desc:
      //    _sb.AppendFormat ("{0} descending ", orderingClause.Expression);
      //    break;
      //}
    }

    public void VisitSelectClause (SelectClause selectClause)
    {
      ArgumentUtility.CheckNotNull ("selectClause", selectClause);
      //_sb.AppendFormat ("select {0}", selectClause.Selector == null ? "<value>" : selectClause.Selector.Body.ToString());
      _sb.Append ("\n");
      _sb.Append ("--- SelectClause ---");
      _sb.Append ("\n");
      _sb.AppendFormat ("PreviousClause: {0}", selectClause.PreviousClause);
      _sb.Append ("\n");
      _sb.AppendFormat ("Selector: {0}", selectClause.Selector);
      _sb.Append ("\n");
      //_sb.AppendFormat ("", selectClause.ResultModifiers);
      //_sb.Append ("\n");

    }

    public void VisitGroupClause (GroupClause groupClause)
    {
      ArgumentUtility.CheckNotNull ("groupClause", groupClause);
      _sb.Append ("\n");
      _sb.Append ("--- GroupClause ---");
      _sb.AppendFormat ("PreviousClause: {0}", groupClause.PreviousClause);
      _sb.Append ("\n");
      _sb.Append ("GroupClause");
      _sb.AppendFormat ("GroupExpression: {0}", groupClause.GroupExpression);
      _sb.Append ("\n");
      _sb.AppendFormat ("ByExpression: {0}", groupClause.ByExpression);
      _sb.Append ("\n");
      //_sb.AppendFormat ("group {0} by {1}", groupClause.GroupExpression, groupClause.ByExpression);
    }

    #endregion

    public override string ToString ()
    {
      return _sb.ToString();
    }
  }
}
/* Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh
 *
 * This program is free software: you can redistribute it and/or modify it under 
 * the terms of the re:motion license agreement in license.txt. If you did not 
 * receive it, please visit http://www.re-motion.org/licensing.
 * 
 * Unless otherwise provided, this software is distributed on an "AS IS" basis, 
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 */

using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using Remotion.Data.Linq.Clauses;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Visitor
{
  public class StringVisitor : IQueryVisitor
  {
    private readonly StringBuilder _sb = null;

    public StringVisitor()
    {
      _sb = new StringBuilder();
    }

    #region IQueryVisitor Members

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
      _sb.AppendFormat ("from {0} {1} in {2} ", fromClause.Identifier.Type.Name, fromClause.Identifier.Name, fromClause.QuerySource);

      foreach (JoinClause jc in fromClause.JoinClauses)
        jc.Accept (this);
    }

    public void VisitAdditionalFromClause (AdditionalFromClause fromClause)
    {
      ArgumentUtility.CheckNotNull ("fromClause", fromClause);

      string fromString;
      MemberExpression memberExpression = fromClause.FromExpression.Body as MemberExpression;
      if (memberExpression != null && IsCompilerGeneratedFromExpression (memberExpression))
        fromString = memberExpression.Member.Name;
      else
        fromString = fromClause.FromExpression.Body.ToString();

      _sb.AppendFormat ("from {0} {1} in {2} ", fromClause.Identifier.Type.Name, fromClause.Identifier.Name, fromString);

      foreach (JoinClause jc in fromClause.JoinClauses)
        jc.Accept (this);
    }

    private bool IsCompilerGeneratedFromExpression (MemberExpression memberExpression)
    {
      return memberExpression.Expression.NodeType == ExpressionType.Constant
          && memberExpression.Expression.Type.IsDefined (typeof (CompilerGeneratedAttribute), false);
    }

    public void VisitSubQueryFromClause (SubQueryFromClause fromClause)
    {
      ArgumentUtility.CheckNotNull ("fromClause", fromClause);
      _sb.AppendFormat ("from {0} {1} in ({2}) ", fromClause.Identifier.Type.Name, fromClause.Identifier.Name, fromClause.SubQueryModel);
    }

    public void VisitJoinClause (JoinClause joinClause)
    {
      ArgumentUtility.CheckNotNull ("joinClause", joinClause);
      _sb.AppendFormat ("join {0} {1} in {2} on {3} equals {4} into {5} ",
          joinClause.Identifier.Type, joinClause.Identifier, joinClause.InExpression,
          joinClause.OnExpression, joinClause.EqualityExpression, joinClause.IntoIdentifier);
    }

    public void VisitLetClause (LetClause letClause)
    {
      ArgumentUtility.CheckNotNull ("letClause", letClause);

      _sb.AppendFormat ("let {0} = {1} ", letClause.Identifier, letClause.Expression);
    }

    public void VisitWhereClause (WhereClause whereClause)
    {
      ArgumentUtility.CheckNotNull ("whereClause", whereClause);

      _sb.AppendFormat ("where {0} ", whereClause.BoolExpression.Body);
    }

    public void VisitOrderByClause (OrderByClause orderByClause)
    {
      ArgumentUtility.CheckNotNull ("orderByClause", orderByClause);
      _sb.Append ("orderby ");
      foreach (OrderingClause oC in orderByClause.OrderingList)
      {
        oC.Accept (this);
      }
    }

    public void VisitOrderingClause (OrderingClause orderingClause)
    {
      ArgumentUtility.CheckNotNull ("orderingClause", orderingClause);

      switch (orderingClause.OrderDirection)
      {
        case OrderDirection.Asc:
          _sb.AppendFormat ("{0} ascending ", orderingClause.Expression);
          break;
        case OrderDirection.Desc:
          _sb.AppendFormat ("{0} descending ", orderingClause.Expression);
          break;
      }
    }

    public void VisitSelectClause (SelectClause selectClause)
    {
      ArgumentUtility.CheckNotNull ("selectClause", selectClause);
      _sb.AppendFormat ("select {0}", selectClause.ProjectionExpression == null ? "<value>" : selectClause.ProjectionExpression.Body.ToString());
    }

    public void VisitGroupClause (GroupClause groupClause)
    {
      ArgumentUtility.CheckNotNull ("groupClause", groupClause);
      _sb.AppendFormat ("group {0} by {1}", groupClause.GroupExpression, groupClause.ByExpression);
    }

    #endregion

    public override string ToString ()
    {
      return _sb.ToString();
    }
  }
}

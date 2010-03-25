// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// as published by the Free Software Foundation; either version 2.1 of the 
// License, or (at your option) any later version.
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
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.ResultOperators;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.SqlPreparation
{
  /// <summary>
  /// <see cref="SqlPreparationQueryModelVisitor"/> generates a <see cref="SqlStatement"/> from a query model.
  /// </summary>
  public class SqlPreparationQueryModelVisitor : QueryModelVisitorBase
  {
    public static SqlStatement TransformQueryModel (
        QueryModel queryModel,
        SqlPreparationContext preparationContext,
        ISqlPreparationStage stage)
    {
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);
      ArgumentUtility.CheckNotNull ("preparationContext", preparationContext);
      ArgumentUtility.CheckNotNull ("stage", stage);

      var visitor = new SqlPreparationQueryModelVisitor (preparationContext, stage);
      queryModel.Accept (visitor);
      return visitor.GetSqlStatement();
    }

    private readonly SqlPreparationContext _context;
    private readonly ISqlPreparationStage _stage;
    private readonly List<SqlTableBase> _sqlTables;
    private readonly List<Ordering> _orderings;

    protected SqlPreparationQueryModelVisitor (SqlPreparationContext context, ISqlPreparationStage stage)
    {
      ArgumentUtility.CheckNotNull ("context", context);
      ArgumentUtility.CheckNotNull ("stage", stage);

      _context = context;
      _stage = stage;

      _sqlTables = new List<SqlTableBase>();
      _orderings = new List<Ordering>();
    }

    public SqlPreparationContext Context
    {
      get { return _context; }
    }

    protected ISqlPreparationStage Stage
    {
      get { return _stage; }
    }

    protected Expression ProjectionExpression { get; set; }
    protected Expression WhereCondition { get; set; }
    protected bool IsCountQuery { get; set; }
    protected bool IsDistinctQuery { get; set; }
    protected Expression TopExpression { get; set; }

    protected List<SqlTableBase> SqlTables
    {
      get { return _sqlTables; }
    }

    protected List<Ordering> Orderings
    {
      get { return _orderings; }
    }

    public virtual SqlStatement GetSqlStatement ()
    {
      var sqlStatement = new SqlStatement (ProjectionExpression, _sqlTables, _orderings);

      sqlStatement.IsCountQuery = IsCountQuery;
      sqlStatement.IsDistinctQuery = IsDistinctQuery;
      sqlStatement.WhereCondition = WhereCondition;
      sqlStatement.TopExpression = TopExpression;

      return sqlStatement;
    }

    public override void VisitMainFromClause (MainFromClause fromClause, QueryModel queryModel)
    {
      ArgumentUtility.CheckNotNull ("fromClause", fromClause);
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);

      Debug.Assert (_sqlTables.Count == 0);

      AddFromClause (fromClause);
    }

    public override void VisitAdditionalFromClause (AdditionalFromClause fromClause, QueryModel queryModel, int index)
    {
      ArgumentUtility.CheckNotNull ("fromClause", fromClause);
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);

      AddFromClause (fromClause);
    }

    public override void VisitWhereClause (WhereClause whereClause, QueryModel queryModel, int index)
    {
      var translatedExpression = _stage.PrepareWhereExpression (whereClause.Predicate);
      AddWhereCondition(translatedExpression);
    }

    public override void VisitSelectClause (SelectClause selectClause, QueryModel queryModel)
    {
      ArgumentUtility.CheckNotNull ("selectClause", selectClause);
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);

      ProjectionExpression = _stage.PrepareSelectExpression (selectClause.Selector);
    }

    public override void VisitOrderByClause (OrderByClause orderByClause, QueryModel queryModel, int index)
    {
      ArgumentUtility.CheckNotNull ("orderByClause", orderByClause);
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);

      var orderings = from ordering in orderByClause.Orderings
                      let orderByExpression = _stage.PrepareOrderByExpression (ordering.Expression)
                      select new Ordering (orderByExpression, ordering.OrderingDirection);
      _orderings.InsertRange (0, orderings);
    }

    public override void VisitResultOperator (ResultOperatorBase resultOperator, QueryModel queryModel, int index)
    {
      ArgumentUtility.CheckNotNull ("resultOperator", resultOperator);
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);

      if (resultOperator is CountResultOperator)
        IsCountQuery = true;
      else if (resultOperator is DistinctResultOperator)
      {
        if (TopExpression != null)
          throw new NotImplementedException ("Distinct after Take is not yet implemented. TODO 2370");
        IsDistinctQuery = true;
      }
      else if (resultOperator is FirstResultOperator)
        TopExpression = _stage.PrepareTopExpression (Expression.Constant (1));
      else if (resultOperator is SingleResultOperator)
        TopExpression = _stage.PrepareTopExpression (Expression.Constant (1));
      else if (resultOperator is TakeResultOperator)
      {
        var expression = ((TakeResultOperator) resultOperator).Count;
        TopExpression = _stage.PrepareTopExpression (expression);
      }
      else
        throw new NotSupportedException (string.Format ("{0} is not supported.", resultOperator));
    }

    protected void AddWhereCondition (Expression translatedExpression)
    {
      if (WhereCondition != null)
        WhereCondition = Expression.AndAlso (WhereCondition, translatedExpression);
      else
        WhereCondition = translatedExpression;
    }

    private void AddFromClause (FromClauseBase fromClause)
    {
      var preparedFromExpression = _stage.PrepareFromExpression (fromClause.FromExpression);
      var sqlTableOrJoin = _stage.PrepareSqlTable (preparedFromExpression, fromClause.ItemType);

      var sqlJoinedTable = sqlTableOrJoin as SqlJoinedTable;
      if (sqlJoinedTable != null)
      {
        _context.AddQuerySourceMapping (fromClause, sqlJoinedTable);
        AddWhereCondition (new JoinConditionExpression (sqlJoinedTable));
        _sqlTables.Add (sqlJoinedTable);
      }
      else
      {
        _context.AddQuerySourceMapping (fromClause, sqlTableOrJoin);
        var topLevelSqlTable = sqlTableOrJoin as SqlTable;
        if (topLevelSqlTable != null)
          _sqlTables.Add (topLevelSqlTable);
      }
    }
  }
}
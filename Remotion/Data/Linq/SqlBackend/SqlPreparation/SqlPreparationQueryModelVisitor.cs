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
using System.Linq.Expressions;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.ResultOperators;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
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
        DefaultSqlPreparationStage stage)
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

    private readonly List<SqlTable> _sqlTables;

    private Expression _projectionExpression;
    private Expression _whereCondition;
    
    private bool _isCountQuery;
    private bool _isDistinctQuery;
    private Expression _topExpression;

    protected SqlPreparationQueryModelVisitor (SqlPreparationContext context, ISqlPreparationStage stage)
    {
      ArgumentUtility.CheckNotNull ("context", context);
      ArgumentUtility.CheckNotNull ("stage", stage);

      _context = context;
      _stage = stage;

      _sqlTables = new List<SqlTable> ();
    }

    public SqlPreparationContext Context
    {
      get { return _context; }
    }

    public SqlStatement GetSqlStatement ()
    {
      var sqlStatement = new SqlStatement (_projectionExpression, _sqlTables.ToArray());

      sqlStatement.IsCountQuery = _isCountQuery;
      sqlStatement.IsDistinctQuery = _isDistinctQuery;
      sqlStatement.WhereCondition = _whereCondition;
      sqlStatement.TopExpression = _topExpression;

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
      if (_whereCondition != null)
        _whereCondition = Expression.AndAlso (_whereCondition, translatedExpression);
      else
        _whereCondition = translatedExpression;
    }

    public override void VisitSelectClause (SelectClause selectClause, QueryModel queryModel)
    {
      ArgumentUtility.CheckNotNull ("selectClause", selectClause);
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);

      _projectionExpression = _stage.PrepareSelectExpression (selectClause.Selector);
    }
    
    public override void VisitResultOperator (ResultOperatorBase resultOperator, QueryModel queryModel, int index)
    {
      ArgumentUtility.CheckNotNull ("resultOperator", resultOperator);
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);

      if (resultOperator is CountResultOperator)
        _isCountQuery = true;
      else if (resultOperator is DistinctResultOperator)
      {
        if (_topExpression != null)
          throw new NotImplementedException ("Distinct after Take is not yet implemented. TODO 2370");
        _isDistinctQuery = true;
      }
      else if (resultOperator is FirstResultOperator)
        _topExpression = Expression.Constant (1);
      else if (resultOperator is SingleResultOperator)
        _topExpression = Expression.Constant (1);
      else if (resultOperator is TakeResultOperator)
      {
        var expression = ((TakeResultOperator) resultOperator).Count;
        _topExpression = _stage.PrepareTopExpression (expression);
      }
      else
        throw new NotSupportedException (string.Format ("{0} is not supported.", resultOperator));
    }

    private void AddFromClause (FromClauseBase fromClause)
    {
      var sqlTable = _stage.GetTableForFromExpression (fromClause.FromExpression, fromClause.ItemType);
      _context.AddQuerySourceMapping (fromClause, sqlTable);
      _sqlTables.Add ((SqlTable) sqlTable);
    }
  }
}
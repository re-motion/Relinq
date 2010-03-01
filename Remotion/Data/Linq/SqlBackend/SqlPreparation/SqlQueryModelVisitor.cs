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
using System.Linq.Expressions;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.SqlPreparation
{
  /// <summary>
  /// <see cref="SqlQueryModelVisitor"/> generates a <see cref="SqlStatement"/> from a query model.
  /// </summary>
  public class SqlQueryModelVisitor : QueryModelVisitorBase
  {
    private readonly SqlPreparationContext _sqlPreparationContext;
    private readonly SqlTable _sqlTable;
    private Expression _projectionExpression;

    public SqlQueryModelVisitor ()
    {
      _sqlTable = new SqlTable ();
      _sqlPreparationContext = new SqlPreparationContext ();
    }

    public SqlPreparationContext SqlPreparationContext
    {
      get { return _sqlPreparationContext; }
    }

    public SqlStatement GetSqlStatement ()
    {
      var sqlStatement = new SqlStatement (_projectionExpression, _sqlTable);
      return sqlStatement;
    }

    public override void VisitSelectClause (SelectClause selectClause, QueryModel queryModel)
    {
      ArgumentUtility.CheckNotNull ("selectClause", selectClause);
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);

      _projectionExpression = SqlSelectExpressionVisitor.TranslateExpression (selectClause.Selector, _sqlPreparationContext);
    }

    public override void VisitMainFromClause (MainFromClause fromClause, QueryModel queryModel)
    {
      ArgumentUtility.CheckNotNull ("fromClause", fromClause);
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);

      // In the future, we'll probably need a visitor here as well when we support more complext FromExpressions.
      _sqlTable.TableSource = new ConstantTableSource ((ConstantExpression) fromClause.FromExpression);
      _sqlPreparationContext.AddQuerySourceMapping (fromClause, _sqlTable);
    }
    
  }
}
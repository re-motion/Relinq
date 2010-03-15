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
using Remotion.Data.Linq.Utilities;
using System.Collections.Generic;

namespace Remotion.Data.Linq.SqlBackend.SqlStatementModel
{
  /// <summary>
  /// <see cref="SqlStatementVisitorBase"/> provides methods to visit sql-statement classes.
  /// </summary>
  public abstract class SqlStatementVisitorBase
  {
    private readonly UniqueIdentifierGenerator _uniqueIdentifierGenerator;

    protected SqlStatementVisitorBase (UniqueIdentifierGenerator uniqueIdentifierGenerator)
    {
      ArgumentUtility.CheckNotNull ("uniqueIdentifierGenerator", uniqueIdentifierGenerator);
      _uniqueIdentifierGenerator = uniqueIdentifierGenerator;
    }

    public UniqueIdentifierGenerator UniqueIdentifierGenerator
    {
      get { return _uniqueIdentifierGenerator; }
    }

    public virtual void VisitSqlStatement (SqlStatement sqlStatement)
    {
      foreach (var sqlTable in sqlStatement.FromExpressions)
      {
        VisitSqlTable (sqlTable);        
      }
      
      sqlStatement.SelectProjection = VisitSelectProjection (sqlStatement.SelectProjection);

      if (sqlStatement.WhereCondition != null)
        sqlStatement.WhereCondition = VisitWhereCondition (sqlStatement.WhereCondition);

      if (sqlStatement.TopExpression != null)
        sqlStatement.TopExpression = VisitTopExpression (sqlStatement.TopExpression);

      if (sqlStatement.OrderByClauses.Count > 0)
      {
        foreach (var orderByClause in sqlStatement.OrderByClauses)
        {
          orderByClause.Expression = VisitOrderingExpression (orderByClause.Expression);
        }
      }
    }

    protected abstract Expression VisitSelectProjection (Expression selectProjection);
    protected abstract void VisitSqlTable (SqlTable sqlTable);
    protected abstract Expression VisitTopExpression (Expression topExpression);
    protected abstract Expression VisitWhereCondition (Expression whereCondition);
    protected abstract Expression VisitOrderingExpression (Expression orderByExpression);
  }
}
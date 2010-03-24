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
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved
{
  /// <summary>
  /// <see cref="SqlTableReferenceExpression"/> represents a data row in a <see cref="SqlTable"/>.
  /// </summary>
  public class SqlTableReferenceExpression : ExtensionExpression
  {
    private readonly SqlTableBase _sqlTable;

    public SqlTableReferenceExpression (SqlTableBase sqlTable)
        : base(sqlTable.ItemType)
    {
      ArgumentUtility.CheckNotNull ("sqlTable", sqlTable);

      _sqlTable = sqlTable;
    }

    public SqlTableBase SqlTable
    {
      get { return _sqlTable; }
    }

    protected override Expression VisitChildren (ExpressionTreeVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);
      return this;
    }

    public override Expression Accept (ExpressionTreeVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);

      var specificVisitor = visitor as IUnresolvedSqlExpressionVisitor;
      if (specificVisitor != null)
        return specificVisitor.VisitSqlTableReferenceExpression (this);
      else
        return base.Accept (visitor);
    }


  }
}
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
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.SqlStatementModel
{
  /// <summary>
  /// <see cref="SqlQueryModelVisitor"/> generates a <see cref="SqlStatement"/> from a query model.
  /// </summary>
  public class SqlQueryModelVisitor : QueryModelVisitorBase
  {
    private readonly SqlStatement _sqlStatement;
    private SqlGenerationContext _sqlGenerationContext; 

    public SqlQueryModelVisitor ()
    {
      _sqlStatement = new SqlStatement();
      _sqlGenerationContext = new SqlGenerationContext ();
    }

    public SqlStatement SqlStatement
    {
      get { return _sqlStatement; }
    }

    public SqlGenerationContext SqlGenerationContext
    {
      get { return _sqlGenerationContext; }
    }

    public override void VisitSelectClause (SelectClause selectClause, QueryModel queryModel)
    {
      _sqlStatement.SelectProjection = SqlSelectExpressionVisitor.TranslateSelectExpression (selectClause.Selector, _sqlGenerationContext);
    }

    public override void VisitMainFromClause (MainFromClause fromClause, QueryModel queryModel)
    {
      _sqlStatement.FromExpression = SqlFromExpressionVisitor.TranslateFromExpression (fromClause.FromExpression);
      _sqlGenerationContext.AddQuerySourceMapping (fromClause, _sqlStatement.FromExpression);
    }

  }
}
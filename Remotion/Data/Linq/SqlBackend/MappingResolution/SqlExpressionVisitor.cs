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
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;

namespace Remotion.Data.Linq.SqlBackend.MappingResolution
{
  /// <summary>
  /// <see cref="SqlExpressionVisitor"/> implements <see cref="ISqlExpressionVisitor"/> and <see cref="ThrowingExpressionTreeVisitor"/>.
  /// </summary>
  public class SqlExpressionVisitor : ThrowingExpressionTreeVisitor, ISqlExpressionVisitor
  {
    private readonly SqlStatementResolver _resolver;

    public static Expression TranslateSqlTableExpression (Expression expression, SqlStatementResolver resolver)
    {
      var visitor = new SqlExpressionVisitor (resolver);
      var result = visitor.VisitExpression (expression);
      return result;
    }

    protected SqlExpressionVisitor (SqlStatementResolver resolver)
    {
      _resolver = resolver;
    }
    
    public SqlTableExpression VisitSqlTableExpression (SqlTableExpression tableExpression)
    {
      var tableSource = _resolver.ResolveTableSource (tableExpression.TableSource);
      return new SqlTableExpression (tableExpression.Type, tableSource);
    }

    public Expression VisitSqlTableReferenceExpression (Expression expression)
    {
      return _resolver.ResolveSelectProjection (expression);
    }
    
    protected override Exception CreateUnhandledItemException<T> (T unhandledItem, string visitMethod)
    {
      var message = string.Format (
          "The given expression type '{0}' is not supported in from clauses. (Expression: '{1}')",
          unhandledItem.GetType ().Name,
          unhandledItem);
      throw new NotSupportedException (message);
    }
  }
}
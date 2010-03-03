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
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.MappingResolution
{
  /// <summary>
  /// <see cref="ResolvingExpressionVisitor"/> implements <see cref="ISqlExpressionVisitor"/> and <see cref="ThrowingExpressionTreeVisitor"/>.
  /// </summary>
  // TODO: ResolvingExpressionVisitor should simply ignore any expressions it cannot resolve; derive from ExpressionTreeVisitor rather than ThrowingExpressionTreeVisitor.
  public class ResolvingExpressionVisitor : ThrowingExpressionTreeVisitor, ISqlExpressionVisitor
  {
    private readonly ISqlStatementResolver _resolver;
    private readonly UniqueIdentifierGenerator _generator;

    public static Expression ResolveExpressions (Expression expression, ISqlStatementResolver resolver, UniqueIdentifierGenerator generator)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      ArgumentUtility.CheckNotNull ("resolver", resolver);
      ArgumentUtility.CheckNotNull ("generator", generator);

      var visitor = new ResolvingExpressionVisitor (resolver, generator);
      var result = visitor.VisitExpression (expression);
      return result;
    }

    protected ResolvingExpressionVisitor (ISqlStatementResolver resolver, UniqueIdentifierGenerator generator)
    {
      ArgumentUtility.CheckNotNull ("resolver", resolver);
      ArgumentUtility.CheckNotNull ("generator", generator);

      _resolver = resolver;
      _generator = generator;
    }
    
    public Expression VisitSqlTableReferenceExpression (SqlTableReferenceExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      return _resolver.ResolveTableReferenceExpression (expression);
    }

    public Expression VisitSqlMemberExpression(SqlMemberExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      
      return _resolver.ResolveMemberExpression (expression, _generator);
    }

    // TODO: Remove
    protected override Exception CreateUnhandledItemException<T> (T unhandledItem, string visitMethod)
    {
      ArgumentUtility.CheckNotNull ("unhandledItem", unhandledItem);
      ArgumentUtility.CheckNotNullOrEmpty ("visitMethod", visitMethod);

      var message = string.Format (
          "The given expression type '{0}' is not supported in from clauses. (Expression: '{1}')",
          unhandledItem.GetType ().Name,
          unhandledItem);
      throw new NotSupportedException (message);
    }
  }
}
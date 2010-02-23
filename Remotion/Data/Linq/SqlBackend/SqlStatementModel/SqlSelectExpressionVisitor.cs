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

namespace Remotion.Data.Linq.SqlBackend.SqlStatementModel
{

  /// <summary>
  /// <see cref="SqlSelectExpressionVisitor"/> transforms <see cref="SqlStatement.SelectProjection"/> 
  /// to a <see cref="SqlTableReferenceExpression"/>.
  /// </summary>
  public class SqlSelectExpressionVisitor : ThrowingExpressionTreeVisitor
  {
    private SqlGenerationContext _context;

    public static SqlTableReferenceExpression TranslateFromExpression (Expression projection, SqlGenerationContext context)
    {
      var visitor = new SqlSelectExpressionVisitor (context);
      var result = visitor.VisitExpression (projection);
      return (SqlTableReferenceExpression) result;
    }

    public SqlSelectExpressionVisitor (SqlGenerationContext context)
    {
      ArgumentUtility.CheckNotNull ("context", context);
      _context = context;
    }

    protected override Exception CreateUnhandledItemException<T> (T unhandledItem, string visitMethod)
    {
      throw new NotImplementedException();
    }

    protected override TResult VisitUnhandledItem<TItem, TResult> (TItem unhandledItem, string visitMethod, Func<TItem, TResult> baseBehavior)
    {
      throw new NotImplementedException();
    }

    protected internal override Expression VisitUnknownExpression (Expression expression)
    {
      throw new NotImplementedException();
    }

    protected override Expression VisitUnaryExpression (UnaryExpression expression)
    {
      throw new NotImplementedException();
    }

    protected override Expression VisitBinaryExpression (BinaryExpression expression)
    {
      throw new NotImplementedException();
    }

    protected override Expression VisitTypeBinaryExpression (TypeBinaryExpression expression)
    {
      throw new NotImplementedException();
    }

    protected override Expression VisitConstantExpression (ConstantExpression expression)
    {
      throw new NotImplementedException();
    }

    protected override Expression VisitConditionalExpression (ConditionalExpression expression)
    {
      throw new NotImplementedException();
    }

    protected override Expression VisitParameterExpression (ParameterExpression expression)
    {
      throw new NotImplementedException();
    }

    protected override Expression VisitLambdaExpression (LambdaExpression expression)
    {
      throw new NotImplementedException();
    }

    protected override Expression VisitMethodCallExpression (MethodCallExpression expression)
    {
      throw new NotImplementedException();
    }

    protected override Expression VisitInvocationExpression (InvocationExpression expression)
    {
      throw new NotImplementedException();
    }

    protected override Expression VisitMemberExpression (MemberExpression expression)
    {
      throw new NotImplementedException();
    }

    protected override Expression VisitNewExpression (NewExpression expression)
    {
      throw new NotImplementedException();
    }

    protected override Expression VisitNewArrayExpression (NewArrayExpression expression)
    {
      throw new NotImplementedException();
    }

    protected override Expression VisitMemberInitExpression (MemberInitExpression expression)
    {
      throw new NotImplementedException();
    }

    protected override Expression VisitListInitExpression (ListInitExpression expression)
    {
      throw new NotImplementedException();
    }

    protected override ElementInit VisitElementInit (ElementInit elementInit)
    {
      throw new NotImplementedException();
    }

    protected override MemberBinding VisitMemberAssignment (MemberAssignment memberAssigment)
    {
      throw new NotImplementedException();
    }

    protected override MemberBinding VisitMemberMemberBinding (MemberMemberBinding binding)
    {
      throw new NotImplementedException();
    }

    protected override MemberBinding VisitMemberListBinding (MemberListBinding listBinding)
    {
      throw new NotImplementedException();
    }

    protected override Expression VisitSubQueryExpression (SubQueryExpression expression)
    {
      throw new NotImplementedException();
    }

    protected override Expression VisitQuerySourceReferenceExpression (QuerySourceReferenceExpression expression)
    {
      return new SqlTableReferenceExpression (expression.Type) { SqlTableExpression = _context.Mapping[expression.ReferencedQuerySource] };
    }
  }
}
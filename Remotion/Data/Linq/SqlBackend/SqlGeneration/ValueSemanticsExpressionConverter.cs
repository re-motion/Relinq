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
using System.Diagnostics;
using System.Linq.Expressions;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.SqlGeneration
{
  /// <summary>
  /// Ensures that a given expression matches SQL server value semantics.
  /// </summary>
  public class ValueSemanticsExpressionConverter : ThrowingExpressionTreeVisitor, IResolvedSqlExpressionVisitor
  {
    public static Expression EnsureValueSemantics (Expression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var visitor = new ValueSemanticsExpressionConverter();
      return visitor.VisitExpression (expression);
    }

    public override Expression VisitExpression (Expression expression)
    {
      if (expression.Type != typeof (bool))
        return expression;

      return base.VisitExpression (expression);
    }

    protected override Expression VisitConstantExpression (ConstantExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      if (expression.Value.Equals (true))
        return Expression.Constant (1);
      else
      {
        Debug.Assert (expression.Value.Equals (false));
        return Expression.Constant (0);
      }
    }

    protected override Expression VisitBinaryExpression (BinaryExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      Debug.Assert (expression.Type == typeof (bool));

      var left = expression.Left;
      var right = expression.Right;

      switch (expression.NodeType)
      {
        case ExpressionType.NotEqual:
        case ExpressionType.Equal:
          left = EnsureValueSemantics (left);
          right = EnsureValueSemantics (right);
          break;
        //case ExpressionType.AndAlso:
        //case ExpressionType.And:
        //case ExpressionType.OrElse:
        //case ExpressionType.Or:
        //case ExpressionType.ExclusiveOr:
        //  TODO
      }

      if (left != expression.Left || right != expression.Right)
        expression = Expression.MakeBinary (expression.NodeType, left, right);

      if (expression.Type == typeof (bool)) // because of value conversion, type might now be int
        return new SqlCaseExpression (expression, Expression.Constant (1), Expression.Constant (0));
      else
        return expression;
    }

    public Expression VisitSqlColumListExpression (SqlColumnListExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      return base.VisitUnknownExpression (expression);
    }

    public Expression VisitSqlColumnExpression (SqlColumnExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      Debug.Assert (expression.Type == typeof (bool));
      return new SqlColumnExpression (typeof (int), expression.OwningTableAlias, expression.ColumnName);
    }

    protected override Exception CreateUnhandledItemException<T> (T unhandledItem, string visitMethod)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("visitMethod", visitMethod);

      string message = string.Format ("Expression type '{0}' was not expected to have boolean type.", typeof (T));
      throw new NotSupportedException (message);
    }
  }
}
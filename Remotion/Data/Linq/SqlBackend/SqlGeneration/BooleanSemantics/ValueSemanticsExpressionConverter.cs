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

namespace Remotion.Data.Linq.SqlBackend.SqlGeneration.BooleanSemantics
{
  /// <summary>
  /// Ensures that a given expression matches SQL server value semantics.
  /// </summary>
  public class ValueSemanticsExpressionConverter : ThrowingExpressionTreeVisitor, IResolvedSqlExpressionVisitor
  {
    public static Expression EnsureValueSemantics (Expression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var visitor = new ValueSemanticsExpressionConverter ();
      return visitor.VisitExpression (expression);
    }

    // private bool _needsPredicateSemantics;

    protected ValueSemanticsExpressionConverter ()
    {
      // _needsPredicateSemantics = needsPredicateSemantics;
    }

    public override Expression VisitExpression (Expression expression)
    {
      // TODO: Visit all
      if (expression.Type != typeof (bool))
        return expression;

      return base.VisitExpression (expression);
    }

    protected override Expression VisitConstantExpression (ConstantExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      //if (expression.Type == typeof (bool) && !_needsPredicateSemantics) TODO
      //{
      if (expression.Value.Equals (true))
        return Expression.Constant (1);
      else
      {
        Debug.Assert (expression.Value.Equals (false));
        return Expression.Constant (0);
      }
      //}
      //else if (expression.Type != typeof (bool) && _needsPredicateSemantics)
      //{
      //  // return expression == 1
      //  throw new NotImplementedException(); // TODO
      //}
      //else
      //{
      //  return expression;
      //}
    }

    protected override Expression VisitBinaryExpression (BinaryExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var left = expression.Left;
      var right = expression.Right;

      switch (expression.NodeType)
      {
        case ExpressionType.NotEqual:
        case ExpressionType.Equal:
          //var oldSemantics = _needsPredicateSemantics;
          //_needsPredicateSemantics = false;
          left = VisitExpression (left);
          right = VisitExpression (right);
          //_needsPredicateSemantics = oldSemantics;
          break;
          //case ExpressionType.AndAlso:
          //case ExpressionType.OrElse:
          //var oldSemantics = _needsPredicateSemantics;
          //_needsPredicateSemantics = true;
          //left = VisitExpression (left);
          //right = VisitExpression (right);
          //_needsPredicateSemantics = true;
          //break;
          //case ExpressionType.And:
          //case ExpressionType.Or:
          //case ExpressionType.ExclusiveOr:
          // if (expression.Type == typeof (bool))
          // {
          //   var oldSemantics = _needsPredicateSemantics;
          //   _needsPredicateSemantics = true;
          //   // etc
          // }
          // else: _needsPredicateSemantics = false; etc.
      }

      if (left != expression.Left || right != expression.Right)
        expression = Expression.MakeBinary (expression.NodeType, left, right);

      if (expression.Type == typeof (bool)) // && !_needsPredicateSemantics) // because of value conversion, type might now be int
        return new SqlCaseExpression (expression, Expression.Constant (1), Expression.Constant (0));
          // else if (expression.Type == typeof (int) && !_needsValueSemantics)
          //   return expression == 1
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

      // if (expression.Type == typeof (bool) && !_needsPredicateSemantics) TODO
      return new SqlColumnExpression (typeof (int), expression.OwningTableAlias, expression.ColumnName);
      // else if (expression.Type == typeof (int) && _needsPredicateSemantics) TODO
      // return expression == 1
      // else
      //  return expression;
    }

    protected override Exception CreateUnhandledItemException<T> (T unhandledItem, string visitMethod)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("visitMethod", visitMethod);

      string message = string.Format ("Expression type '{0}' was not expected to have boolean type.", typeof (T));
      throw new NotSupportedException (message);
    }
  }
}
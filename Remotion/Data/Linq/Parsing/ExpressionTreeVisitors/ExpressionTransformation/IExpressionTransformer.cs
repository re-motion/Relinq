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
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Remotion.Data.Linq.Parsing.ExpressionTreeVisitors
{
  ///// <summary>
  ///// The <see cref="IExpressionTransformer{T}"/> defines the API for classes that transform <see cref="Expression"/>s./>
  ///// </summary>
  ///// <typeparam name="T">The expression type.</typeparam>
  //public interface IExpressionTransformer<T> where T : Expression
  //{
  //  Expression Transform (T expression, Stack<Expression> parentExpressions); // Encapsulate this in a read-only wrapper
  //}

  //public class BinaryExpressionTransformer : IExpressionTransformer<BinaryExpression>
  //{
  //  public Expression Transform (BinaryExpression expression, Stack<Expression> parentExpressions)
  //  {
  //    if (expression.Method != null)
  //    {
  //      var newExpression = Expression.Call (expression.Method, expression.Left, expression.Right);
  //      return newExpression;
  //    }
  //    else if (expression.NodeType == ExpressionType.Subtract)
  //    {
  //      if (expression.Right.NodeType == ExpressionType.Constant && ((ConstantExpression) expression.Right).Value.Equals (0))
  //      {
  //        return expression.Left;
  //      }
  //      else
  //      {
  //        var newExpression = Expression.Add (expression.Left, Expression.Negate (expression.Right));
  //        return newExpression;
  //      }
  //    }
  //    else
  //    {
  //      return expression;
  //    }
  //  }
  //}
}
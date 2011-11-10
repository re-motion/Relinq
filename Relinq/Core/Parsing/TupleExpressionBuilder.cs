// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (c) rubicon IT GmbH, www.rubicon.eu
// 
// re-linq is free software; you can redistribute it and/or modify it under 
// the terms of the GNU Lesser General Public License as published by the 
// Free Software Foundation; either version 2.1 of the License, 
// or (at your option) any later version.
// 
// re-linq is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-linq; if not, see http://www.gnu.org/licenses.
// 
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Remotion.Linq.Utilities;

namespace Remotion.Linq.Parsing
{
  /// <summary>
  /// <see cref="TupleExpressionBuilder"/> can be used to build tuples incorporating a sequence of <see cref="Expression"/>s. 
  /// For example, given three expressions, exp1, exp2, and exp3, it will build nested <see cref="NewExpression"/>s that are equivalent to the 
  /// following: new KeyValuePair(exp1, new KeyValuePair(exp2, exp3)).
  /// Given an <see cref="Expression"/> whose type matches that of a tuple built by <see cref="TupleExpressionBuilder"/>, the builder can also return 
  /// an enumeration of accessor expressions that can be used to access the tuple elements in the same order as they were put into the nested tuple 
  /// expression. In above example, this would yield tupleExpression.Key, tupleExpression.Value.Key, and tupleExpression.Value.Value.
  /// This class can be handy whenever a set of <see cref="Expression"/> needs to be put into a single <see cref="Expression"/> 
  /// (eg., a select projection), especially if each sub-expression needs to be explicitly accessed at a later point of time (eg., to retrieve the 
  /// items from a statement surrounding a sub-statement yielding the tuple in its select projection).
  /// </summary>
  public class TupleExpressionBuilder
  {
    public static Expression AggregateExpressionsIntoTuple (IEnumerable<Expression> expressions)
    {
      ArgumentUtility.CheckNotNull ("expressions", expressions);

      return expressions
          .Reverse ()
          .Aggregate ((current, expression) => CreateTupleExpression (expression, current));
    }

    public static IEnumerable<Expression> GetExpressionsFromTuple (Expression tupleExpression)
    {
      ArgumentUtility.CheckNotNull ("tupleExpression", tupleExpression);

      while (tupleExpression.Type.IsGenericType && tupleExpression.Type.GetGenericTypeDefinition() == typeof (KeyValuePair<,>))
      {
        yield return Expression.MakeMemberAccess (tupleExpression, tupleExpression.Type.GetProperty ("Key"));
        tupleExpression = Expression.MakeMemberAccess (tupleExpression, tupleExpression.Type.GetProperty ("Value"));
      }

      yield return tupleExpression;
    }

    private static Expression CreateTupleExpression (Expression left, Expression right)
    {
      var tupleType = typeof (KeyValuePair<,>).MakeGenericType (left.Type, right.Type);
      var newTupleExpression =
          Expression.New (
              tupleType.GetConstructor (new[] { left.Type, right.Type }),
              new[] { left, right },
              new[] { tupleType.GetMethod ("get_Key"), tupleType.GetMethod ("get_Value") });
      return newTupleExpression;
    }
  }
}
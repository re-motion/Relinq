// Copyright (c) rubicon IT GmbH, www.rubicon.eu
//
// See the NOTICE file distributed with this work for additional information
// regarding copyright ownership.  rubicon licenses this file to you under 
// the Apache License, Version 2.0 (the "License"); you may not use this 
// file except in compliance with the License.  You may obtain a copy of the 
// License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, WITHOUT 
// WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.  See the 
// License for the specific language governing permissions and limitations
// under the License.
// 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Utilities;

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
  public static class TupleExpressionBuilder
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

      while (tupleExpression.Type.GetTypeInfo().IsGenericType && tupleExpression.Type.GetGenericTypeDefinition() == typeof (KeyValuePair<,>))
      {
        yield return Expression.MakeMemberAccess (tupleExpression, tupleExpression.Type.GetRuntimeProperty ("Key"));
        tupleExpression = Expression.MakeMemberAccess (tupleExpression, tupleExpression.Type.GetRuntimeProperty ("Value"));
      }

      yield return tupleExpression;
    }

    private static Expression CreateTupleExpression (Expression left, Expression right)
    {
      var tupleType = typeof (KeyValuePair<,>).MakeGenericType (left.Type, right.Type);
      var newTupleExpression =
          Expression.New (
              tupleType.GetTypeInfo().DeclaredConstructors.Single(),
              new[] { left, right },
              new MemberInfo[] { tupleType.GetRuntimeProperty ("Key").GetGetMethod (true), tupleType.GetRuntimeProperty ("Value").GetGetMethod (true) });
      return newTupleExpression;
    }
  }
}
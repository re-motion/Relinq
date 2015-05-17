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
using System.Linq.Expressions;

namespace Remotion.Linq.UnitTests.Parsing.ExpressionVisitorTests
{
  public static class ExpressionInstanceCreator
  {
    private static readonly Dictionary<ExpressionType, Expression> s_expressionTypeMap = InitializeExpressionTypeMap ();

    private static Dictionary<ExpressionType, Expression> InitializeExpressionTypeMap ()
    {
      var map = new Dictionary<ExpressionType, Expression> ();

      Expression zero = Expression.Constant (0);
      Expression dateTimeValue = Expression.Constant (DateTime.MinValue);
      Expression zeroDouble = Expression.Constant (0.0);
      NewArrayExpression arrayExpression = Expression.NewArrayInit (typeof (int), zero);
      Expression trueExpression = Expression.Constant (true);
      LambdaExpression lambdaExpression = Expression.Lambda<Func<int>> (zero);
      LambdaExpression lambdaExpressionWithArguments = Expression.Lambda<Func<int, int>> (zero, Expression.Parameter (typeof (int), "i"));
      NewExpression newExpression = Expression.New (typeof (List<int>).GetConstructor (new[] { typeof (int) }), zero);

      map[ExpressionType.Add] = Expression.Add (zero, zero);
      map[ExpressionType.AddChecked] = Expression.AddChecked (zero, zero);
      map[ExpressionType.And] = Expression.And (zero, zero);
      map[ExpressionType.AndAlso] = Expression.AndAlso (trueExpression, trueExpression);
      map[ExpressionType.ArrayLength] = Expression.ArrayLength (arrayExpression);
      map[ExpressionType.ArrayIndex] = Expression.ArrayIndex (arrayExpression, zero);
      map[ExpressionType.Call] = Expression.Call (zero, typeof (int).GetMethod ("Equals", new[] { typeof (int) }), zero);
      map[ExpressionType.Coalesce] = Expression.Coalesce (arrayExpression, arrayExpression);
      map[ExpressionType.Conditional] = Expression.Condition (trueExpression, zero, zero);
      map[ExpressionType.Constant] = Expression.Constant (zero);
      map[ExpressionType.Convert] = Expression.Convert (zero, typeof (object));
      map[ExpressionType.ConvertChecked] = Expression.ConvertChecked (zero, typeof (object));
      map[ExpressionType.Divide] = Expression.Divide (zero, zero);
      map[ExpressionType.Equal] = Expression.Equal (zero, zero);
      map[ExpressionType.ExclusiveOr] = Expression.ExclusiveOr (trueExpression, trueExpression);
      map[ExpressionType.GreaterThan] = Expression.GreaterThan (zero, zero);
      map[ExpressionType.GreaterThanOrEqual] = Expression.GreaterThanOrEqual (zero, zero);
      map[ExpressionType.Invoke] = Expression.Invoke (lambdaExpressionWithArguments, zero);
      map[ExpressionType.Lambda] = lambdaExpressionWithArguments;
      map[ExpressionType.LeftShift] = Expression.LeftShift (zero, zero);
      map[ExpressionType.LessThan] = Expression.LessThan (zero, zero);
      map[ExpressionType.LessThanOrEqual] = Expression.LessThanOrEqual (zero, zero);
      map[ExpressionType.ListInit] = Expression.ListInit (newExpression, zero);
      map[ExpressionType.MemberAccess] = Expression.MakeMemberAccess (dateTimeValue, typeof (DateTime).GetProperty ("Date"));
      map[ExpressionType.MemberInit] = Expression.MemberInit (newExpression, Expression.Bind (typeof (List<int>).GetProperty ("Capacity"), zero));
      map[ExpressionType.Modulo] = Expression.Modulo (zero, zero);
      map[ExpressionType.Multiply] = Expression.Multiply (zero, zero);
      map[ExpressionType.MultiplyChecked] = Expression.MultiplyChecked (zero, zero);
      map[ExpressionType.Negate] = Expression.Negate (zero);
      map[ExpressionType.UnaryPlus] = Expression.UnaryPlus (zero);
      map[ExpressionType.NegateChecked] = Expression.NegateChecked (zero);
      map[ExpressionType.New] = newExpression;
      map[ExpressionType.NewArrayInit] = arrayExpression;
      map[ExpressionType.NewArrayBounds] = Expression.NewArrayBounds (typeof (int), zero);
      map[ExpressionType.Not] = Expression.Not (trueExpression);
      map[ExpressionType.NotEqual] = Expression.NotEqual (zero, zero);
      map[ExpressionType.Or] = Expression.Or (trueExpression, trueExpression);
      map[ExpressionType.OrElse] = Expression.OrElse (trueExpression, trueExpression);
      map[ExpressionType.Parameter] = Expression.Parameter (typeof (object), "bla");
      map[ExpressionType.Power] = Expression.Power (zeroDouble, zeroDouble);
      map[ExpressionType.Quote] = Expression.Quote (lambdaExpression);
      map[ExpressionType.RightShift] = Expression.RightShift (zero, zero);
      map[ExpressionType.Subtract] = Expression.Subtract (zero, zero);
      map[ExpressionType.SubtractChecked] = Expression.SubtractChecked (zero, zero);
      map[ExpressionType.TypeAs] = Expression.TypeAs (zero, typeof (object));
      map[ExpressionType.TypeIs] = Expression.TypeIs (zero, typeof (object));
      map[(ExpressionType)(-1)] = new SpecialExpressionNode ((ExpressionType)(-1), typeof (int));
#if !NET_3_5
      map[ExpressionType.Block] = Expression.Block (zero);
      map[ExpressionType.DebugInfo] = Expression.DebugInfo (Expression.SymbolDocument ("test.cs"), 1, 1, 1, 1);
      map[ExpressionType.Goto] = Expression.Goto (Expression.Label());
      map[ExpressionType.Index] = Expression.MakeIndex (arrayExpression, typeof (int[]).GetProperty ("Item"), new[] { zero });
      map[ExpressionType.Label] = Expression.Label (Expression.Label());
      map[ExpressionType.Loop] = Expression.Loop (zero);
      map[ExpressionType.RuntimeVariables] = Expression.RuntimeVariables (Expression.Parameter (typeof (string)));
      map[ExpressionType.Switch] = Expression.Switch (zero, Expression.SwitchCase (Expression.Default (typeof (void)), zero));
      map[ExpressionType.Try] = Expression.TryFinally (zero, zero);
#endif
      return map;
    }

    public static Expression GetExpressionInstance (ExpressionType type)
    {
      return s_expressionTypeMap[type];
    }

    public static ElementInit CreateElementInit ()
    {
      return Expression.ElementInit (typeof (List<int>).GetMethod ("Add"), Expression.Constant (1));
    }

    public static MemberAssignment CreateMemberAssignment ()
    {
      return Expression.Bind (typeof (SimpleClass).GetField ("Value"), Expression.Constant ("test"));
    }

    public static MemberMemberBinding CreateMemberMemberBinding (IEnumerable<MemberBinding> memberBindings)
    {
      return Expression.MemberBind (typeof (SimpleClass).GetField ("ListValue"), memberBindings);
    }

    public static MemberListBinding CreateMemberListBinding (IEnumerable<ElementInit> initializers)
    {
      return Expression.ListBind (typeof (SimpleClass).GetField ("ListValue"), initializers);
    }

#if !NET_3_5
    public static CatchBlock CreateCatchBlock ()
    {
      return Expression.Catch (typeof (Exception), Expression.Constant ("test"));
    }

    public static LabelTarget CreateLabelTarget ()
    {
      return Expression.Label();
    }

    public static SwitchCase CreateSwitchCase ()
    {
      return Expression.SwitchCase (Expression.Constant ("test"), Expression.Constant ("test"));
    }
#endif
  }
}

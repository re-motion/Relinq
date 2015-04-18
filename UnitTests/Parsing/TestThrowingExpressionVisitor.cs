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
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using NUnit.Framework;
using Remotion.Linq.Parsing;

namespace Remotion.Linq.UnitTests.Parsing
{
  public class TestThrowingExpressionVisitor : ThrowingExpressionVisitor
  {
    public new Expression Visit (Expression expression)
    {
      return base.Visit (expression);
    }

    public new MemberBinding VisitMemberBinding (MemberBinding memberBinding)
    {
      return base.VisitMemberBinding (memberBinding);
    }

    public new ElementInit VisitElementInit (ElementInit elementInit)
    {
      return base.VisitElementInit (elementInit);
    }

    protected override Exception CreateUnhandledItemException<T> (T unhandledItem, string visitMethod)
    {
      throw new NotSupportedException("Test of " + visitMethod + ": " + unhandledItem);
    }

    protected override TResult VisitUnhandledItem<TItem, TResult> (TItem unhandledItem, string visitMethod, Func<TItem, TResult> baseBehavior)
    {
      var baseBehaviorCalledMethod = GetCalledMethod (baseBehavior.Method.GetMethodBody ());

      Type declaringType;
      if (baseBehaviorCalledMethod.DeclaringType == typeof (RelinqExpressionVisitor))
        declaringType = typeof (RelinqExpressionVisitor);
      else
        declaringType = typeof (ExpressionVisitor2);

      Assert.That (baseBehaviorCalledMethod, Is.EqualTo (declaringType.GetMethod (visitMethod, BindingFlags.NonPublic | BindingFlags.Instance)));

      return base.VisitUnhandledItem (unhandledItem, visitMethod, baseBehavior);
    }

    // Note: This method scans the given method body for a Call opcode and returns the called method. This only works in very limited scenarios where 
    // the code for the call opcode (0x28) occurs nowhere before the call opcode.
    private MethodInfo GetCalledMethod (MethodBody body)
    {
      var il = body.GetILAsByteArray ();
      int offset = 0;
      while (offset < il.Length && il[offset] != (byte) OpCodes.Call.Value)
        ++offset;

      Assert.That (offset < il.Length, "Found no Method call.");

      ++offset;
      Assert.That (offset < il.Length - 4, "Assertion failed.");
      var methodToken = il[offset] | (il[offset + 1] << 8) | (il[offset + 2] << 16) | (il[offset + 3] << 24);
      return (MethodInfo) typeof (ThrowingExpressionVisitor).Module.ResolveMethod (methodToken);
    }
  }
}

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
using System.Reflection;
using System.Reflection.Emit;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Parsing;

namespace Remotion.Data.Linq.UnitTests.Linq.Core.Parsing
{
  public class TestThrowingExpressionTreeVisitor : ThrowingExpressionTreeVisitor
  {
    public new Expression VisitExpression (Expression expression)
    {
      return base.VisitExpression (expression);
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
      Assert.That (baseBehaviorCalledMethod, Is.EqualTo (typeof (ExpressionTreeVisitor).GetMethod (visitMethod, BindingFlags.NonPublic | BindingFlags.Instance)));

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
      Debug.Assert (offset < il.Length - 4, "Assertion failed.");
      var methodToken = il[offset] | (il[offset + 1] << 8) | (il[offset + 2] << 16) | (il[offset + 3] << 24);
      return (MethodInfo) typeof (ThrowingExpressionTreeVisitor).Module.ResolveMethod (methodToken);
    }
  }
}

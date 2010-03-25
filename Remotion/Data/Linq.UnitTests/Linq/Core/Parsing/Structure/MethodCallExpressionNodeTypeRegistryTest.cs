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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq;
using Remotion.Data.Linq.Parsing.Structure;
using Remotion.Data.Linq.Parsing.Structure.IntermediateModel;
using Remotion.Data.Linq.UnitTests.Linq.Core.Parsing.Structure.TestDomain;

namespace Remotion.Data.Linq.UnitTests.Linq.Core.Parsing.Structure
{
  [TestFixture]
  public class MethodCallExpressionNodeTypeRegistryTest
  {
    private MethodCallExpressionNodeTypeRegistry _registry;

    [SetUp]
    public void SetUp ()
    {
      _registry = new MethodCallExpressionNodeTypeRegistry ();
    }

    [Test]
    public void GetRegisterableMethodDefinition_OrdinaryMethod ()
    {
      var method = typeof (object).GetMethod ("Equals", BindingFlags.Public | BindingFlags.Instance);
      var registerableMethod = MethodCallExpressionNodeTypeRegistry.GetRegisterableMethodDefinition (method);

      Assert.That (registerableMethod, Is.SameAs (method));
    }

    [Test]
    public void GetRegisterableMethodDefinition_GenericMethodDefinition ()
    {
      var method = ReflectionUtility.GetMethod (() => Queryable.Count<object> (null)).GetGenericMethodDefinition();
      var registerableMethod = MethodCallExpressionNodeTypeRegistry.GetRegisterableMethodDefinition (method);

      Assert.That (registerableMethod, Is.SameAs (method));
    }

    [Test]
    public void GetRegisterableMethodDefinition_ClosedGenericMethod ()
    {
      var method = ReflectionUtility.GetMethod (() => Queryable.Count<object> (null));
      var registerableMethod = MethodCallExpressionNodeTypeRegistry.GetRegisterableMethodDefinition (method);

      Assert.That (registerableMethod, Is.SameAs (method.GetGenericMethodDefinition()));
    }

    [Test]
    public void GetRegisterableMethodDefinition_NonGenericMethod_InGenericTypeDefinition ()
    {
      var method = typeof (GenericClass<>).GetMethod ("NonGenericMethod");
      var registerableMethod = MethodCallExpressionNodeTypeRegistry.GetRegisterableMethodDefinition (method);

      Assert.That (registerableMethod, Is.SameAs (method));
    }

    [Test]
    public void GetRegisterableMethodDefinition_NonGenericMethod_InClosedGenericType ()
    {
      var method = typeof (GenericClass<int>).GetMethod ("NonGenericMethod");
      var registerableMethod = MethodCallExpressionNodeTypeRegistry.GetRegisterableMethodDefinition (method);

      Assert.That (registerableMethod, Is.SameAs (typeof (GenericClass<>).GetMethod ("NonGenericMethod")));
    }

    [Test]
    public void GetRegisterableMethodDefinition_ClosedGenericMethod_InClosedGenericType ()
    {
      var method = typeof (GenericClass<int>).GetMethod ("GenericMethod").MakeGenericMethod (typeof (string));
      var registerableMethod = MethodCallExpressionNodeTypeRegistry.GetRegisterableMethodDefinition (method);

      Assert.That (registerableMethod, Is.SameAs (typeof (GenericClass<>).GetMethod ("GenericMethod")));
    }

    [Test]
    public void Register ()
    {
      Assert.That (_registry.Count, Is.EqualTo (0));

      _registry.Register (SelectExpressionNode.SupportedMethods, typeof (SelectExpressionNode));

      Assert.That(_registry.Count, Is.EqualTo (2));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException))]
    public void Register_ClosedGenericMethod_NotAllowed ()
    {
      var closedGenericMethod = SelectExpressionNode.SupportedMethods[0].MakeGenericMethod (typeof (int), typeof (int));
      _registry.Register (new[] { closedGenericMethod }, typeof (SelectExpressionNode));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException))]
    public void Register_MethodInClosedGenericType_NotAllowed ()
    {
      var methodInClosedGenericType = typeof (GenericClass<int>).GetMethod ("NonGenericMethod");
      _registry.Register (new[] { methodInClosedGenericType }, typeof (SelectExpressionNode));
    }

    [Test]
    public void GetNodeType ()
    {
      _registry.Register (SelectExpressionNode.SupportedMethods, typeof (SelectExpressionNode));

      var type = _registry.GetNodeType (SelectExpressionNode.SupportedMethods[0]);

      Assert.That (type, Is.SameAs (typeof (SelectExpressionNode)));
    }

    [Test]
    public void GetNodeType_WithMultipleNodes ()
    {
      _registry.Register (SelectExpressionNode.SupportedMethods, typeof (SelectExpressionNode));
      _registry.Register (SumExpressionNode.SupportedMethods, typeof (SumExpressionNode));

      var type1 = _registry.GetNodeType (SelectExpressionNode.SupportedMethods[0]);
      var type2 = _registry.GetNodeType (SumExpressionNode.SupportedMethods[0]);
      var type3 = _registry.GetNodeType (SumExpressionNode.SupportedMethods[1]);

      Assert.That (type1, Is.SameAs (typeof (SelectExpressionNode)));
      Assert.That (type2, Is.SameAs (typeof (SumExpressionNode)));
      Assert.That (type3, Is.SameAs (typeof (SumExpressionNode)));
    }

    [Test]
    public void GetNodeType_ClosedGenericMethod ()
    {
      _registry.Register (SelectExpressionNode.SupportedMethods, typeof (SelectExpressionNode));

      var closedGenericMethodCallExpression = 
          (MethodCallExpression) ExpressionHelper.MakeExpression<IQueryable<int>, IQueryable<int>> (q => q.Select (i => i + 1));
      var type = _registry.GetNodeType (closedGenericMethodCallExpression.Method);

      Assert.That (type, Is.SameAs (typeof (SelectExpressionNode)));
    }

    [Test]
    public void GetNodeType_NonGenericMethod ()
    {
      var registry = _registry;
      registry.Register (SumExpressionNode.SupportedMethods, typeof (SumExpressionNode));

      var nonGenericMethod = SumExpressionNode.SupportedMethods[0];
      Assert.That (nonGenericMethod.IsGenericMethod, Is.False);

      var type = registry.GetNodeType (nonGenericMethod);

      Assert.That (type, Is.SameAs (typeof (SumExpressionNode)));
    }

    [Test]
    public void GetNodeType_MethodInClosedGenericType ()
    {
      var methodInOpenGenericType = typeof (GenericClass<>).GetMethod ("NonGenericMethod");
      _registry.Register (new[] { methodInOpenGenericType }, typeof (SelectExpressionNode));

      var methodCallExpressionInClosedGenericType =
          (MethodCallExpression) ExpressionHelper.MakeExpression<GenericClass<int>, bool> (l => l.NonGenericMethod (12));
      var type = _registry.GetNodeType (methodCallExpressionInClosedGenericType.Method);

      Assert.That (type, Is.SameAs (typeof (SelectExpressionNode)));
    }

    [Test]
    [ExpectedException (typeof (KeyNotFoundException), 
        ExpectedMessage = "No corresponding expression node type was registered for method 'System.Linq.Queryable.Select'.")]
    public void GetNodeType_UnknownMethod ()
    {
      var registry = _registry;
      registry.GetNodeType (SelectExpressionNode.SupportedMethods[0]);
    }

    [Test]
    public void Register_SameMethodTwice_OverridesPreviousNodeType ()
    {
      var registry = _registry;
      registry.Register (WhereExpressionNode.SupportedMethods, typeof (SelectExpressionNode));
      registry.Register (WhereExpressionNode.SupportedMethods, typeof (WhereExpressionNode));

      var type = registry.GetNodeType (WhereExpressionNode.SupportedMethods[0]);
      Assert.That (type, Is.SameAs (typeof (WhereExpressionNode)));
    }

    [Test]
    public void IsRegistered_True ()
    {
      var registry = _registry;
      registry.Register (SelectExpressionNode.SupportedMethods, typeof (SelectExpressionNode));

      var result = registry.IsRegistered (SelectExpressionNode.SupportedMethods[0]);
      Assert.That (result, Is.True);
    }

    [Test]
    public void IsRegistered_True_ClosedGenericMethod ()
    {
      var registry = _registry;
      registry.Register (SelectExpressionNode.SupportedMethods, typeof (SelectExpressionNode));

      var closedGenericMethodCallExpression =
          (MethodCallExpression) ExpressionHelper.MakeExpression<IQueryable<int>, IQueryable<int>> (q => q.Select (i => i + 1));
      var result = registry.IsRegistered (closedGenericMethodCallExpression.Method);

      Assert.That (result, Is.True);
    }

    [Test]
    public void IsRegistered_True_NonGenericMethod ()
    {
      var registry = _registry;
      registry.Register (SumExpressionNode.SupportedMethods, typeof (SumExpressionNode));

      var nonGenericMethod = SumExpressionNode.SupportedMethods[0];
      Assert.That (nonGenericMethod.IsGenericMethod, Is.False);

      var result = registry.IsRegistered (nonGenericMethod);

      Assert.That (result, Is.True);
    }

    [Test]
    public void IsRegistered_False ()
    {
      var registry = _registry;
      var result = registry.IsRegistered (SelectExpressionNode.SupportedMethods[0]);
      Assert.That (result, Is.False);
    }

    [Test]
    public void CreateDefault ()
    {
      MethodCallExpressionNodeTypeRegistry registry = MethodCallExpressionNodeTypeRegistry.CreateDefault ();

      AssertAllMethodsRegistered (registry, typeof (CountExpressionNode));
      AssertAllMethodsRegistered (registry, typeof (SelectExpressionNode));
      AssertAllMethodsRegistered (registry, typeof (WhereExpressionNode));
      AssertAllMethodsRegistered (registry, typeof (SelectManyExpressionNode));
      AssertAllMethodsRegistered (registry, typeof (SumExpressionNode));
      AssertAllMethodsRegistered (registry, typeof (FirstExpressionNode));
      AssertAllMethodsRegistered (registry, typeof (LastExpressionNode));
      AssertAllMethodsRegistered (registry, typeof (MinExpressionNode));
      AssertAllMethodsRegistered (registry, typeof (MaxExpressionNode));
      AssertAllMethodsRegistered (registry, typeof (OrderByExpressionNode));
      AssertAllMethodsRegistered (registry, typeof (OrderByDescendingExpressionNode));
      AssertAllMethodsRegistered (registry, typeof (ThenByExpressionNode));
      AssertAllMethodsRegistered (registry, typeof (ThenByDescendingExpressionNode));
      AssertAllMethodsRegistered (registry, typeof (DistinctExpressionNode));
      AssertAllMethodsRegistered (registry, typeof (SingleExpressionNode));
      AssertAllMethodsRegistered (registry, typeof (TakeExpressionNode));
      AssertAllMethodsRegistered (registry, typeof (CastExpressionNode));
      AssertAllMethodsRegistered (registry, typeof (GroupByExpressionNode));
    }

    private void AssertAllMethodsRegistered (MethodCallExpressionNodeTypeRegistry registry, Type type)
    {
      var methodInfos = (MethodInfo[]) type.GetField ("SupportedMethods").GetValue (null);
      Assert.That (methodInfos.Length, Is.GreaterThan (0));

      foreach (var methodInfo in methodInfos)
        Assert.That (registry.GetNodeType (methodInfo), Is.SameAs (type));
    }
  }
}

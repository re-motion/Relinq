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
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.Parsing.Structure.IntermediateModel;
using Remotion.Linq.Parsing.Structure.NodeTypeProviders;
using Remotion.Linq.UnitTests.Linq.Core.Parsing.Structure.TestDomain;

namespace Remotion.Linq.UnitTests.Linq.Core.Parsing.Structure.NodeTypeProviders
{
  [TestFixture]
  public class MethodInfoBasedNodeTypeRegistryTest
  {
    private MethodInfoBasedNodeTypeRegistry _registry;

    [SetUp]
    public void SetUp ()
    {
      _registry = new MethodInfoBasedNodeTypeRegistry ();
    }

    [Test]
    public void GetRegisterableMethodDefinition_OrdinaryMethod ()
    {
      var method = typeof (object).GetMethod ("Equals", BindingFlags.Public | BindingFlags.Instance);
      var registerableMethod = MethodInfoBasedNodeTypeRegistry.GetRegisterableMethodDefinition (method);

      Assert.That (registerableMethod, Is.SameAs (method));
    }

    [Test]
    public void GetRegisterableMethodDefinition_GenericMethodDefinition ()
    {
      var method = ReflectionUtility.GetMethod (() => Queryable.Count<object> (null)).GetGenericMethodDefinition();
      var registerableMethod = MethodInfoBasedNodeTypeRegistry.GetRegisterableMethodDefinition (method);

      Assert.That (registerableMethod, Is.SameAs (method));
    }

    [Test]
    public void GetRegisterableMethodDefinition_ClosedGenericMethod ()
    {
      var method = ReflectionUtility.GetMethod (() => Queryable.Count<object> (null));
      var registerableMethod = MethodInfoBasedNodeTypeRegistry.GetRegisterableMethodDefinition (method);

      Assert.That (registerableMethod, Is.SameAs (method.GetGenericMethodDefinition()));
    }

    [Test]
    public void GetRegisterableMethodDefinition_NonGenericMethod_InGenericTypeDefinition ()
    {
      var method = typeof (GenericClass<>).GetMethod ("NonGenericMethod");
      var registerableMethod = MethodInfoBasedNodeTypeRegistry.GetRegisterableMethodDefinition (method);

      Assert.That (registerableMethod, Is.SameAs (method));
    }

    [Test]
    public void GetRegisterableMethodDefinition_NonGenericMethod_InClosedGenericType ()
    {
      var method = typeof (GenericClass<int>).GetMethod ("NonGenericMethod");
      var registerableMethod = MethodInfoBasedNodeTypeRegistry.GetRegisterableMethodDefinition (method);

      Assert.That (registerableMethod, Is.SameAs (typeof (GenericClass<>).GetMethod ("NonGenericMethod")));
    }

    [Test]
    public void GetRegisterableMethodDefinition_ClosedGenericMethod_InClosedGenericType ()
    {
      var method = typeof (GenericClass<int>).GetMethod ("GenericMethod").MakeGenericMethod (typeof (string));
      var registerableMethod = MethodInfoBasedNodeTypeRegistry.GetRegisterableMethodDefinition (method);

      Assert.That (registerableMethod, Is.SameAs (typeof (GenericClass<>).GetMethod ("GenericMethod")));
    }

    [Test]
    public void Register_WithMethodInfo ()
    {
      Assert.That (_registry.RegisteredMethodInfoCount, Is.EqualTo (0));

      _registry.Register (SelectExpressionNode.SupportedMethods, typeof (SelectExpressionNode));

      Assert.That(_registry.RegisteredMethodInfoCount, Is.EqualTo (2));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException))]
    public void Register_WithMethodInfoAndClosedGenericMethod_NotAllowed ()
    {
      var closedGenericMethod = SelectExpressionNode.SupportedMethods[0].MakeGenericMethod (typeof (int), typeof (int));
      _registry.Register (new[] { closedGenericMethod }, typeof (SelectExpressionNode));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException))]
    public void Register_WithMethodInfoAndMethodInClosedGenericType_NotAllowed ()
    {
      var methodInClosedGenericType = typeof (GenericClass<int>).GetMethod ("NonGenericMethod");
      _registry.Register (new[] { methodInClosedGenericType }, typeof (SelectExpressionNode));
    }

    [Test]
    public void GetNodeType_WithMethodInfo ()
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
    public void GetNodeType_UnknownMethod ()
    {
      var result = _registry.GetNodeType (SelectExpressionNode.SupportedMethods[0]);

      Assert.That (result, Is.Null);
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
    public void CreateFromTypes ()
    {
      MethodInfoBasedNodeTypeRegistry registry = MethodInfoBasedNodeTypeRegistry.CreateFromTypes (
          new[] { typeof (SelectExpressionNode), typeof (WhereExpressionNode) });

      Assert.That (registry.RegisteredMethodInfoCount, 
          Is.EqualTo (SelectExpressionNode.SupportedMethods.Length + WhereExpressionNode.SupportedMethods.Length));

      AssertAllMethodsRegistered (registry, typeof (SelectExpressionNode));
      AssertAllMethodsRegistered (registry, typeof (WhereExpressionNode));
    }

    private void AssertAllMethodsRegistered (MethodInfoBasedNodeTypeRegistry registry, Type type)
    {
      var supportedMethodsField = type.GetField ("SupportedMethods");
      if (supportedMethodsField != null)
      {
        var methodInfos = (MethodInfo[]) supportedMethodsField.GetValue (null);
        Assert.That (methodInfos.Length, Is.GreaterThan (0));

        foreach (var methodInfo in methodInfos)
          Assert.That (registry.GetNodeType (methodInfo), Is.SameAs (type));
      }

      var supportedMethodNamesField = type.GetField ("SupportedMethodNames");
      if (supportedMethodNamesField != null)
      {
        var methodNames = (string[]) supportedMethodNamesField.GetValue (null);
        Assert.That (methodNames.Length, Is.GreaterThan (0));

        foreach (var methodName in methodNames)
          Assert.That (registry.GetNodeType (type.GetMethod(methodName)), Is.SameAs (type));
      }
    }
  }
}
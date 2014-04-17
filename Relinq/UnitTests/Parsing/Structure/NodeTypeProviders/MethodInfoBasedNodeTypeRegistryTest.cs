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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.Parsing.Structure.IntermediateModel;
using Remotion.Linq.Parsing.Structure.NodeTypeProviders;
using Remotion.Linq.UnitTests.Parsing.Structure.TestDomain;
using Remotion.Linq.Utilities;

namespace Remotion.Linq.UnitTests.Parsing.Structure.NodeTypeProviders
{
  [TestFixture]
  public class MethodInfoBasedNodeTypeRegistryTest
  {
    private MethodInfoBasedNodeTypeRegistry _registry;

    [SetUp]
    public void SetUp ()
    {
      _registry = new MethodInfoBasedNodeTypeRegistry();
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
    public void GetRegisterableMethodDefinition_ClosedGenericMethod_InClosedGenericType_HavingOverloadWithSameParameterCount ()
    {
      var method1 = typeof (GenericClass<int>).GetMethods()
          .Where (m => m.Name == "GenericMethodHavingOverloadWithSameParameterCount" && m.GetParameters().Last().ParameterType == typeof (int))
          .Select (m => m.MakeGenericMethod (typeof (double)))
          .Single();

      var method2 = typeof (GenericClass<int>).GetMethods()
          .Where (m => m.Name == "GenericMethodHavingOverloadWithSameParameterCount" && m.GetParameters().Last().ParameterType == typeof (string))
          .Select (m => m.MakeGenericMethod (typeof (double)))
          .Single();

      var expectedMethod1 = typeof (GenericClass<>).GetMethods()
          .Where (m => m.Name == "GenericMethodHavingOverloadWithSameParameterCount" && m.GetParameters().Last().ParameterType == typeof (int))
          .Single();

      var expectedMethod2 = typeof (GenericClass<>).GetMethods()
          .Where (m => m.Name == "GenericMethodHavingOverloadWithSameParameterCount" && m.GetParameters().Last().ParameterType == typeof (string))
          .Single();

      var registerableMethod1 = MethodInfoBasedNodeTypeRegistry.GetRegisterableMethodDefinition (method1);
      var registerableMethod2 = MethodInfoBasedNodeTypeRegistry.GetRegisterableMethodDefinition (method2);

      Assert.That (registerableMethod1, Is.SameAs (expectedMethod1));
      Assert.That (registerableMethod2, Is.SameAs (expectedMethod2));
    }

    [Test]
    public void GetRegisterableMethodDefinition_ClosedGenericMethod_InClosedGenericType_HavingOverloadsDistinguishedByParameterTypeFromGenericType ()
    {
      var method1 = typeof (GenericClass<int, string>).GetMethods()
          .Where (m => m.Name == "NonGenericMethodOverloadedWithGenericParameterFromType" && m.GetParameters().First().ParameterType.Name == "Int32")
          .Single();

      var method2 = typeof (GenericClass<int, string>).GetMethods()
          .Where (m => m.Name == "NonGenericMethodOverloadedWithGenericParameterFromType" && m.GetParameters().First().ParameterType.Name == "String")
          .Single();

      var expectedMethod1 = typeof (GenericClass<,>).GetMethods()
          .Where (m => m.Name == "NonGenericMethodOverloadedWithGenericParameterFromType" && m.GetParameters().First().ParameterType.Name == "T1")
          .Single();

      var expectedMethod2 = typeof (GenericClass<,>).GetMethods()
          .Where (m => m.Name == "NonGenericMethodOverloadedWithGenericParameterFromType" && m.GetParameters().First().ParameterType.Name == "T2")
          .Single();

      var registerableMethod1 = MethodInfoBasedNodeTypeRegistry.GetRegisterableMethodDefinition (method1);
      var registerableMethod2 = MethodInfoBasedNodeTypeRegistry.GetRegisterableMethodDefinition (method2);

      Assert.That (registerableMethod1, Is.SameAs (expectedMethod1));
      Assert.That (registerableMethod2, Is.SameAs (expectedMethod2));
    }

    [Test]
    public void Register_WithMethodInfo ()
    {
      Assert.That (_registry.RegisteredMethodInfoCount, Is.EqualTo (0));

      _registry.Register (SelectExpressionNode.SupportedMethods, typeof (SelectExpressionNode));

      Assert.That (_registry.RegisteredMethodInfoCount, Is.EqualTo (2));
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

      Assert.That (
          registry.RegisteredMethodInfoCount,
          Is.EqualTo (SelectExpressionNode.SupportedMethods.Length + WhereExpressionNode.SupportedMethods.Length));

      AssertAllMethodsRegistered (registry, typeof (SelectExpressionNode));
      AssertAllMethodsRegistered (registry, typeof (WhereExpressionNode));
    }

    [Test]
    public void CreateFromTypes_UsesPublicStaticField ()
    {
      var registry = MethodInfoBasedNodeTypeRegistry.CreateFromTypes (new[] { typeof (TestExpressionNode) });

      Assert.That (registry.RegisteredMethodInfoCount, Is.EqualTo (1));

      Assert.That (registry.GetNodeType (typeof (TestExpressionNode).GetMethod ("RegisteredMethod")), Is.SameAs (typeof (TestExpressionNode)));
    }

    [Test]
    public void CreateFromTypes_WithStaticFieldIgnoresStaticFieldFromBaseType ()
    {
      var registry = MethodInfoBasedNodeTypeRegistry.CreateFromTypes (new[] { typeof (DerivedTestExpressionNode) });

      Assert.That (registry.RegisteredMethodInfoCount, Is.EqualTo (1));

      Assert.That (
          registry.GetNodeType (typeof (DerivedTestExpressionNode).GetMethod ("MethodOnDerivedNode")),
          Is.SameAs (typeof (DerivedTestExpressionNode)));
    }

    [Test]
    public void CreateFromTypes_IgnoresInstanceField ()
    {
      var registry = MethodInfoBasedNodeTypeRegistry.CreateFromTypes (new[] { typeof (TestExpressionNodeWithInstanceField) });

      Assert.That (registry.RegisteredMethodInfoCount, Is.EqualTo (0));
    }

    [Test]
    public void CreateFromTypes_IgnoresNonPublicStaticField ()
    {
      var registry = MethodInfoBasedNodeTypeRegistry.CreateFromTypes (new[] { typeof (TestExpressionNodeWithNonPublicStaticField) });

      Assert.That (registry.RegisteredMethodInfoCount, Is.EqualTo (0));
    }

    [Test]
    public void CreateFromTypes_WithoutStaticField ()
    {
      var registry = MethodInfoBasedNodeTypeRegistry.CreateFromTypes (new[] { typeof (TestExpressionNodeWithoutStaticField) });

      Assert.That (registry.RegisteredMethodInfoCount, Is.EqualTo (0));
    }

    [Test]
    public void CreateFromTypes_WithoutStaticFieldIgnoresStaticFieldFromBaseType ()
    {
      var registry = MethodInfoBasedNodeTypeRegistry.CreateFromTypes (new[] { typeof (DerivedTestExpressionNodeWithoutStaticField) });

      Assert.That (registry.RegisteredMethodInfoCount, Is.EqualTo (0));
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
          Assert.That (registry.GetNodeType (type.GetMethod (methodName)), Is.SameAs (type));
      }
    }


    internal class TestExpressionNode : MethodCallExpressionNodeBase
    {
      public static MethodInfo[] SupportedMethods =
      {
          ReflectionUtility.GetMethod (() => ((TestExpressionNode) null).RegisteredMethod()),
      };

      public TestExpressionNode (MethodCallExpressionParseInfo parseInfo)
          : base (parseInfo)
      {
      }

      public int RegisteredMethod ()
      {
        throw new NotImplementedException();
      }

      public override Expression Resolve (
          ParameterExpression inputParameter,
          Expression expressionToBeResolved,
          ClauseGenerationContext clauseGenerationContext)
      {
        throw new NotImplementedException();
      }

      protected override QueryModel ApplyNodeSpecificSemantics (QueryModel queryModel, ClauseGenerationContext clauseGenerationContext)
      {
        throw new NotImplementedException();
      }
    }

    internal class DerivedTestExpressionNode : TestExpressionNode
    {
      public new static MethodInfo[] SupportedMethods =
      {
          ReflectionUtility.GetMethod (() => ((DerivedTestExpressionNode) null).MethodOnDerivedNode()),
      };

      public DerivedTestExpressionNode (MethodCallExpressionParseInfo parseInfo)
          : base (parseInfo)
      {
      }

      public int MethodOnDerivedNode ()
      {
        throw new NotImplementedException();
      }
    }

    internal class DerivedTestExpressionNodeWithoutStaticField : TestExpressionNode
    {
      public DerivedTestExpressionNodeWithoutStaticField (MethodCallExpressionParseInfo parseInfo)
          : base (parseInfo)
      {
      }
    }

    internal class TestExpressionNodeWithInstanceField : MethodCallExpressionNodeBase
    {
      public MethodInfo[] SupportedMethods =
      {
          ReflectionUtility.GetMethod (() => ((TestExpressionNode) null).Resolve (null, null, new ClauseGenerationContext())),
      };

      public TestExpressionNodeWithInstanceField (MethodCallExpressionParseInfo parseInfo)
          : base (parseInfo)
      {
      }

      public override Expression Resolve (
          ParameterExpression inputParameter,
          Expression expressionToBeResolved,
          ClauseGenerationContext clauseGenerationContext)
      {
        throw new NotImplementedException();
      }

      protected override QueryModel ApplyNodeSpecificSemantics (QueryModel queryModel, ClauseGenerationContext clauseGenerationContext)
      {
        throw new NotImplementedException();
      }
    }

    internal class TestExpressionNodeWithNonPublicStaticField : MethodCallExpressionNodeBase
    {
      protected static MethodInfo[] SupportedMethods =
      {
          ReflectionUtility.GetMethod (() => ((TestExpressionNodeWithNonPublicStaticField) null).Resolve (null, null, new ClauseGenerationContext())),
      };

      public TestExpressionNodeWithNonPublicStaticField (MethodCallExpressionParseInfo parseInfo)
          : base (parseInfo)
      {
      }

      public override Expression Resolve (
          ParameterExpression inputParameter,
          Expression expressionToBeResolved,
          ClauseGenerationContext clauseGenerationContext)
      {
        throw new NotImplementedException();
      }

      protected override QueryModel ApplyNodeSpecificSemantics (QueryModel queryModel, ClauseGenerationContext clauseGenerationContext)
      {
        throw new NotImplementedException();
      }
    }

    internal class TestExpressionNodeWithoutStaticField : MethodCallExpressionNodeBase
    {
      public TestExpressionNodeWithoutStaticField (MethodCallExpressionParseInfo parseInfo)
          : base (parseInfo)
      {
      }

      public override Expression Resolve (
          ParameterExpression inputParameter,
          Expression expressionToBeResolved,
          ClauseGenerationContext clauseGenerationContext)
      {
        throw new NotImplementedException();
      }

      protected override QueryModel ApplyNodeSpecificSemantics (QueryModel queryModel, ClauseGenerationContext clauseGenerationContext)
      {
        throw new NotImplementedException();
      }
    }
  }
}
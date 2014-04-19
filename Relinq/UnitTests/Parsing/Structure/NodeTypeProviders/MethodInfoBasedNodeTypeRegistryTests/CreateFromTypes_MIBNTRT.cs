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
using NUnit.Framework;
using Remotion.Linq.Parsing.Structure.IntermediateModel;
using Remotion.Linq.Parsing.Structure.NodeTypeProviders;
using Remotion.Linq.Utilities;

namespace Remotion.Linq.UnitTests.Parsing.Structure.NodeTypeProviders.MethodInfoBasedNodeTypeRegistryTests
{
  [TestFixture]
  public class CreateFromTypes_MethodInfoBasedNodeTypeRegistryTest
  {
    [Test]
    public void Test ()
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
    public void Test_UsesPublicStaticField ()
    {
      var registry = MethodInfoBasedNodeTypeRegistry.CreateFromTypes (new[] { typeof (TestExpressionNode) });

      Assert.That (registry.RegisteredMethodInfoCount, Is.EqualTo (1));

      Assert.That (registry.GetNodeType (typeof (TestExpressionNode).GetMethod ("RegisteredMethod")), Is.SameAs (typeof (TestExpressionNode)));
    }

    [Test]
    public void Test_WithStaticFieldIgnoresStaticFieldFromBaseType ()
    {
      var registry = MethodInfoBasedNodeTypeRegistry.CreateFromTypes (new[] { typeof (DerivedTestExpressionNode) });

      Assert.That (registry.RegisteredMethodInfoCount, Is.EqualTo (1));

      Assert.That (
          registry.GetNodeType (typeof (DerivedTestExpressionNode).GetMethod ("MethodOnDerivedNode")),
          Is.SameAs (typeof (DerivedTestExpressionNode)));
    }

    [Test]
    public void Test_IgnoresInstanceField ()
    {
      var registry = MethodInfoBasedNodeTypeRegistry.CreateFromTypes (new[] { typeof (TestExpressionNodeWithInstanceField) });

      Assert.That (registry.RegisteredMethodInfoCount, Is.EqualTo (0));
    }

    [Test]
    public void Test_IgnoresNonPublicStaticField ()
    {
      var registry = MethodInfoBasedNodeTypeRegistry.CreateFromTypes (new[] { typeof (TestExpressionNodeWithNonPublicStaticField) });

      Assert.That (registry.RegisteredMethodInfoCount, Is.EqualTo (0));
    }

    [Test]
    public void Test_WithoutStaticField ()
    {
      var registry = MethodInfoBasedNodeTypeRegistry.CreateFromTypes (new[] { typeof (TestExpressionNodeWithoutStaticField) });

      Assert.That (registry.RegisteredMethodInfoCount, Is.EqualTo (0));
    }

    [Test]
    public void Test_WithoutStaticFieldIgnoresStaticFieldFromBaseType ()
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
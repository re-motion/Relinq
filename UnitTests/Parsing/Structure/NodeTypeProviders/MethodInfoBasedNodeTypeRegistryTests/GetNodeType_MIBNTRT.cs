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
using NUnit.Framework;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.Parsing.Structure.IntermediateModel;
using Remotion.Linq.Parsing.Structure.NodeTypeProviders;
using Remotion.Linq.UnitTests.Parsing.Structure.NodeTypeProviders.MethodInfoBasedNodeTypeRegistryTests.TestDomain;
using Remotion.Linq.Utilities;

namespace Remotion.Linq.UnitTests.Parsing.Structure.NodeTypeProviders.MethodInfoBasedNodeTypeRegistryTests
{
  [TestFixture]
  public class GetNodeType_MethodInfoBasedNodeTypeRegistryTest
  {
    private MethodInfoBasedNodeTypeRegistry _registry;

    [SetUp]
    public void SetUp ()
    {
      _registry = new MethodInfoBasedNodeTypeRegistry();
    }

    [Test]
    public void Test_WithMethodInfo ()
    {
      _registry.Register (SelectExpressionNode.GetSupportedMethods(), typeof (SelectExpressionNode));

      var type = _registry.GetNodeType (SelectExpressionNode.GetSupportedMethods().First());

      Assert.That (type, Is.SameAs (typeof (SelectExpressionNode)));
    }

    [Test]
    public void Test_WithMultipleNodes ()
    {
      _registry.Register (SelectExpressionNode.GetSupportedMethods(), typeof (SelectExpressionNode));
      _registry.Register (SumExpressionNode.GetSupportedMethods(), typeof (SumExpressionNode));

      var type1 = _registry.GetNodeType (SelectExpressionNode.GetSupportedMethods().First());
      var type2 = _registry.GetNodeType (SumExpressionNode.GetSupportedMethods().First());
      var type3 = _registry.GetNodeType (SumExpressionNode.GetSupportedMethods().Skip(1).First());

      Assert.That (type1, Is.SameAs (typeof (SelectExpressionNode)));
      Assert.That (type2, Is.SameAs (typeof (SumExpressionNode)));
      Assert.That (type3, Is.SameAs (typeof (SumExpressionNode)));
    }

    [Test]
    public void Test_ClosedGenericMethod ()
    {
      _registry.Register (SelectExpressionNode.GetSupportedMethods(), typeof (SelectExpressionNode));

      var closedGenericMethodCallExpression =
          (MethodCallExpression) ExpressionHelper.MakeExpression<IQueryable<int>, IQueryable<int>> (q => q.Select (i => i + 1));
      var type = _registry.GetNodeType (closedGenericMethodCallExpression.Method);

      Assert.That (type, Is.SameAs (typeof (SelectExpressionNode)));
    }

    [Test]
    public void Test_NonGenericMethod ()
    {
      var registry = _registry;
      registry.Register (SumExpressionNode.GetSupportedMethods(), typeof (SumExpressionNode));

      var nonGenericMethod = ReflectionUtility.GetMethod (() => Queryable.Sum ((IQueryable<int>) null));
      Assert.That (nonGenericMethod.IsGenericMethod, Is.False);

      var type = registry.GetNodeType (nonGenericMethod);

      Assert.That (type, Is.SameAs (typeof (SumExpressionNode)));
    }

    [Test]
    public void Test_MethodInClosedGenericType ()
    {
      var methodInOpenGenericType = typeof (GenericClass<>).GetMethod ("NonGenericMethod");
      _registry.Register (new[] { methodInOpenGenericType }, typeof (SelectExpressionNode));

      var methodCallExpressionInClosedGenericType =
          (MethodCallExpression) ExpressionHelper.MakeExpression<GenericClass<int>, bool> (l => l.NonGenericMethod (12));
      var type = _registry.GetNodeType (methodCallExpressionInClosedGenericType.Method);

      Assert.That (type, Is.SameAs (typeof (SelectExpressionNode)));
    }

    [Test]
    public void Test_AmbiguousClosedGenericMethodInClosedGenericType ()
    {
      var methodInOpenGenericTypes = typeof (GenericClass<,>).GetMethods()
          .Where (mi => mi.Name == "NonGenericMethodOverloadedWithGenericParameterFromTypeAndSameParameterName")
          .ToArray();
      _registry.Register (methodInOpenGenericTypes, typeof (SelectExpressionNode));

      var methodCallExpressionInClosedGenericType =
          (MethodCallExpression) ExpressionHelper.MakeExpression<GenericClass<int, string>, bool> (
              l => l.NonGenericMethodOverloadedWithGenericParameterFromTypeAndSameParameterName ("string", 1.0));

      var result = _registry.GetNodeType (methodCallExpressionInClosedGenericType.Method);

      Assert.That (result, Is.Null);
    }

    [Test]
    public void Test_UnknownMethod ()
    {
      var result = _registry.GetNodeType (SelectExpressionNode.GetSupportedMethods().First());

      Assert.That (result, Is.Null);
    }
  }
}
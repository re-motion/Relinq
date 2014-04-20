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

namespace Remotion.Linq.UnitTests.Parsing.Structure.NodeTypeProviders.MethodInfoBasedNodeTypeRegistryTests
{
  [TestFixture]
  public class IsRegistered_MethodInfoBasedNodeTypeRegistryTest
  {
    private MethodInfoBasedNodeTypeRegistry _registry;

    [SetUp]
    public void SetUp ()
    {
      _registry = new MethodInfoBasedNodeTypeRegistry();
    }

    [Test]
    public void Test_WithRegistered_ReturnsTrue ()
    {
      var registry = _registry;
      registry.Register (SelectExpressionNode.SupportedMethods, typeof (SelectExpressionNode));

      var result = registry.IsRegistered (SelectExpressionNode.SupportedMethods[0]);
      Assert.That (result, Is.True);
    }

    [Test]
    public void Test_WithClosedGenericMethod_ReturnsTrue ()
    {
      var registry = _registry;
      registry.Register (SelectExpressionNode.SupportedMethods, typeof (SelectExpressionNode));

      var closedGenericMethodCallExpression =
          (MethodCallExpression) ExpressionHelper.MakeExpression<IQueryable<int>, IQueryable<int>> (q => q.Select (i => i + 1));
      var result = registry.IsRegistered (closedGenericMethodCallExpression.Method);

      Assert.That (result, Is.True);
    }

    [Test]
    public void Test_WithNonGenericMethod_ReturnsTrue ()
    {
      var registry = _registry;
      registry.Register (SumExpressionNode.SupportedMethods, typeof (SumExpressionNode));

      var nonGenericMethod = SumExpressionNode.SupportedMethods[0];
      Assert.That (nonGenericMethod.IsGenericMethod, Is.False);

      var result = registry.IsRegistered (nonGenericMethod);

      Assert.That (result, Is.True);
    }

    [Test]
    public void Test_WithAmbiguousClosedGenericMethodInClosedGenericType_ReturnsFalse ()
    {
      var methodInOpenGenericTypes = typeof (GenericClass<,>).GetMethods()
          .Where (mi => mi.Name == "NonGenericMethodOverloadedWithGenericParameterFromTypeAndSameParameterName")
          .ToArray();
      _registry.Register (methodInOpenGenericTypes, typeof (SelectExpressionNode));

      var methodCallExpressionInClosedGenericType =
          (MethodCallExpression) ExpressionHelper.MakeExpression<GenericClass<int, string>, bool> (
              l => l.NonGenericMethodOverloadedWithGenericParameterFromTypeAndSameParameterName ("string", 1.0));

      var result = _registry.IsRegistered (methodCallExpressionInClosedGenericType.Method);

      Assert.That (result, Is.False);
    }

    [Test]
    public void Test_WithNotRegistered_ReturnsFalse ()
    {
      var registry = _registry;
      var result = registry.IsRegistered (SelectExpressionNode.SupportedMethods[0]);
      Assert.That (result, Is.False);
    }
  }
}
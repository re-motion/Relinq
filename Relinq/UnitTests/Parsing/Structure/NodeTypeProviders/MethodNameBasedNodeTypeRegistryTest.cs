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
using NUnit.Framework;
using Remotion.Linq.Clauses;
using Remotion.Linq.Parsing.Structure.IntermediateModel;
using Remotion.Linq.Parsing.Structure.NodeTypeProviders;

namespace Remotion.Linq.UnitTests.Parsing.Structure.NodeTypeProviders
{
  [TestFixture]
  public class MethodNameBasedNodeTypeRegistryTest
  {
    private MethodNameBasedNodeTypeRegistry _registry;

    [SetUp]
    public void SetUp ()
    {
      _registry = new MethodNameBasedNodeTypeRegistry ();
    }

    [Test]
    public void Register ()
    {
      Assert.That (_registry.RegisteredNamesCount, Is.EqualTo (0));

      _registry.Register (new[] { new NameBasedRegistrationInfo ("Test", mi => true) }, typeof (SelectExpressionNode));

      Assert.That (_registry.RegisteredNamesCount, Is.EqualTo (1));
    }

    [Test]
    public void GetNodeType_MatchingMethod ()
    {
      var methodInfo = typeof (string).GetMethod ("Concat", new[] { typeof (string), typeof (string) });
      _registry.Register (new[] { new NameBasedRegistrationInfo ("Concat", mi => true) }, typeof (SelectExpressionNode));
      
      var result = _registry.GetNodeType (methodInfo);

      Assert.That (result, Is.SameAs (typeof (SelectExpressionNode)));
    }

    [Test]
    public void GetNodeType_NonMatchingMethod_NameDoesntMatch ()
    {
      var methodInfo = typeof (string).GetMethod ("Concat", new[] { typeof (string), typeof (string) });
      _registry.Register (new[] { new NameBasedRegistrationInfo ("Select", mi => true) }, typeof (SelectExpressionNode));

      var result = _registry.GetNodeType (methodInfo);

      Assert.That (result, Is.Null);
    }

    [Test]
    public void GetNodeType_NonMatchingMethod_FilterReturnsFalse ()
    {
      var methodInfo = typeof (string).GetMethod ("Concat", new[] { typeof (string), typeof (string) });
      _registry.Register (new[] { new NameBasedRegistrationInfo ("Concat", mi => false) }, typeof (SelectExpressionNode));

      var result = _registry.GetNodeType (methodInfo);

      Assert.That (result, Is.Null);
    }

    [Test]
    public void GetNodeType_TwoWithSameName_FirstMatches ()
    {
      var methodInfo = typeof (string).GetMethod ("Concat", new[] { typeof (string), typeof (string) });
      _registry.Register (new[] { new NameBasedRegistrationInfo ("Concat", mi => true) }, typeof (SelectExpressionNode));
      _registry.Register (new[] { new NameBasedRegistrationInfo ("Concat", mi => true) }, typeof (WhereExpressionNode));

      var result = _registry.GetNodeType (methodInfo);

      Assert.That (result, Is.SameAs (typeof (SelectExpressionNode)));
    }

    [Test]
    public void GetNodeType_TwoWithSameName_SecondMatches ()
    {
      var methodInfo = typeof (string).GetMethod ("Concat", new[] { typeof (string), typeof (string) });
      _registry.Register (new[] { new NameBasedRegistrationInfo ("Concat", mi => false) }, typeof (SelectExpressionNode));
      _registry.Register (new[] { new NameBasedRegistrationInfo ("Concat", mi => true) }, typeof (WhereExpressionNode));

      var result = _registry.GetNodeType (methodInfo);

      Assert.That (result, Is.SameAs (typeof (WhereExpressionNode)));
    }

    [Test]
    public void IsRegistered_MatchingMethod ()
    {
      var methodInfo = typeof (string).GetMethod ("Concat", new[] { typeof (string), typeof (string) });
      _registry.Register (new[] { new NameBasedRegistrationInfo ("Concat", mi => true) }, typeof (SelectExpressionNode));

      var result = _registry.IsRegistered (methodInfo);

      Assert.That (result, Is.True);
    }

    [Test]
    public void IsRegistered_NonMatchingMethod_NameDoesntMatch ()
    {
      var methodInfo = typeof (string).GetMethod ("Concat", new[] { typeof (string), typeof (string) });
      _registry.Register (new[] { new NameBasedRegistrationInfo ("Select", mi => true) }, typeof (SelectExpressionNode));

      var result = _registry.IsRegistered (methodInfo);

      Assert.That (result, Is.False);
    }

    [Test]
    public void IsRegistered_NonMatchingMethod_FilterReturnsFalse ()
    {
      var methodInfo = typeof (string).GetMethod ("Concat", new[] { typeof (string), typeof (string) });
      _registry.Register (new[] { new NameBasedRegistrationInfo ("Concat", mi => false) }, typeof (SelectExpressionNode));

      var result = _registry.IsRegistered (methodInfo);

      Assert.That (result, Is.False);
    }

    [Test]
    public void IsRegistered_TwoWithSameName_FirstMatches ()
    {
      var methodInfo = typeof (string).GetMethod ("Concat", new[] { typeof (string), typeof (string) });
      _registry.Register (new[] { new NameBasedRegistrationInfo ("Concat", mi => true) }, typeof (SelectExpressionNode));
      _registry.Register (new[] { new NameBasedRegistrationInfo ("Concat", mi => true) }, typeof (WhereExpressionNode));

      var result = _registry.IsRegistered (methodInfo);

      Assert.That (result, Is.True);
    }

    [Test]
    public void IsRegistered_TwoWithSameName_SecondMatches ()
    {
      var methodInfo = typeof (string).GetMethod ("Concat", new[] { typeof (string), typeof (string) });
      _registry.Register (new[] { new NameBasedRegistrationInfo ("Concat", mi => false) }, typeof (SelectExpressionNode));
      _registry.Register (new[] { new NameBasedRegistrationInfo ("Concat", mi => true) }, typeof (WhereExpressionNode));

      var result = _registry.IsRegistered (methodInfo);

      Assert.That (result, Is.True);
    }

    [Test]
    public void CreateFromTypes ()
    {
      var registry = MethodNameBasedNodeTypeRegistry.CreateFromTypes (new[] { typeof (TestExpressionNode) });

      Assert.That (registry.RegisteredNamesCount, Is.EqualTo (2));

      Assert.That (registry.GetNodeType (typeof (TestExpressionNode).GetMethod ("Resolve")), Is.SameAs (typeof (TestExpressionNode)));
    }

    internal class TestExpressionNode : ResultOperatorExpressionNodeBase
    {
      public static NameBasedRegistrationInfo[] SupportedMethodNames = {
          new NameBasedRegistrationInfo ("Resolve", mi => true),
          new NameBasedRegistrationInfo ("CreateResultOperator", mi => false),
      };

      public TestExpressionNode (MethodCallExpressionParseInfo parseInfo, LambdaExpression optionalPredicate, LambdaExpression optionalSelector)
        : base (parseInfo, optionalPredicate, optionalSelector)
      {
      }

      public override Expression Resolve (ParameterExpression inputParameter, Expression expressionToBeResolved, ClauseGenerationContext clauseGenerationContext)
      {
        throw new NotImplementedException ();
      }

      protected override ResultOperatorBase CreateResultOperator (ClauseGenerationContext clauseGenerationContext)
      {
        throw new NotImplementedException ();
      }
    }
  }
}

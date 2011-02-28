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
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Parsing.Structure.IntermediateModel;
using Remotion.Data.Linq.Parsing.Structure.NodeTypeProviders;

namespace Remotion.Data.Linq.UnitTests.Linq.Core.Parsing.Structure.NodeTypeProviders
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

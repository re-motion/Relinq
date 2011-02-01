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
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Parsing.Structure;
using Remotion.Data.Linq.Parsing.Structure.IntermediateModel;

namespace Remotion.Data.Linq.UnitTests.Linq.Core.Parsing.Structure
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
    public void Register_WithNames ()
    {
      Assert.That (_registry.RegisteredNamesCount, Is.EqualTo (0));

      _registry.Register (new[] { "Test" }, typeof (SelectExpressionNode));

      Assert.That (_registry.RegisteredNamesCount, Is.EqualTo (1));
    }

    [Test]
    public void GetNodeType_WithNames ()
    {
      var methodInfo = typeof (string).GetMethod ("Concat", new[] { typeof (string), typeof (string) });
      _registry.Register (new[] { "Concat" }, typeof (SelectExpressionNode));

      var result = _registry.GetNodeType (methodInfo);

      Assert.That (result, Is.SameAs (typeof (SelectExpressionNode)));
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
      registry.Register (new[] { "Where" }, typeof (SelectExpressionNode));
      registry.Register (new[] { "Where" }, typeof (WhereExpressionNode));

      var type = registry.GetNodeType (WhereExpressionNode.SupportedMethods[0]);
      Assert.That (type, Is.SameAs (typeof (WhereExpressionNode)));
    }

    [Test]
    public void IsRegistered_True ()
    {
      var registry = _registry;
      registry.Register (new[] { "Select" }, typeof (SelectExpressionNode));

      var result = registry.IsRegistered (SelectExpressionNode.SupportedMethods[0]);
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
    [Ignore ("TODO 3343: Contains should be registered by name")]
    public void CreateDefault ()
    {
      MethodNameBasedNodeTypeRegistry registry = MethodNameBasedNodeTypeRegistry.CreateDefault ();
      registry.Register (new[] { "Resolve" }, typeof (TestExpressionNode));

      AssertAllMethodsRegistered (registry, typeof (TestExpressionNode));
      AssertAllMethodsRegistered (registry, typeof (ContainsExpressionNode));
    }

    private void AssertAllMethodsRegistered (MethodNameBasedNodeTypeRegistry registry, Type type)
    {
      var supportedMethodNamesField = type.GetField ("SupportedMethodNames");
      if (supportedMethodNamesField != null)
      {
        var methodNames = (string[]) supportedMethodNamesField.GetValue (null);
        Assert.That (methodNames.Length, Is.GreaterThan (0));

        foreach (var methodName in methodNames)
          Assert.That (registry.GetNodeType (type.GetMethod(methodName)), Is.SameAs (type));
      }
    }

    internal class TestExpressionNode : ResultOperatorExpressionNodeBase
    {
      public static string[] SupportedMethodNames = { "Resolve" };

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

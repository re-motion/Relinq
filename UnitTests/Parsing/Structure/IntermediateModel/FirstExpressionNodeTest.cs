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
using NUnit.Framework;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.Parsing.Structure.IntermediateModel;
using Remotion.Linq.UnitTests.TestDomain;
using Remotion.Linq.Utilities;

namespace Remotion.Linq.UnitTests.Parsing.Structure.IntermediateModel
{
  [TestFixture]
  public class FirstExpressionNodeTest : ExpressionNodeTestBase
  {
    private FirstExpressionNode _node;

    [SetUp]
    public override void SetUp ()
    {
      base.SetUp();

      _node = new FirstExpressionNode (CreateParseInfo (), null);
    }

    [Test]
    public void GetSupportedMethods ()
    {
      Assert.That (
          FirstExpressionNode.GetSupportedMethods(),
          Is.EquivalentTo (
              new[]
              {
                  GetGenericMethodDefinition (() => Queryable.First<object> (null)),
                  GetGenericMethodDefinition (() => Queryable.First<object> (null, null)),
                  GetGenericMethodDefinition (() => Queryable.FirstOrDefault<object> (null)),
                  GetGenericMethodDefinition (() => Queryable.FirstOrDefault<object> (null, null)),
                  GetGenericMethodDefinition (() => Enumerable.First<object> (null)),
                  GetGenericMethodDefinition (() => Enumerable.First<object> (null, null)),
                  GetGenericMethodDefinition (() => Enumerable.FirstOrDefault<object> (null)),
                  GetGenericMethodDefinition (() => Enumerable.FirstOrDefault<object> (null, null)),
              }));
    }

    [Test]
    public void Resolve_ThrowsInvalidOperationException ()
    {
      Assert.That (
          () => _node.Resolve (ExpressionHelper.CreateParameterExpression (), ExpressionHelper.CreateExpression (), ClauseGenerationContext),
          Throws.InstanceOf<NotSupportedException>()
              .With.Message.EqualTo (
                  "FirstExpressionNode does not support resolving of expressions, because it does not stream any data to the following node."));
    }

    [Test]
    public void Apply ()
    {
      TestApply (_node, typeof (FirstResultOperator));
    }

    [Test]
    public void Apply_NoDefaultAllowed ()
    {
      var node = new FirstExpressionNode (CreateParseInfo (ReflectionUtility.GetMethod (() => Queryable.First<Cook> (null))), null);
      node.Apply (QueryModel, ClauseGenerationContext);
      
      Assert.That (((FirstResultOperator) QueryModel.ResultOperators[0]).ReturnDefaultWhenEmpty, Is.False);
    }

    [Test]
    public void Apply_DefaultAllowed ()
    {
      var node = new FirstExpressionNode (CreateParseInfo (ReflectionUtility.GetMethod (() => Queryable.FirstOrDefault<Cook> (null, null))), null);
      node.Apply (QueryModel, ClauseGenerationContext);
      
      Assert.That (((FirstResultOperator) QueryModel.ResultOperators[0]).ReturnDefaultWhenEmpty, Is.True);
    }
  }
}

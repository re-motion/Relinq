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

namespace Remotion.Linq.UnitTests.Parsing.Structure.IntermediateModel
{
  [TestFixture]
  public class AnyExpressionNodeTest : ExpressionNodeTestBase
  {
    private AnyExpressionNode _node;

    public override void SetUp ()
    {
      base.SetUp ();
      _node = new AnyExpressionNode (CreateParseInfo (), null);
    }

    [Test]
    public void GetSupportedMethods ()
    {
      Assert.That (
          AnyExpressionNode.GetSupportedMethods(),
          Is.EquivalentTo (
              new[]
              {
                  GetGenericMethodDefinition (() => Queryable.Any<object> (null)),
                  GetGenericMethodDefinition (() => Queryable.Any<object> (null, null)),
                  GetGenericMethodDefinition (() => Enumerable.Any<object> (null)),
                  GetGenericMethodDefinition (() => Enumerable.Any<object> (null, null))
              }));
    }

    [Test]
    public void Initialization_WithPredicate ()
    {
      var parseInfo = CreateParseInfo ();
      var predicate = ExpressionHelper.CreateLambdaExpression<int, bool> (i => i > 5);
      var node = new AnyExpressionNode (parseInfo, predicate);

      Assert.That (node.Source, Is.InstanceOf (typeof (WhereExpressionNode)));
      Assert.That (((WhereExpressionNode) node.Source).Predicate, Is.SameAs (predicate));
      Assert.That (((WhereExpressionNode) node.Source).Source, Is.SameAs (SourceNode));
    }

    [Test]
    public void Initialization_WithoutPredicate ()
    {
      var parseInfo = CreateParseInfo ();
      var node = new AnyExpressionNode (parseInfo, null);

      Assert.That (node.Source, Is.SameAs (SourceNode));
    }

    [Test]
    public void Resolve_ThrowsInvalidOperationException ()
    {
      Assert.That (
          () => _node.Resolve (ExpressionHelper.CreateParameterExpression (), ExpressionHelper.CreateExpression (), ClauseGenerationContext),
          Throws.InstanceOf<NotSupportedException>()
              .With.Message.EqualTo (
                  "AnyExpressionNode does not support resolving of expressions, because it does not stream any data to the following node."));
    }

    [Test]
    public void Apply ()
    {
      TestApply (_node, typeof (AnyResultOperator));
    }
  }
}

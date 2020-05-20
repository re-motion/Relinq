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
  public class LongLongCountExpressionNodeTest : ExpressionNodeTestBase
  {
    private LongCountExpressionNode _node;

    public override void SetUp ()
    {
      base.SetUp ();
      _node = new LongCountExpressionNode (CreateParseInfo (), null);
    }

    [Test]
    public void GetSupportedMethods ()
    {
      Assert.That (
          LongCountExpressionNode.GetSupportedMethods(),
          Is.EquivalentTo (
              new[]
              {
                  GetGenericMethodDefinition (() => Queryable.LongCount<object> (null)),
                  GetGenericMethodDefinition (() => Queryable.LongCount<object> (null, null)),
                  GetGenericMethodDefinition (() => Enumerable.LongCount<object> (null)),
                  GetGenericMethodDefinition (() => Enumerable.LongCount<object> (null, null)),

                  GetGenericMethodDefinition (() => (((Array) null).LongLength)),
              }));
    }

    [Test]
    public void Resolve_ThrowsInvalidOperationException ()
    {
      Assert.That (
          () => _node.Resolve (ExpressionHelper.CreateParameterExpression (), ExpressionHelper.CreateExpression (), ClauseGenerationContext),
          Throws.InstanceOf<NotSupportedException>()
              .With.Message.EqualTo (
                  "LongCountExpressionNode does not support resolving of expressions, because it does not stream any data to the following node."));
    }

    [Test]
    public void Apply ()
    {
      TestApply (_node, typeof (LongCountResultOperator));
    }
  }
}

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
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.Parsing.Structure.IntermediateModel;

namespace Remotion.Linq.UnitTests.Parsing.Structure.IntermediateModel
{
  [TestFixture]
  public class MaxExpressionNodeTest : ExpressionNodeTestBase
  {
    private MaxExpressionNode _node;

    public override void SetUp ()
    {
      base.SetUp ();
      _node = new MaxExpressionNode (CreateParseInfo (), null);
    }

    [Test]
    public void GetSupportedMethods ()
    {
      Assert.That (
          MaxExpressionNode.GetSupportedMethods(),
          Is.EquivalentTo (
              new[]
              {
                  GetGenericMethodDefinition (() => Queryable.Max<object> (null)),
                  GetGenericMethodDefinition (() => Queryable.Max<object, object> (null, null)),
                  GetGenericMethodDefinition (() => Enumerable.Max<object> (null)),
                  GetGenericMethodDefinition (() => Enumerable.Max<object, object> (null, null)),

                  GetGenericMethodDefinition (() => Enumerable.Max ((IEnumerable<decimal>) null)),
                  GetGenericMethodDefinition (() => Enumerable.Max ((IEnumerable<decimal?>) null)),
                  GetGenericMethodDefinition (() => Enumerable.Max ((IEnumerable<double>) null)),
                  GetGenericMethodDefinition (() => Enumerable.Max ((IEnumerable<double?>) null)),
                  GetGenericMethodDefinition (() => Enumerable.Max ((IEnumerable<int>) null)),
                  GetGenericMethodDefinition (() => Enumerable.Max ((IEnumerable<int?>) null)),
                  GetGenericMethodDefinition (() => Enumerable.Max ((IEnumerable<long>) null)),
                  GetGenericMethodDefinition (() => Enumerable.Max ((IEnumerable<long?>) null)),
                  GetGenericMethodDefinition (() => Enumerable.Max ((IEnumerable<float>) null)),
                  GetGenericMethodDefinition (() => Enumerable.Max ((IEnumerable<float?>) null)),
                  GetGenericMethodDefinition (() => Enumerable.Max<object> (null, o => (decimal) 0)),
                  GetGenericMethodDefinition (() => Enumerable.Max<object> (null, o => (decimal?) 0)),
                  GetGenericMethodDefinition (() => Enumerable.Max<object> (null, o => (double) 0)),
                  GetGenericMethodDefinition (() => Enumerable.Max<object> (null, o => (double?) 0)),
                  GetGenericMethodDefinition (() => Enumerable.Max<object> (null, o => (int) 0)),
                  GetGenericMethodDefinition (() => Enumerable.Max<object> (null, o => (int?) 0)),
                  GetGenericMethodDefinition (() => Enumerable.Max<object> (null, o => (long) 0)),
                  GetGenericMethodDefinition (() => Enumerable.Max<object> (null, o => (long?) 0)),
                  GetGenericMethodDefinition (() => Enumerable.Max<object> (null, o => (float) 0)),
                  GetGenericMethodDefinition (() => Enumerable.Max<object> (null, o => (float?) 0))
              }));
    }

    [Test]
    public void Resolve_ThrowsInvalidOperationException ()
    {
      Assert.That (
          () => _node.Resolve (ExpressionHelper.CreateParameterExpression (), ExpressionHelper.CreateExpression (), ClauseGenerationContext),
          Throws.InstanceOf<NotSupportedException>()
              .With.Message.EqualTo (
                  "MaxExpressionNode does not support resolving of expressions, because it does not stream any data to the following node."));
    }

    [Test]
    public void ApplySelector ()
    {
      TestApply (_node, typeof (MaxResultOperator));
    }
  }
}

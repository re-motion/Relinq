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
    public void SupportedMethod_WithoutSelector ()
    {
      AssertSupportedMethod_Generic (MaxExpressionNode.SupportedMethods, q => q.Max(), e => e.Max());
    }

    [Test]
    public void SupportedMethod_WithSelector ()
    {
      AssertSupportedMethod_Generic (MaxExpressionNode.SupportedMethods, q => q.Max (i => i.ToString()), e => e.Max (i => i.ToString()));
    }

    [Test]
    public void SupportedMethod_IEnumerableOverloads ()
    {
      AssertSupportedMethod_NonGeneric (MaxExpressionNode.SupportedMethods, null, e => ((IEnumerable<decimal>) e).Max ());
      AssertSupportedMethod_NonGeneric (MaxExpressionNode.SupportedMethods, null, e => ((IEnumerable<decimal?>) e).Max ());
      AssertSupportedMethod_NonGeneric (MaxExpressionNode.SupportedMethods, null, e => ((IEnumerable<double>) e).Max ());
      AssertSupportedMethod_NonGeneric (MaxExpressionNode.SupportedMethods, null, e => ((IEnumerable<double?>) e).Max ());
      AssertSupportedMethod_NonGeneric (MaxExpressionNode.SupportedMethods, null, e => ((IEnumerable<int>) e).Max ());
      AssertSupportedMethod_NonGeneric (MaxExpressionNode.SupportedMethods, null, e => ((IEnumerable<int?>) e).Max ());
      AssertSupportedMethod_NonGeneric (MaxExpressionNode.SupportedMethods, null, e => ((IEnumerable<long>) e).Max ());
      AssertSupportedMethod_NonGeneric (MaxExpressionNode.SupportedMethods, null, e => ((IEnumerable<long?>) e).Max ());
      AssertSupportedMethod_NonGeneric (MaxExpressionNode.SupportedMethods, null, e => ((IEnumerable<float>) e).Max ());
      AssertSupportedMethod_NonGeneric (MaxExpressionNode.SupportedMethods, null, e => ((IEnumerable<float?>) e).Max ());
      AssertSupportedMethod_Generic<object, decimal> (MaxExpressionNode.SupportedMethods, null, e => e.Max (i => 0.0m));
      AssertSupportedMethod_Generic<object, decimal?> (MaxExpressionNode.SupportedMethods, null, e => e.Max (i => (decimal?) 0.0m));
      AssertSupportedMethod_Generic<object, double> (MaxExpressionNode.SupportedMethods, null, e => e.Max (i => 0.0));
      AssertSupportedMethod_Generic<object, double?> (MaxExpressionNode.SupportedMethods, null, e => e.Max (i => (double?) 0.0));
      AssertSupportedMethod_Generic<object, int> (MaxExpressionNode.SupportedMethods, null, e => e.Max (i => 0));
      AssertSupportedMethod_Generic<object, int?> (MaxExpressionNode.SupportedMethods, null, e => e.Max (i => (int?) 0));
      AssertSupportedMethod_Generic<object, long> (MaxExpressionNode.SupportedMethods, null, e => e.Max (i => 0L));
      AssertSupportedMethod_Generic<object, long?> (MaxExpressionNode.SupportedMethods, null, e => e.Max (i => (long?) 0L));
      AssertSupportedMethod_Generic<object, float> (MaxExpressionNode.SupportedMethods, null, e => e.Max (i => 0.0f));
      AssertSupportedMethod_Generic<object, float?> (MaxExpressionNode.SupportedMethods, null, e => e.Max (i => (float?) 0.0f));
    }


    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = 
        "MaxExpressionNode does not support resolving of expressions, because it does not stream any data to the following node.")]
    public void Resolve_ThrowsInvalidOperationException ()
    {
      _node.Resolve (ExpressionHelper.CreateParameterExpression (), ExpressionHelper.CreateExpression (), ClauseGenerationContext);
    }

    [Test]
    public void ApplySelector ()
    {
      TestApply (_node, typeof (MaxResultOperator));
    }
  }
}

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
  public class MinExpressionNodeTest : ExpressionNodeTestBase
  {
    private MinExpressionNode _node;

    public override void SetUp ()
    {
      base.SetUp ();
      _node = new MinExpressionNode (CreateParseInfo (), null);
    }

    [Test]
    public void SupportedMethod_WithoutSelector ()
    {
      AssertSupportedMethod_Generic (MinExpressionNode.SupportedMethods, q => q.Min (), e => e.Min ());
    }

    [Test]
    public void SupportedMethod_WithSelector ()
    {
      AssertSupportedMethod_Generic (MinExpressionNode.SupportedMethods, q => q.Min (i => i.ToString ()), e => e.Min (i => i.ToString ()));
    }

    [Test]
    public void SupportedMethod_IEnumerableOverloads ()
    {
      AssertSupportedMethod_NonGeneric (MinExpressionNode.SupportedMethods, null, e => ((IEnumerable<decimal>) e).Min ());
      AssertSupportedMethod_NonGeneric (MinExpressionNode.SupportedMethods, null, e => ((IEnumerable<decimal?>) e).Min ());
      AssertSupportedMethod_NonGeneric (MinExpressionNode.SupportedMethods, null, e => ((IEnumerable<double>) e).Min ());
      AssertSupportedMethod_NonGeneric (MinExpressionNode.SupportedMethods, null, e => ((IEnumerable<double?>) e).Min ());
      AssertSupportedMethod_NonGeneric (MinExpressionNode.SupportedMethods, null, e => ((IEnumerable<int>) e).Min ());
      AssertSupportedMethod_NonGeneric (MinExpressionNode.SupportedMethods, null, e => ((IEnumerable<int?>) e).Min ());
      AssertSupportedMethod_NonGeneric (MinExpressionNode.SupportedMethods, null, e => ((IEnumerable<long>) e).Min ());
      AssertSupportedMethod_NonGeneric (MinExpressionNode.SupportedMethods, null, e => ((IEnumerable<long?>) e).Min ());
      AssertSupportedMethod_NonGeneric (MinExpressionNode.SupportedMethods, null, e => ((IEnumerable<float>) e).Min ());
      AssertSupportedMethod_NonGeneric (MinExpressionNode.SupportedMethods, null, e => ((IEnumerable<float?>) e).Min ());
      AssertSupportedMethod_Generic<object, decimal> (MinExpressionNode.SupportedMethods, null, e => e.Min (i => 0.0m));
      AssertSupportedMethod_Generic<object, decimal?> (MinExpressionNode.SupportedMethods, null, e => e.Min (i => (decimal?) 0.0m));
      AssertSupportedMethod_Generic<object, double> (MinExpressionNode.SupportedMethods, null, e => e.Min (i => 0.0));
      AssertSupportedMethod_Generic<object, double?> (MinExpressionNode.SupportedMethods, null, e => e.Min (i => (double?) 0.0));
      AssertSupportedMethod_Generic<object, int> (MinExpressionNode.SupportedMethods, null, e => e.Min (i => 0));
      AssertSupportedMethod_Generic<object, int?> (MinExpressionNode.SupportedMethods, null, e => e.Min (i => (int?) 0));
      AssertSupportedMethod_Generic<object, long> (MinExpressionNode.SupportedMethods, null, e => e.Min (i => 0L));
      AssertSupportedMethod_Generic<object, long?> (MinExpressionNode.SupportedMethods, null, e => e.Min (i => (long?) 0L));
      AssertSupportedMethod_Generic<object, float> (MinExpressionNode.SupportedMethods, null, e => e.Min (i => 0.0f));
      AssertSupportedMethod_Generic<object, float?> (MinExpressionNode.SupportedMethods, null, e => e.Min (i => (float?) 0.0f));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException))]
    public void Resolve_ThrowsInvalidOperationException ()
    {
      _node.Resolve (ExpressionHelper.CreateParameterExpression (), ExpressionHelper.CreateExpression (), ClauseGenerationContext);
    }

    [Test]
    public void Apply ()
    {
      TestApply (_node, typeof (MinResultOperator));
    }
  }
}

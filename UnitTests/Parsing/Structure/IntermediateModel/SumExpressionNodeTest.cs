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
  public class SumExpressionNodeTest : ExpressionNodeTestBase
  {
    private SumExpressionNode _node;

    public override void SetUp ()
    {
      base.SetUp();
      _node = new SumExpressionNode (CreateParseInfo(), null);
    }

    [Test]
    public void SupportedMethod_WithoutSelector_OnDecimal ()
    {
      AssertSupportedMethod_NonGeneric (
          SumExpressionNode.SupportedMethods, 
          q => ((IQueryable<decimal>) q).Sum(),
          e => ((IEnumerable<decimal>) e).Sum ());
    }

    [Test]
    public void SupportedMethod_WithoutSelector_OnNDecimal ()
    {
      AssertSupportedMethod_NonGeneric (SumExpressionNode.SupportedMethods, q => ((IQueryable<decimal?>) q).Sum(), e => ((IEnumerable<decimal?>) e).Sum());
    }

    [Test]
    public void SupportedMethod_WithoutSelector_OnDouble ()
    {
      AssertSupportedMethod_NonGeneric (SumExpressionNode.SupportedMethods, q => ((IQueryable<double>) q).Sum(), e => ((IEnumerable<double>) e).Sum());
    }

    [Test]
    public void SupportedMethod_WithoutSelector_OnNDouble ()
    {
      AssertSupportedMethod_NonGeneric (SumExpressionNode.SupportedMethods, q => ((IQueryable<double?>) q).Sum(), e => ((IEnumerable<double?>) e).Sum());
    }

    [Test]
    public void SupportedMethod_WithoutSelector_OnSingle ()
    {
      AssertSupportedMethod_NonGeneric (SumExpressionNode.SupportedMethods, q => ((IQueryable<float>) q).Sum(), e => ((IEnumerable<float>) e).Sum());
    }

    [Test]
    public void SupportedMethod_WithoutSelector_OnNSingle ()
    {
      AssertSupportedMethod_NonGeneric (SumExpressionNode.SupportedMethods, q => ((IQueryable<float?>) q).Sum(), e => ((IEnumerable<float?>) e).Sum());
    }

    [Test]
    public void SupportedMethod_WithoutSelector_OnInt32 ()
    {
      AssertSupportedMethod_NonGeneric (SumExpressionNode.SupportedMethods, q => ((IQueryable<int>) q).Sum (), e => ((IEnumerable<int>) e).Sum ());
    }

    [Test]
    public void SupportedMethod_WithoutSelector_OnNInt32 ()
    {
      AssertSupportedMethod_NonGeneric (SumExpressionNode.SupportedMethods, q => ((IQueryable<int?>) q).Sum(), e => ((IEnumerable<int?>) e).Sum());
    }

    [Test]
    public void SupportedMethod_WithoutSelector_OnInt64 ()
    {
      AssertSupportedMethod_NonGeneric (SumExpressionNode.SupportedMethods, q => ((IQueryable<long>) q).Sum(), e => ((IEnumerable<long>) e).Sum());
    }

    [Test]
    public void SupportedMethod_WithoutSelector_OnNInt64 ()
    {
      AssertSupportedMethod_NonGeneric (SumExpressionNode.SupportedMethods, q => ((IQueryable<long?>) q).Sum(), e => ((IEnumerable<long?>) e).Sum());
    }

    [Test]
    public void SupportedMethod_WithDecimalSelector ()
    {
      AssertSupportedMethod_Generic (SumExpressionNode.SupportedMethods, q => q.Sum (i => 0.0m), e => e.Sum (i => 0.0m));
    }

    [Test]
    public void SupportedMethod_WithNDecimalSelector ()
    {
      AssertSupportedMethod_Generic (SumExpressionNode.SupportedMethods, q => q.Sum (i => (decimal?) 0.0m), e => e.Sum (i => (decimal?) 0.0m));
    }

    [Test]
    public void SupportedMethod_WithDoubleSelector ()
    {
      AssertSupportedMethod_Generic (SumExpressionNode.SupportedMethods, q => q.Sum (i => 0.0), e => e.Sum (i => 0.0));
    }

    [Test]
    public void SupportedMethod_WithNDoubleSelector ()
    {
      AssertSupportedMethod_Generic (SumExpressionNode.SupportedMethods, q => q.Sum (i => (double?) 0.0), e => e.Sum (i => (double?) 0.0));
    }

    [Test]
    public void SupportedMethod_WithSingleSelector ()
    {
      AssertSupportedMethod_Generic (SumExpressionNode.SupportedMethods, q => q.Sum (i => 0.0f), e => e.Sum (i => 0.0f));
    }

    [Test]
    public void SupportedMethod_WithNSingleSelector ()
    {
      AssertSupportedMethod_Generic (SumExpressionNode.SupportedMethods, q => q.Sum (i => (float?) 0.0f), e => e.Sum (i => (float?) 0.0f));
    }

    [Test]
    public void SupportedMethod_WithInt32Selector ()
    {
      AssertSupportedMethod_Generic (SumExpressionNode.SupportedMethods, q => q.Sum (i => 0), e => e.Sum (i => 0));
    }

    [Test]
    public void SupportedMethod_WithNInt32Selector ()
    {
      AssertSupportedMethod_Generic (SumExpressionNode.SupportedMethods, q => q.Sum (i => (int?) 0), e => e.Sum (i => (int?) 0));
    }

    [Test]
    public void SupportedMethod_WithInt64Selector ()
    {
      AssertSupportedMethod_Generic (SumExpressionNode.SupportedMethods, q => q.Sum (i => 0L), e => e.Sum (i => 0L));
    }

    [Test]
    public void SupportedMethod_WithNInt64Selector ()
    {
      AssertSupportedMethod_Generic (SumExpressionNode.SupportedMethods, q => q.Sum (i => (long?) 0L), e => e.Sum (i => (long?) 0L));
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = 
        "SumExpressionNode does not support resolving of expressions, because it does not stream any data to the following node.")]
    public void Resolve_ThrowsInvalidOperationException ()
    {
      _node.Resolve (ExpressionHelper.CreateParameterExpression(), ExpressionHelper.CreateExpression(), ClauseGenerationContext);
    }

    [Test]
    public void Apply ()
    {
      TestApply (_node, typeof (SumResultOperator));
    }
  }
}

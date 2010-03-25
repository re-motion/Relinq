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
using System.Linq;
using NUnit.Framework;
using Remotion.Data.Linq.Clauses.ResultOperators;
using Remotion.Data.Linq.Parsing.Structure.IntermediateModel;

namespace Remotion.Data.Linq.UnitTests.Linq.Core.Parsing.Structure.IntermediateModel
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
    [ExpectedException (typeof (InvalidOperationException))]
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

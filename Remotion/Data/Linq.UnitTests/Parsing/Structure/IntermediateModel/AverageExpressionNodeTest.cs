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
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Clauses.ResultOperators;
using Remotion.Data.Linq.Parsing.Structure.IntermediateModel;

namespace Remotion.Data.Linq.UnitTests.Parsing.Structure.IntermediateModel
{
  [TestFixture]
  public class AverageExpressionNodeTest : ExpressionNodeTestBase
  {
    private AverageExpressionNode _node;

    [SetUp]
    public override void SetUp ()
    {
      base.SetUp();
      _node = new AverageExpressionNode (CreateParseInfo(), null);
    }

    [Test]
    public void SupportedMethod_WithoutSelector_OnDecimal ()
    {
      AssertSupportedMethod_NonGeneric (AverageExpressionNode.SupportedMethods, q => ((IQueryable<decimal>) q).Average (), e => ((IEnumerable<decimal>) e).Average ());
    }

    [Test]
    public void SupportedMethod_WithoutSelector_OnNDecimal ()
    {
      AssertSupportedMethod_NonGeneric (AverageExpressionNode.SupportedMethods, q => ((IQueryable<decimal?>) q).Average (), e => ((IEnumerable<decimal?>) e).Average ());
    }

    [Test]
    public void SupportedMethod_WithoutSelector_OnDouble ()
    {
      AssertSupportedMethod_NonGeneric (AverageExpressionNode.SupportedMethods, q => ((IQueryable<double>) q).Average (), e => ((IEnumerable<double>) e).Average ());
    }

    [Test]
    public void SupportedMethod_WithoutSelector_OnNDouble ()
    {
      AssertSupportedMethod_NonGeneric (AverageExpressionNode.SupportedMethods, q => ((IQueryable<double?>) q).Average (), e => ((IEnumerable<double?>) e).Average ());
    }

    [Test]
    public void SupportedMethod_WithoutSelector_OnSingle ()
    {
      AssertSupportedMethod_NonGeneric (AverageExpressionNode.SupportedMethods, q => ((IQueryable<float>) q).Average (), e => ((IEnumerable<float>) e).Average ());
    }

    [Test]
    public void SupportedMethod_WithoutSelector_OnNSingle ()
    {
      AssertSupportedMethod_NonGeneric (AverageExpressionNode.SupportedMethods, q => ((IQueryable<float?>) q).Average (), e => ((IEnumerable<float?>) e).Average ());
    }

    [Test]
    public void SupportedMethod_WithoutSelector_OnInt32 ()
    {
      AssertSupportedMethod_NonGeneric (AverageExpressionNode.SupportedMethods, q => ((IQueryable<int>) q).Average (), e => ((IEnumerable<int>) e).Average ());
    }

    [Test]
    public void SupportedMethod_WithoutSelector_OnNInt32 ()
    {
      AssertSupportedMethod_NonGeneric (AverageExpressionNode.SupportedMethods, q => ((IQueryable<int?>) q).Average (), e => ((IEnumerable<int?>) e).Average ());
    }

    [Test]
    public void SupportedMethod_WithoutSelector_OnInt64 ()
    {
      AssertSupportedMethod_NonGeneric (AverageExpressionNode.SupportedMethods, q => ((IQueryable<long>) q).Average (), e => ((IEnumerable<long>) e).Average ());
    }

    [Test]
    public void SupportedMethod_WithoutSelector_OnNInt64 ()
    {
      AssertSupportedMethod_NonGeneric (AverageExpressionNode.SupportedMethods, q => ((IQueryable<long?>) q).Average (), e => ((IEnumerable<long?>) e).Average ());
    }

    [Test]
    public void SupportedMethod_WithDecimalSelector ()
    {
      AssertSupportedMethod_Generic (AverageExpressionNode.SupportedMethods, q => q.Average (i => 0.0m), e => e.Average (i => 0.0m));
    }

    [Test]
    public void SupportedMethod_WithNDecimalSelector ()
    {
      AssertSupportedMethod_Generic (AverageExpressionNode.SupportedMethods, q => q.Average (i => (decimal?) 0.0m), e => e.Average (i => (decimal?) 0.0m));
    }

    [Test]
    public void SupportedMethod_WithDoubleSelector ()
    {
      AssertSupportedMethod_Generic (AverageExpressionNode.SupportedMethods, q => q.Average (i => 0.0), e => e.Average (i => 0.0));
    }

    [Test]
    public void SupportedMethod_WithNDoubleSelector ()
    {
      AssertSupportedMethod_Generic (AverageExpressionNode.SupportedMethods, q => q.Average (i => (double?) 0.0), e => e.Average (i => (double?) 0.0));
    }

    [Test]
    public void SupportedMethod_WithSingleSelector ()
    {
      AssertSupportedMethod_Generic (AverageExpressionNode.SupportedMethods, q => q.Average (i => 0.0f), e => e.Average (i => 0.0f));
    }

    [Test]
    public void SupportedMethod_WithNSingleSelector ()
    {
      AssertSupportedMethod_Generic (AverageExpressionNode.SupportedMethods, q => q.Average (i => (float?) 0.0f), e => e.Average (i => (float?) 0.0f));
    }

    [Test]
    public void SupportedMethod_WithInt32Selector ()
    {
      AssertSupportedMethod_Generic (AverageExpressionNode.SupportedMethods, q => q.Average (i => 0), e => e.Average (i => 0));
    }

    [Test]
    public void SupportedMethod_WithNInt32Selector ()
    {
      AssertSupportedMethod_Generic (AverageExpressionNode.SupportedMethods, q => q.Average (i => (int?) 0), e => e.Average (i => (int?) 0));
    }

    [Test]
    public void SupportedMethod_WithInt64Selector ()
    {
      AssertSupportedMethod_Generic (AverageExpressionNode.SupportedMethods, q => q.Average (i => 0L), e => e.Average (i => 0L));
    }

    [Test]
    public void SupportedMethod_WithNInt64Selector ()
    {
      AssertSupportedMethod_Generic (AverageExpressionNode.SupportedMethods, q => q.Average (i => (long?) 0L), e => e.Average (i => (long?) 0L));
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
      TestApply (_node, typeof (AverageResultOperator));
    }
  }
}

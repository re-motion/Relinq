// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// re-linq is free software; you can redistribute it and/or modify it under 
// the terms of the GNU Lesser General Public License as published by the 
// Free Software Foundation; either version 2.1 of the License, 
// or (at your option) any later version.
// 
// re-linq is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-linq; if not, see http://www.gnu.org/licenses.
// 
using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Parsing.Structure.IntermediateModel;

namespace Remotion.Linq.UnitTests.Linq.Core.Parsing.Structure.IntermediateModel
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

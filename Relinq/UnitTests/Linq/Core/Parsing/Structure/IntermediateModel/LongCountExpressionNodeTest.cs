// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (c) rubicon IT GmbH, www.rubicon.eu
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
using System.Linq;
using NUnit.Framework;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Parsing.Structure.IntermediateModel;

namespace Remotion.Linq.UnitTests.Linq.Core.Parsing.Structure.IntermediateModel
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
    public void SupportedMethod_WithoutPredicate ()
    {
      AssertSupportedMethod_Generic (LongCountExpressionNode.SupportedMethods, q => q.LongCount (), e => e.LongCount ());
    }

    [Test]
    public void SupportedMethods_WithoutPredicate_ForArrays ()
    {
      Assert.That (LongCountExpressionNode.SupportedMethods, Has.Member (typeof (Array).GetProperty ("LongLength").GetGetMethod ()));
    }

    [Test]
    public void SupportedMethod_WithPredicate ()
    {
      AssertSupportedMethod_Generic (LongCountExpressionNode.SupportedMethods, q => q.LongCount (o => o == null), e => e.LongCount (o => o == null));
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
      TestApply (_node, typeof (LongCountResultOperator));
    }
  }
}

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
using Remotion.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Parsing.Structure.IntermediateModel;

namespace Remotion.Linq.UnitTests.Linq.Core.Parsing.Structure.IntermediateModel
{
  [TestFixture]
  public class SingleExpressionNodeTest : ExpressionNodeTestBase
  {
    private SingleExpressionNode _node;

    [SetUp]
    public override void SetUp ()
    {
      base.SetUp ();

      _node = new SingleExpressionNode (CreateParseInfo (), null);
    }

    [Test]
    public void SupportedMethod_WithoutPredicate ()
    {
      AssertSupportedMethod_Generic (SingleExpressionNode.SupportedMethods, q => q.Single (), e => e.Single ());
    }

    [Test]
    public void SupportedMethod_WithPredicate ()
    {
      AssertSupportedMethod_Generic (SingleExpressionNode.SupportedMethods, q => q.Single (o => o == null), e => e.Single (o => o == null));
    }

    [Test]
    public void SupportedMethod_SingleOrDefault_WithoutPredicate ()
    {
      AssertSupportedMethod_Generic (SingleExpressionNode.SupportedMethods, q => q.SingleOrDefault (), e => e.SingleOrDefault ());
    }

    [Test]
    public void SupportedMethod_SingleOrDefault_WithPredicate ()
    {
      AssertSupportedMethod_Generic (SingleExpressionNode.SupportedMethods, q => q.SingleOrDefault (o => o == null), e => e.SingleOrDefault (o => o == null));
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
      TestApply (_node, typeof (SingleResultOperator));
    }

    [Test]
    public void Apply_NoDefaultAllowed ()
    {
      var node = new SingleExpressionNode (CreateParseInfo (SingleExpressionNode.SupportedMethods[0].MakeGenericMethod (typeof (Cook))), null);
      node.Apply (QueryModel, ClauseGenerationContext);

      Assert.That (((SingleResultOperator) QueryModel.ResultOperators[0]).ReturnDefaultWhenEmpty, Is.False);
    }

    [Test]
    public void Apply_DefaultAllowed ()
    {
      var node = new SingleExpressionNode (CreateParseInfo (SingleExpressionNode.SupportedMethods[3].MakeGenericMethod (typeof (Cook))), null);
      node.Apply (QueryModel, ClauseGenerationContext);

      Assert.That (((SingleResultOperator) QueryModel.ResultOperators[0]).ReturnDefaultWhenEmpty, Is.True);
    }
  }
}

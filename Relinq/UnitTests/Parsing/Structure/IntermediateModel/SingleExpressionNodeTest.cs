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
using Remotion.Linq.UnitTests.TestDomain;

namespace Remotion.Linq.UnitTests.Parsing.Structure.IntermediateModel
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
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = 
        "SingleExpressionNode does not support resolving of expressions, because it does not stream any data to the following node.")]
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

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
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Clauses.ResultOperators;
using Remotion.Data.Linq.Parsing.Structure.IntermediateModel;
using Remotion.Data.Linq.UnitTests.TestDomain;

namespace Remotion.Data.Linq.UnitTests.Parsing.Structure.IntermediateModel
{
  [TestFixture]
  public class LastExpressionNodeTest : ExpressionNodeTestBase
  {
    private LastExpressionNode _node;

    [SetUp]
    public override void SetUp ()
    {
      base.SetUp ();

      _node = new LastExpressionNode (CreateParseInfo (), null);
    }

    [Test]
    public void SupportedMethod_WithoutPredicate ()
    {
      AssertSupportedMethod_Generic (LastExpressionNode.SupportedMethods, q => q.Last (), e => e.Last ());
    }

    [Test]
    public void SupportedMethod_WithPredicate ()
    {
      AssertSupportedMethod_Generic (LastExpressionNode.SupportedMethods, q => q.Last (o => o == null), e => e.Last (o => o == null));
    }

    [Test]
    public void SupportedMethod_LastOrDefault_WithoutPredicate ()
    {
      AssertSupportedMethod_Generic (LastExpressionNode.SupportedMethods, q => q.LastOrDefault (), e => e.LastOrDefault ());
    }

    [Test]
    public void SupportedMethod_LastOrDefault_WithPredicate ()
    {
      AssertSupportedMethod_Generic (LastExpressionNode.SupportedMethods, q => q.LastOrDefault (o => o == null), e => e.LastOrDefault (o => o == null));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException))]
    public void Resolve_ThrowsInvalidOperationException ()
    {
      _node.Resolve (ExpressionHelper.CreateParameterExpression (), ExpressionHelper.CreateExpression (), ClauseGenerationContext);
    }

    [Test]
    public void Apply()
    {
      TestApply (_node, typeof (LastResultOperator));
    }

    [Test]
    public void Apply_NoDefaultAllowed ()
    {
      var node = new LastExpressionNode (CreateParseInfo (LastExpressionNode.SupportedMethods[0].MakeGenericMethod (typeof (Cook))), null);
      
      node.Apply (QueryModel, ClauseGenerationContext);
      
      Assert.That (((LastResultOperator) QueryModel.ResultOperators[0]).ReturnDefaultWhenEmpty, Is.False);
    }

    [Test]
    public void Apply_DefaultAllowed ()
    {
      var node = new LastExpressionNode (CreateParseInfo (LastExpressionNode.SupportedMethods[3].MakeGenericMethod (typeof (Cook))), null);
      
      node.Apply (QueryModel, ClauseGenerationContext);

      Assert.That (((LastResultOperator) QueryModel.ResultOperators[0]).ReturnDefaultWhenEmpty, Is.True);
    }
  }
}

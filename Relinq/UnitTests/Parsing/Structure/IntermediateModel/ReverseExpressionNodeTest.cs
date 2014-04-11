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
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.Parsing.Structure.IntermediateModel;
using Rhino.Mocks;

namespace Remotion.Linq.UnitTests.Parsing.Structure.IntermediateModel
{
  [TestFixture]
  public class ReverseExpressionNodeTest : ExpressionNodeTestBase
  {
    private ReverseExpressionNode _node;

    public override void SetUp ()
    {
      base.SetUp ();
      _node = new ReverseExpressionNode (CreateParseInfo ());
    }

    [Test]
    public void SupportedMethod_WithoutComparer ()
    {
      AssertSupportedMethod_Generic (ReverseExpressionNode.SupportedMethods, q => q.Reverse (), e => e.Reverse ());
    }

    [Test]
    public void Resolve_PassesExpressionToSource ()
    {
      var sourceMock = MockRepository.GenerateMock<IExpressionNode> ();
      var node = new ReverseExpressionNode (CreateParseInfo (sourceMock));
      var expression = ExpressionHelper.CreateLambdaExpression ();
      var parameter = ExpressionHelper.CreateParameterExpression ();
      var expectedResult = ExpressionHelper.CreateExpression ();
      sourceMock.Expect (mock => mock.Resolve (parameter, expression, ClauseGenerationContext)).Return (expectedResult);

      var result = node.Resolve (parameter, expression, ClauseGenerationContext);

      sourceMock.VerifyAllExpectations ();
      Assert.That (result, Is.SameAs (expectedResult));
    }

    [Test]
    public void Apply ()
    {
      TestApply (_node, typeof (ReverseResultOperator));
    }
  }
}

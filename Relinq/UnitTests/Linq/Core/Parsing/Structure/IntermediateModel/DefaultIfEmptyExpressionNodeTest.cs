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
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Parsing.Structure.IntermediateModel;
using Rhino.Mocks;

namespace Remotion.Linq.UnitTests.Linq.Core.Parsing.Structure.IntermediateModel
{
  [TestFixture]
  public class DefaultIfEmptyExpressionNodeTest : ExpressionNodeTestBase
  {
    private DefaultIfEmptyExpressionNode _nodeWithDefaultValue;
    private DefaultIfEmptyExpressionNode _nodeWithoutDefaultValue;
    private ConstantExpression _defaultValue;

    public override void SetUp ()
    {
      base.SetUp ();
      _defaultValue = Expression.Constant (100);
      _nodeWithDefaultValue = new DefaultIfEmptyExpressionNode (CreateParseInfo (), _defaultValue);
      _nodeWithoutDefaultValue = new DefaultIfEmptyExpressionNode (CreateParseInfo (), null);
    }

    [Test]
    public void SupportedMethod_NoDefaultValue ()
    {
      AssertSupportedMethod_Generic (DefaultIfEmptyExpressionNode.SupportedMethods,  q => q.DefaultIfEmpty (), e => e.DefaultIfEmpty ());
    }

    [Test]
    public void SupportedMethod_WithDefaultValue ()
    {
      AssertSupportedMethod_Generic (DefaultIfEmptyExpressionNode.SupportedMethods, q => q.DefaultIfEmpty (null), e => e.DefaultIfEmpty (null));
    }

    [Test]
    public void Resolve_PassesExpressionToSource ()
    {
      var sourceMock = MockRepository.GenerateMock<IExpressionNode> ();
      var node = new DefaultIfEmptyExpressionNode (CreateParseInfo (sourceMock), null);
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
      TestApply (_nodeWithDefaultValue, typeof (DefaultIfEmptyResultOperator));
    }

    [Test]
    public void Apply_WithDefaultValue ()
    {
      _nodeWithDefaultValue.Apply (QueryModel, ClauseGenerationContext);
      var resultOperator = ((DefaultIfEmptyResultOperator) QueryModel.ResultOperators[0]);
      Assert.That (resultOperator.OptionalDefaultValue, Is.SameAs (_defaultValue));
    }

    [Test]
    public void Apply_WithoutDefaultValue ()
    {
      _nodeWithoutDefaultValue.Apply (QueryModel, ClauseGenerationContext);
      var resultOperator = ((DefaultIfEmptyResultOperator) QueryModel.ResultOperators[0]);
      Assert.That (resultOperator.OptionalDefaultValue, Is.Null);
    }
  }
}

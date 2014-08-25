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
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.Parsing.Structure.IntermediateModel;
using Rhino.Mocks;

namespace Remotion.Linq.UnitTests.Parsing.Structure.IntermediateModel
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

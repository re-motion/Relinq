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

namespace Remotion.Linq.UnitTests.Parsing.Structure.IntermediateModel
{
  [TestFixture]
  public class AggregateFromSeedExpressionNodeTest : ExpressionNodeTestBase
  {
    private ConstantExpression _seed;
    private Expression<Func<int, int, int>> _func;
    private Expression<Func<int, string>> _resultSelector;

    private AggregateFromSeedExpressionNode _nodeWithoutResultSelector;
    private AggregateFromSeedExpressionNode _nodeWithResultSelector;

    public override void SetUp ()
    {
      base.SetUp ();
      _seed = Expression.Constant (0);
      _func = ExpressionHelper.CreateLambdaExpression<int, int, int> ((total, i) => total + i);
      _resultSelector = ExpressionHelper.CreateLambdaExpression<int, string> (total => total.ToString());

      _nodeWithoutResultSelector = new AggregateFromSeedExpressionNode (CreateParseInfo (), _seed, _func, null);
      _nodeWithResultSelector = new AggregateFromSeedExpressionNode (CreateParseInfo (), _seed, _func, _resultSelector);
    }

    [Test]
    public void SupportedMethods_WithoutResultSelector ()
    {
      AssertSupportedMethod_Generic (
          AggregateFromSeedExpressionNode.SupportedMethods,
          q => q.Aggregate<object, object> (null, (i, j) => null),
          e => e.Aggregate<object, object> (null, (i, j) => null));
    }

    [Test]
    public void SupportedMethods_WithResultSelector ()
    {
      AssertSupportedMethod_Generic (
          AggregateFromSeedExpressionNode.SupportedMethods,
          q => q.Aggregate<object, object, object> (null, (i, j) => null, o => null),
          e => e.Aggregate<object, object, object> (null, (i, j) => null, o => null));
    }

    [Test]
    public void Initialization_WithoutResultSelector ()
    {
      var parseInfo = CreateParseInfo();
      var node = new AggregateFromSeedExpressionNode (parseInfo, _seed, _func, null);

      Assert.That (node.Source, Is.SameAs (SourceNode));
      Assert.That (node.Seed, Is.SameAs (_seed));
      Assert.That (node.Func, Is.SameAs (_func));
      Assert.That (node.OptionalResultSelector, Is.Null);
    }

    [Test]
    public void Initialization_WithResultSelector ()
    {
      var parseInfo = CreateParseInfo ();
      var node = new AggregateFromSeedExpressionNode (parseInfo, _seed, _func, _resultSelector);

      Assert.That (node.Source, Is.SameAs (SourceNode));
      Assert.That (node.Seed, Is.SameAs (_seed));
      Assert.That (node.Func, Is.SameAs (_func));
      Assert.That (node.OptionalResultSelector, Is.SameAs (_resultSelector));
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = "Func must have exactly two parameters.\r\nParameter name: func")]
    public void Initialization_WrongNumberOfParametersInFunc ()
    {
      var parseInfo = CreateParseInfo ();
      new AggregateFromSeedExpressionNode (parseInfo, _seed, ExpressionHelper.CreateLambdaExpression<int, bool> (i => false), _resultSelector);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = 
        "Result selector must have exactly one parameter.\r\nParameter name: optionalResultSelector")]
    public void Initialization_WrongNumberOfParametersInResultSelector ()
    {
      var parseInfo = CreateParseInfo ();
      new AggregateFromSeedExpressionNode (parseInfo, _seed, _func, ExpressionHelper.CreateLambdaExpression<int, int, bool> ((i, j) => false));
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = 
        "AggregateFromSeedExpressionNode does not support resolving of expressions, because it does not stream any data to the following node.")]
    public void Resolve_ThrowsInvalidOperationException ()
    {
      _nodeWithResultSelector.Resolve (ExpressionHelper.CreateParameterExpression (), ExpressionHelper.CreateExpression (), ClauseGenerationContext);
    }

    [Test]
    public void GetResolvedFunc ()
    {
      var expectedResult = Expression.Lambda (Expression.Add (_func.Parameters[0], SourceReference), _func.Parameters[0]);

      var result = _nodeWithResultSelector.GetResolvedFunc (ClauseGenerationContext);

      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void Apply_WithoutResultSelector ()
    {
      var result = _nodeWithoutResultSelector.Apply (QueryModel, ClauseGenerationContext);
      Assert.That (result, Is.SameAs (QueryModel));

      var resultOperator = (AggregateFromSeedResultOperator) QueryModel.ResultOperators[0];
      Assert.That (resultOperator.Seed, Is.SameAs (_seed));
      Assert.That (resultOperator.Func, Is.EqualTo (_nodeWithoutResultSelector.GetResolvedFunc (ClauseGenerationContext)));
      Assert.That (resultOperator.OptionalResultSelector, Is.Null);
    }

    [Test]
    public void Apply_WithResultSelector ()
    {
      var result = _nodeWithResultSelector.Apply (QueryModel, ClauseGenerationContext);
      Assert.That (result, Is.SameAs (QueryModel));

      var resultOperator = (AggregateFromSeedResultOperator) QueryModel.ResultOperators[0];
      Assert.That (resultOperator.Seed, Is.SameAs (_seed));
      Assert.That (resultOperator.Func, Is.EqualTo (_nodeWithResultSelector.GetResolvedFunc (ClauseGenerationContext)));
      Assert.That (resultOperator.OptionalResultSelector, Is.SameAs (_nodeWithResultSelector.OptionalResultSelector));
    }
  }
}

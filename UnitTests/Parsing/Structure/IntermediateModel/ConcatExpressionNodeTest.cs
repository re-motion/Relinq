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
  public class ConcatExpressionNodeTest : ExpressionNodeTestBase
  {
    private ConcatExpressionNode _node;
    private Expression _source2;

    public override void SetUp ()
    {
      base.SetUp ();
      _source2 = Expression.Constant (new[] { "test1", "test2" });
      _node = new ConcatExpressionNode (
          CreateParseInfo (SourceNode, "u", ConcatExpressionNode.SupportedMethods[0].MakeGenericMethod (typeof (int))), _source2);
    }

    [Test]
    public void SupportedMethod_WithoutComparer ()
    {
      AssertSupportedMethod_Generic (ConcatExpressionNode.SupportedMethods, q => q.Concat (null), e => e.Concat (null));
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_node.ItemType, Is.SameAs (typeof (int)));
    }

    [Test]
    public void Resolve ()
    {
      var querySource = ExpressionHelper.CreateConcatResultOperator ();
      ClauseGenerationContext.AddContextInfo (_node, querySource);

      var lambdaExpression = ExpressionHelper.CreateLambdaExpression<int, string> (u => u.ToString());

      var result = _node.Resolve (lambdaExpression.Parameters[0], lambdaExpression.Body, ClauseGenerationContext);

      var expectedResult = ExpressionHelper.Resolve<int, string> (querySource, u => u.ToString());
      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void Apply ()
    {
      var result = _node.Apply (QueryModel, ClauseGenerationContext);
      Assert.That (result, Is.SameAs (QueryModel));

      Assert.That (((ConcatResultOperator) QueryModel.ResultOperators[0]).ItemName, Is.EqualTo ("u"));
      Assert.That (((ConcatResultOperator) QueryModel.ResultOperators[0]).ItemType, Is.SameAs (typeof (int)));
      Assert.That (((ConcatResultOperator) QueryModel.ResultOperators[0]).Source2, Is.SameAs (_source2));
    }

    [Test]
    public void Apply_AddsMapping ()
    {
      _node.Apply (QueryModel, ClauseGenerationContext);

      var resultOperator = (ConcatResultOperator) QueryModel.ResultOperators[0];
      Assert.That (ClauseGenerationContext.GetContextInfo (_node), Is.SameAs (resultOperator));
    }
  }
}

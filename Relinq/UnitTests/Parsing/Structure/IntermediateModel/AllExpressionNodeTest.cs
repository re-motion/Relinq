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
  public class AllExpressionNodeTest : ExpressionNodeTestBase
  {
    private Expression<Func<int, bool>> _predicate;
    private AllExpressionNode _node;

    public override void SetUp ()
    {
      base.SetUp ();

      _predicate = ExpressionHelper.CreateLambdaExpression<int, bool> (i => i > 2);
      _node = new AllExpressionNode (CreateParseInfo (), _predicate);
    }

    [Test]
    public void SupportedMethods ()
    {
      AssertSupportedMethod_Generic (AllExpressionNode.SupportedMethods, q => q.All (o => o == null), e => e.All (o => o == null));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException))]
    public void Resolve_ThrowsInvalidOperationException ()
    {
      _node.Resolve (ExpressionHelper.CreateParameterExpression (), ExpressionHelper.CreateExpression (), ClauseGenerationContext);
    }

    [Test]
    public void GetResolvedPredicate ()
    {
      var predicate = ExpressionHelper.CreateLambdaExpression<int, bool> (i => i > 5);
      var node = new AllExpressionNode (CreateParseInfo (), predicate);

      var expectedResult = Expression.MakeBinary (ExpressionType.GreaterThan, SourceReference, Expression.Constant (5));

      var result = node.GetResolvedPredicate (ClauseGenerationContext);

      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void Apply ()
    {
      var result = _node.Apply (QueryModel, ClauseGenerationContext);
      Assert.That (result, Is.SameAs (QueryModel));

      Assert.That (QueryModel.ResultOperators.Count, Is.EqualTo (1));
      Assert.That (QueryModel.ResultOperators[0], Is.InstanceOf (typeof (AllResultOperator)));

      var resultOperator = ((AllResultOperator) QueryModel.ResultOperators[0]);
      Assert.That (resultOperator.Predicate, Is.EqualTo (_node.GetResolvedPredicate (ClauseGenerationContext)));
    }
  }
}

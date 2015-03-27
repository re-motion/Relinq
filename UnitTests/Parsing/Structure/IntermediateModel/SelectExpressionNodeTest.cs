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
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.Parsing.Structure.IntermediateModel;

namespace Remotion.Linq.UnitTests.Parsing.Structure.IntermediateModel
{
  [TestFixture]
  public class SelectExpressionNodeTest : ExpressionNodeTestBase
  {
    private SelectExpressionNode _node;

    public override void SetUp ()
    {
      base.SetUp ();

      var selector = ExpressionHelper.CreateLambdaExpression<int, bool> (i => i > 5);
      _node = new SelectExpressionNode (CreateParseInfo (), selector);
    }


    [Test]
    public void GetSupportedMethods ()
    {
      Assert.That (
          SelectExpressionNode.GetSupportedMethods(),
          Is.EquivalentTo (
              new[]
              {
                  GetGenericMethodDefinition (() => Queryable.Select<object, object> (null, o => null)),
                  GetGenericMethodDefinition (() => Enumerable.Select<object, object> (null, o => null))
              }));
    }

    [Test]
    public void Resolve_ReplacesParameter_WithProjection ()
    {
      var node = new SelectExpressionNode (CreateParseInfo (), ExpressionHelper.CreateLambdaExpression<int, int> (j => j * j));
      var expression = ExpressionHelper.CreateLambdaExpression<int, bool> (i => i > 5);

      var result = node.Resolve (expression.Parameters[0], expression.Body, ClauseGenerationContext);

      var expectedResult = Expression.MakeBinary (
          ExpressionType.GreaterThan,
          Expression.MakeBinary (ExpressionType.Multiply, SourceReference, SourceReference),
          Expression.Constant (5));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void GetResolvedSelector ()
    {
      var selector = ExpressionHelper.CreateLambdaExpression<int, bool> (i => i > 5);
      var node = new SelectExpressionNode (CreateParseInfo (), selector);

      var expectedResult = Expression.MakeBinary (ExpressionType.GreaterThan, SourceReference, Expression.Constant (5));

      var result = node.GetResolvedSelector(ClauseGenerationContext);

      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void Apply ()
    {
      var result = _node.Apply (QueryModel, ClauseGenerationContext);
      Assert.That (result, Is.SameAs (QueryModel));

      var selectClause = QueryModel.SelectClause;
      
      Assert.That (selectClause.Selector, Is.EqualTo (_node.GetResolvedSelector (ClauseGenerationContext)));
    }
  }
}

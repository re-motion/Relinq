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
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Parsing.Structure.IntermediateModel;

namespace Remotion.Linq.UnitTests.Linq.Core.Parsing.Structure.IntermediateModel
{
  [TestFixture]
  public class UnionExpressionNodeTest : ExpressionNodeTestBase
  {
    private UnionExpressionNode _node;
    private Expression _source2;

    public override void SetUp ()
    {
      base.SetUp ();
      _source2 = Expression.Constant (new[] { "test1", "test2" });
      _node = new UnionExpressionNode (
          CreateParseInfo (SourceNode, "u", UnionExpressionNode.SupportedMethods[0].MakeGenericMethod (typeof (int))), _source2);
    }

    [Test]
    public void SupportedMethod_WithoutComparer ()
    {
      AssertSupportedMethod_Generic (UnionExpressionNode.SupportedMethods, q => q.Union (null), e => e.Union (null));
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_node.ItemType, Is.SameAs (typeof (int)));
    }

    [Test]
    public void Resolve ()
    {
      var querySource = ExpressionHelper.CreateUnionResultOperator ();
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

      Assert.That (((UnionResultOperator) QueryModel.ResultOperators[0]).ItemName, Is.EqualTo ("u"));
      Assert.That (((UnionResultOperator) QueryModel.ResultOperators[0]).ItemType, Is.SameAs (typeof (int)));
      Assert.That (((UnionResultOperator) QueryModel.ResultOperators[0]).Source2, Is.SameAs (_source2));
    }

    [Test]
    public void Apply_AddsMapping ()
    {
      _node.Apply (QueryModel, ClauseGenerationContext);

      var resultOperator = (UnionResultOperator) QueryModel.ResultOperators[0];
      Assert.That (ClauseGenerationContext.GetContextInfo (_node), Is.SameAs (resultOperator));
    }
  }
}

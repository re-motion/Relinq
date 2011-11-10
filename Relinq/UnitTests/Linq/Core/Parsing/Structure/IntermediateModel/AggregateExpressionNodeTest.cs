// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (C) rubicon IT GmbH, www.rubicon.eu
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
  public class AggregateExpressionNodeTest : ExpressionNodeTestBase
  {
    private Expression<Func<int, int, int>> _func;
    private AggregateExpressionNode _node;

    public override void SetUp ()
    {
      base.SetUp ();
      _func = ExpressionHelper.CreateLambdaExpression<int, int, int> ((total, i) => total + i);
      _node = new AggregateExpressionNode (CreateParseInfo (), _func);
    }

    [Test]
    public void SupportedMethods ()
    {
      AssertSupportedMethod_Generic (AggregateExpressionNode.SupportedMethods, q => q.Aggregate ((i, j) => null), e => e.Aggregate ((i, j) => null));
    }

    [Test]
    public void Initialization ()
    {
      var parseInfo = CreateParseInfo();
      var node = new AggregateExpressionNode (parseInfo, _func);

      Assert.That (node.Source, Is.SameAs (SourceNode));
      Assert.That (node.Func, Is.SameAs (_func));
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = "Func must have exactly two parameters.\r\nParameter name: func")]
    public void Initialization_WrongNumberOfParameters ()
    {
      var parseInfo = CreateParseInfo ();
      new AggregateExpressionNode (parseInfo, ExpressionHelper.CreateLambdaExpression<int, bool> (i => false));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException))]
    public void Resolve_ThrowsInvalidOperationException ()
    {
      _node.Resolve (ExpressionHelper.CreateParameterExpression (), ExpressionHelper.CreateExpression (), ClauseGenerationContext);
    }

    [Test]
    public void GetResolvedFunc ()
    {
      var expectedResult = Expression.Lambda (Expression.Add (_func.Parameters[0], SourceReference), _func.Parameters[0]);

      var result = _node.GetResolvedFunc (ClauseGenerationContext);

      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void Apply ()
    {
      var result = _node.Apply (QueryModel, ClauseGenerationContext);
      Assert.That (result, Is.SameAs (QueryModel));

      var resultOperator = (AggregateResultOperator) QueryModel.ResultOperators[0];
      Assert.That (resultOperator.Func, Is.EqualTo (_node.GetResolvedFunc (ClauseGenerationContext)));
    }
  }
}

// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// version 3.0 as published by the Free Software Foundation.
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
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Parsing.Structure;
using Remotion.Data.Linq.Parsing.Structure.IntermediateModel;
using Rhino.Mocks;

namespace Remotion.Data.UnitTests.Linq.Parsing.Structure.IntermediateModel
{
  [TestFixture]
  public class NodeExpressionResolverTest : ExpressionNodeTestBase
  {
    private NodeExpressionResolver _nodeExpressionResolver;
    private Expression<Func<int, bool>> _unresolvedLambda;

    public override void SetUp ()
    {
      base.SetUp();
      _unresolvedLambda = ExpressionHelper.CreateLambdaExpression<int, bool> (i => i > 5);
      _nodeExpressionResolver = new NodeExpressionResolver (SourceNode);
    }

    [Test]
    public void GetResolvedExpression ()
    {
      var expectedResult = Expression.MakeBinary (ExpressionType.GreaterThan, SourceReference, Expression.Constant (5));
      var result = _nodeExpressionResolver.GetResolvedExpression (_unresolvedLambda.Body, _unresolvedLambda.Parameters[0], QuerySourceClauseMapping);

      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void GetResolvedExpression_Lazy ()
    {
      _nodeExpressionResolver.GetResolvedExpression (() => _unresolvedLambda.Body, _unresolvedLambda.Parameters[0], QuerySourceClauseMapping);

      bool lambdaCalled = false;
      _nodeExpressionResolver.GetResolvedExpression (
          () =>
          {
            lambdaCalled = true;
            return _unresolvedLambda.Body;
          },
          _unresolvedLambda.Parameters[0],
          QuerySourceClauseMapping);

      Assert.That (lambdaCalled, Is.False);
    }

    [Test]
    public void GetResolvedExpression_RemovesTransparentIdentifiers ()
    {
      var expectedResult = Expression.MakeBinary (ExpressionType.GreaterThan, SourceReference, Expression.Constant (5));
      var result = _nodeExpressionResolver.GetResolvedExpression (_unresolvedLambda.Body, _unresolvedLambda.Parameters[0], QuerySourceClauseMapping);

      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void GetResolvedExpression_Cached ()
    {
      var sourceMock = new MockRepository().StrictMock<IExpressionNode>();
      var cache = new NodeExpressionResolver (sourceMock);
      var fakeResult = ExpressionHelper.CreateLambdaExpression();

      sourceMock
          .Expect (
          mock => mock.Resolve (Arg<ParameterExpression>.Is.Anything, Arg<Expression>.Is.Anything, Arg<QuerySourceClauseMapping>.Is.Anything))
          .Repeat.Once()
          .Return (fakeResult);

      sourceMock.Replay();

      cache.GetResolvedExpression (_unresolvedLambda, _unresolvedLambda.Parameters[0], QuerySourceClauseMapping);
      cache.GetResolvedExpression (_unresolvedLambda, _unresolvedLambda.Parameters[0], QuerySourceClauseMapping);

      sourceMock.VerifyAllExpectations();
    }
  }
}
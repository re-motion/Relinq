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
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.ResultModifications;
using Remotion.Data.Linq.Parsing.Structure.IntermediateModel;
using System.Linq;
using Rhino.Mocks;

namespace Remotion.Data.UnitTests.Linq.Parsing.Structure.IntermediateModel
{
  [TestFixture]
  public class CountExpressionNodeTest : ExpressionNodeTestBase
  {
    [Test]
    public void SupportedMethod_WithoutPredicate ()
    {
      MethodInfo method = GetGenericMethodDefinition (q => q.Count());
      Assert.That (CountExpressionNode.SupportedMethods, List.Contains (method));
    }

    [Test]
    public void SupportedMethod_WithPredicate ()
    {
      var method = GetGenericMethodDefinition (q => q.Count (i => i > 5));
      Assert.That (CountExpressionNode.SupportedMethods, List.Contains (method));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException))]
    public void Resolve_ThrowsInvalidOperationException ()
    {
      var node = new CountExpressionNode (SourceStub, null);
      node.Resolve (ExpressionHelper.CreateParameterExpression(), ExpressionHelper.CreateExpression());
    }

    [Test]
    public void GetResolvedPredicate ()
    {
      var predicate = ExpressionHelper.CreateLambdaExpression<int, bool> (i => i > 5);
      var node = new CountExpressionNode (SourceStub, predicate);
      var expectedResult = Expression.MakeBinary (ExpressionType.GreaterThan, SourceReference, Expression.Constant (5));

      var result = node.GetResolvedOptionalPredicate ();

      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void GetResolvedPredicate_Null ()
    {
      var sourceMock = MockRepository.GenerateMock<IExpressionNode> ();
      var node = new CountExpressionNode (sourceMock, null);
      var result = node.GetResolvedOptionalPredicate ();
      Assert.That (result, Is.Null);
    }

    [Test]
    public void GetResolvedPredicate_Cached ()
    {
      var sourceMock = new MockRepository().StrictMock<IExpressionNode>();
      var predicate = ExpressionHelper.CreateLambdaExpression<int, bool> (i => i > 5);
      var node = new CountExpressionNode (sourceMock, predicate);
      var expectedResult = ExpressionHelper.CreateLambdaExpression ();

      sourceMock.Expect (mock => mock.Resolve (Arg<ParameterExpression>.Is.Anything, Arg<Expression>.Is.Anything)).Repeat.Once().Return (expectedResult);

      sourceMock.Replay ();
      
      node.GetResolvedOptionalPredicate ();     
      node.GetResolvedOptionalPredicate();

      sourceMock.VerifyAllExpectations ();
    }

    [Test]
    public void CreateClause_WithoutOptionalPredicate_PreviousClauseIsSelect ()
    {
      var previousClause = ExpressionHelper.CreateSelectClause();
      var node = new CountExpressionNode (SourceStub, null);
      
      var clause = (SelectClause)node.CreateClause(previousClause);

      Assert.That (clause, Is.SameAs (previousClause));
      Assert.That (clause.ResultModifications.Count, Is.EqualTo (1));
      Assert.That (clause.ResultModifications[0], Is.InstanceOfType (typeof (CountResultModification)));
      Assert.That (clause.ResultModifications[0].SelectClause, Is.SameAs (clause));
    }

    [Ignore("TODO: 1184")]
    [Test]
    public void CreateClause_WithoutOptionalPredicate_PreviousClauseIsNoSelect ()
    {
      var source = new ConstantExpressionNode (typeof (int[]), new int[] { 1, 2, 3 }, "i1");
      var node = new CountExpressionNode (source, null);
      var previousClause = ExpressionHelper.CreateMainFromClause ();

      var clause = (SelectClause) node.CreateClause (previousClause);

      Assert.That (clause.PreviousClause, Is.SameAs (previousClause));
      Assert.That (clause.ResultModifications.Count, Is.EqualTo (1));
      Assert.That (clause.ResultModifications[0], Is.InstanceOfType (typeof (CountResultModification)));
      Assert.That (clause.ResultModifications[0].SelectClause, Is.SameAs (clause));
      var expectedSelector = ExpressionHelper.CreateLambdaExpression<int, int> (i1 => i1);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedSelector, clause.Selector);
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException))]
    public void CreateParameterForOutput ()
    {
      var node = new CountExpressionNode (SourceStub, null);
      node.CreateParameterForOutput ();
    }
  }
}
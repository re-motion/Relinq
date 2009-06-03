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
  public class MaxExpressionNodeTest : ExpressionNodeTestBase
  {
    [Test]
    public void SupportedMethod_WithoutSelector ()
    {
      MethodInfo method = GetGenericMethodDefinition (q => q.Max());
      Assert.That (MaxExpressionNode.SupportedMethods, List.Contains (method));
    }

    [Test]
    public void SupportedMethod_WithSelector ()
    {
      MethodInfo method = GetGenericMethodDefinition (q => q.Max (i => i.ToString()));
      Assert.That (MaxExpressionNode.SupportedMethods, List.Contains (method));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException))]
    public void Resolve_ThrowsInvalidOperationException ()
    {
      var node = new MaxExpressionNode (SourceStub, null);
      node.Resolve (ExpressionHelper.CreateParameterExpression (), ExpressionHelper.CreateExpression ());
    }

    [Test]
    public void GetResolvedSelector ()
    {
      var selector = ExpressionHelper.CreateLambdaExpression<int, bool> (i => i > 5);
      var node = new MaxExpressionNode (SourceStub, selector);

      var expectedResult = Expression.MakeBinary (ExpressionType.GreaterThan, SourceReference, Expression.Constant (5));
      
      var result = node.GetResolvedOptionalSelector ();

      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void GetResolvedSelector_Null ()
    {
      var sourceMock = MockRepository.GenerateMock<IExpressionNode> ();
      var node = new MaxExpressionNode (sourceMock, null);
      var result = node.GetResolvedOptionalSelector ();
      Assert.That (result, Is.Null);
    }

    [Test]
    public void GetResolvedOptionalSelector_Cached ()
    {
      var sourceMock = new MockRepository ().StrictMock<IExpressionNode> ();
      var selector = ExpressionHelper.CreateLambdaExpression<int, bool> (i => i > 5);
      var node = new MaxExpressionNode (sourceMock, selector);
      var expectedResult = ExpressionHelper.CreateLambdaExpression ();
      
      sourceMock.Expect (mock => mock.Resolve (Arg<ParameterExpression>.Is.Anything, Arg<Expression>.Is.Anything)).Repeat.Once ().Return (expectedResult);

      sourceMock.Replay();

      node.GetResolvedOptionalSelector ();
      node.GetResolvedOptionalSelector ();

      sourceMock.VerifyAllExpectations ();
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException))]
    public void CreateParameterForOutput ()
    {
      var node = new MaxExpressionNode (SourceStub, null);
      node.CreateParameterForOutput ();
    }

    [Test]
    public void CreateClause_WithoutSelector_PreviousClauseIsSelect ()
    {
      var node = new MaxExpressionNode (SourceStub, null);
      TestCreateClause_PreviousClauseIsSelect(node, typeof (MaxResultModification));
    }

    [Test]
    public void CreateClause_WithoutSelector_PreviousClauseIsNoSelect ()
    {
      var node = new MaxExpressionNode (SourceStub, null);
      TestCreateClause_PreviousClauseIsNoSelect (node, typeof (MaxResultModification));
    }

    [Test]
    public void CreateClause_WithSelector_AdjustsSelectClause ()
    {
      var previousPreviousClause = ExpressionHelper.CreateClause ();
      var selectorOfPreviousClause = ExpressionHelper.CreateLambdaExpression<Student, int> (s => s.ID);
      var previousClause = new SelectClause (previousPreviousClause, selectorOfPreviousClause);

      var selector = ExpressionHelper.CreateLambdaExpression<int, string> (i => i.ToString ());
      var node = new MaxExpressionNode (SourceStub, selector);

      var clause = (SelectClause) node.CreateClause (previousClause);

      Assert.That (clause, Is.SameAs (previousClause));
      Assert.That (clause.Selector, Is.Not.SameAs (selectorOfPreviousClause));

      var expectedNewSelector = ExpressionHelper.CreateLambdaExpression<Student, string> (s => s.ID.ToString ());
      ExpressionTreeComparer.CheckAreEqualTrees (expectedNewSelector, clause.Selector);
    }
  }
}
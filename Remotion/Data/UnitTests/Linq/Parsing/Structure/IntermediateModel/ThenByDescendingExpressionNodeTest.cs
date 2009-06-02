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
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Parsing.Structure.IntermediateModel;
using System.Linq;
using Remotion.Utilities;
using Rhino.Mocks;

namespace Remotion.Data.UnitTests.Linq.Parsing.Structure.IntermediateModel
{
  [TestFixture]
  public class ThenByDescendingExpressionNodeTest : ExpressionNodeTestBase
  {
    [Test]
    public void SupportedMethod_WithoutComparer ()
    {
      MethodInfo method = GetGenericMethodDefinition (q => ((IOrderedQueryable<object>) q).ThenByDescending(i => i));
      Assert.That (ThenByDescendingExpressionNode.SupportedMethods, List.Contains (method));
    }

    [Test]
    public void Resolve_PassesExpressionToSource ()
    {
      var sourceMock = MockRepository.GenerateMock<IExpressionNode>();
      var selector = ExpressionHelper.CreateLambdaExpression<int, int> (i => i);
      var node = new ThenByDescendingExpressionNode (sourceMock, selector);
      var expression = ExpressionHelper.CreateLambdaExpression();
      var parameter = ExpressionHelper.CreateParameterExpression();
      var expectedResult = ExpressionHelper.CreateExpression();
      sourceMock.Expect (mock => mock.Resolve (parameter, expression)).Return (expectedResult);
      
      var result = node.Resolve (parameter, expression);

      sourceMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (expectedResult));
    }

    [Test]
    public void GetResolvedSelector ()
    {
      var selector = ExpressionHelper.CreateLambdaExpression<int, bool> (i => i > 5);
      var node = new ThenByDescendingExpressionNode (SourceStub, selector);

      var expectedResult = Expression.MakeBinary (ExpressionType.GreaterThan, SourceReference, Expression.Constant (5));

      var result = node.GetResolvedKeySelector ();

      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void GetResolvedSelector_Cached ()
    {
      var sourceMock = new MockRepository ().StrictMock<IExpressionNode> ();
      var selector = ExpressionHelper.CreateLambdaExpression<int, bool> (i => i > 5);
      var node = new ThenByDescendingExpressionNode (sourceMock, selector);
      var expectedResult = ExpressionHelper.CreateLambdaExpression ();

      sourceMock.Expect (mock => mock.Resolve (Arg<ParameterExpression>.Is.Anything, Arg<Expression>.Is.Anything)).Repeat.Once ().Return (expectedResult);

      sourceMock.Replay ();

      node.GetResolvedKeySelector ();
      node.GetResolvedKeySelector ();

      sourceMock.VerifyAllExpectations ();
    }

    [Test]
    public void CreateClause ()
    {
      var previousClause = ExpressionHelper.CreateOrderByClause ();
      var selector = ExpressionHelper.CreateLambdaExpression<int, bool> (i => i > 5);
      var node = new ThenByDescendingExpressionNode (SourceStub, selector);
      var oldCount = previousClause.OrderingList.Count;

      var clause = (OrderByClause) node.CreateClause (previousClause);

      Assert.That (clause, Is.SameAs (previousClause));
      Assert.That (clause.OrderingList.Count, Is.EqualTo (oldCount + 1));
      Assert.That (clause.OrderingList.Last ().OrderingDirection, Is.EqualTo (OrderingDirection.Desc));
      Assert.That (clause.OrderingList.Last ().Expression, Is.SameAs (node.KeySelector));
      Assert.That (clause.OrderingList.Last ().OrderByClause, Is.SameAs (previousClause));
    }

    [Test]
    [ExpectedException (typeof (ArgumentTypeException))]
    public void CreateClause_InvalidPreviousClause ()
    {
      var previousClause = ExpressionHelper.CreateMainFromClause ();
      var selector = ExpressionHelper.CreateLambdaExpression<int, bool> (i => i > 5);
      var node = new ThenByDescendingExpressionNode (SourceStub, selector);

      node.CreateClause (previousClause);
    }
  }
}
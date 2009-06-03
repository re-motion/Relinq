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
using Remotion.Data.Linq.Parsing.Structure.IntermediateModel;
using System.Linq;
using Rhino.Mocks;

namespace Remotion.Data.UnitTests.Linq.Parsing.Structure.IntermediateModel
{
  [TestFixture]
  public class WhereExpressionNodeTest : ExpressionNodeTestBase
  {
    [Test]
    public void SupportedMethod_WithoutPosition ()
    {
      MethodInfo method = GetGenericMethodDefinition (q => q.Where (i => i > 5));
      Assert.That (WhereExpressionNode.SupportedMethods, List.Contains (method));
    }

    [Test]
    public void Resolve_PassesExpressionToSource ()
    {
      var sourceMock = MockRepository.GenerateMock<IExpressionNode>();
      var predicate = ExpressionHelper.CreateLambdaExpression<int, bool> (i => i > 5);
      var node = new WhereExpressionNode (sourceMock, predicate);
      var expression = ExpressionHelper.CreateLambdaExpression();
      var parameter = ExpressionHelper.CreateParameterExpression();
      var expectedResult = ExpressionHelper.CreateExpression();
      sourceMock.Expect (mock => mock.Resolve (parameter, expression)).Return (expectedResult);
      
      var result = node.Resolve (parameter, expression);

      sourceMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (expectedResult));
    }

    [Test]
    public void GetResolvedPredicate ()
    {
      var predicate = ExpressionHelper.CreateLambdaExpression<int, bool> (i => i > 5);
      var node = new WhereExpressionNode (SourceStub, predicate);

      var expectedResult = Expression.MakeBinary (ExpressionType.GreaterThan, SourceReference, Expression.Constant (5));

      var result = node.GetResolvedPredicate ();

      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void GetResolvedPredicate_Cached ()
    {
      var sourceMock = new MockRepository ().StrictMock<IExpressionNode> ();
      var predicate = ExpressionHelper.CreateLambdaExpression<int, bool> (i => i > 5);
      var node = new WhereExpressionNode (sourceMock, predicate);
      var expectedResult = ExpressionHelper.CreateLambdaExpression ();

      sourceMock.Expect (mock => mock.Resolve (Arg<ParameterExpression>.Is.Anything, Arg<Expression>.Is.Anything)).Repeat.Once ().Return (expectedResult);

      sourceMock.Replay ();

      node.GetResolvedPredicate ();
      node.GetResolvedPredicate ();

      sourceMock.VerifyAllExpectations ();
    }

    [Test]
    public void CreateParameterForOutput ()
    {
      var source = new ConstantExpressionNode (typeof (int[]), new[] { 1, 2, 3, 4, 5 }, "x");
      var node = new WhereExpressionNode (source, ExpressionHelper.CreateLambdaExpression<int, bool> (y => y > 4));

      var parameter = node.CreateParameterForOutput ();

      Assert.That (parameter.Name, Is.EqualTo ("x"));
      Assert.That (parameter.Type, Is.SameAs (typeof (int)));
    }

    [Test]
    public void CreateClause ()
    {
      var previousClause = ExpressionHelper.CreateClause ();
      var predicate = ExpressionHelper.CreateLambdaExpression<int, bool> (i => i > 5);
      var node = new WhereExpressionNode (SourceStub, predicate);

      var clause = (WhereClause) node.CreateClause (previousClause);

      Assert.That (clause.PreviousClause, Is.SameAs (previousClause));
      Assert.That (clause.Predicate, Is.EqualTo (node.Predicate)); // TODO: This should become the resolved expression at a later point of time
    }
  }
}
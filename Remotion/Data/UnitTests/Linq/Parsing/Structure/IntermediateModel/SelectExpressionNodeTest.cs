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
using Remotion.Data.Linq.Parsing.Structure;
using Remotion.Data.Linq.Parsing.Structure.IntermediateModel;
using System.Linq;
using Rhino.Mocks;

namespace Remotion.Data.UnitTests.Linq.Parsing.Structure.IntermediateModel
{
  [TestFixture]
  public class SelectExpressionNodeTest : ExpressionNodeTestBase
  {
    [Test]
    public void SupportedMethod_WithoutPosition ()
    {
      MethodInfo method = GetGenericMethodDefinition (q => q.Select (i => i.ToString()));
      Assert.That (SelectExpressionNode.SupportedMethods, List.Contains (method));
    }

    [Test]
    public void Resolve_ReplacesParameter_WithProjection ()
    {
      var node = new SelectExpressionNode (CreateParseInfo (), ExpressionHelper.CreateLambdaExpression<int, int> (j => j * j));
      var expression = ExpressionHelper.CreateLambdaExpression<int, bool> (i => i > 5);

      var result = node.Resolve (expression.Parameters[0], expression.Body, null);

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

      var result = node.GetResolvedSelector(QuerySourceClauseMapping);

      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void GetResolvedSelector_Cached ()
    {
      var sourceMock = new MockRepository().StrictMock<IExpressionNode>();
      var selector = ExpressionHelper.CreateLambdaExpression<int, bool> (i => i > 5);
      var node = new SelectExpressionNode (CreateParseInfo (sourceMock), selector);
      var expectedResult = ExpressionHelper.CreateLambdaExpression();

      sourceMock.Expect (mock => mock.Resolve (Arg<ParameterExpression>.Is.Anything, Arg<Expression>.Is.Anything, Arg<QuerySourceClauseMapping>.Is.Anything)).Repeat.Once ().Return (
          expectedResult);

      sourceMock.Replay();

      node.GetResolvedSelector (QuerySourceClauseMapping);
      node.GetResolvedSelector (QuerySourceClauseMapping);

      sourceMock.VerifyAllExpectations();
    }

    [Test]
    public void CreateClause ()
    {
      var previousClause = ExpressionHelper.CreateClause();
      var selector = ExpressionHelper.CreateLambdaExpression<int, bool> (i => i > 5);
      var node = new SelectExpressionNode (CreateParseInfo (), selector);

      var selectClause = (SelectClause) node.CreateClause (previousClause, null);

      Assert.That (selectClause.PreviousClause, Is.SameAs (previousClause));
      Assert.That (selectClause.Selector, Is.EqualTo (node.Selector)); // TODO: This should become the resolved expression at a later point of time
      Assert.That (selectClause.ResultModifications, Is.Empty);
    }

    [Test]
    public void CreateParameterForOutput ()
    {
      var node = new SelectExpressionNode (CreateParseInfo (SourceStub, "z"), ExpressionHelper.CreateLambdaExpression<int, string> (y => y.ToString()));

      var parameter = node.CreateParameterForOutput ();

      Assert.That (parameter.Name, Is.EqualTo ("z"));
      Assert.That (parameter.Type, Is.SameAs (typeof (string)));
    }
  }
}
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
using Remotion.Data.Linq.Parsing.Structure;
using Remotion.Data.Linq.Parsing.Structure.IntermediateModel;
using System.Linq;
using Rhino.Mocks;

namespace Remotion.Data.UnitTests.Linq.Parsing.Structure.IntermediateModel
{
  [TestFixture]
  public class LastExpressionNodeTest : ExpressionNodeTestBase
  {
    [Test]
    public void SupportedMethod_WithoutPredicate ()
    {
      MethodInfo method = GetGenericMethodDefinition (q => q.Last());
      Assert.That (LastExpressionNode.SupportedMethods, List.Contains (method));
    }

    [Test]
    public void SupportedMethod_WithPredicate ()
    {
      MethodInfo method = GetGenericMethodDefinition (q => q.Last (i => i > 5));
      Assert.That (LastExpressionNode.SupportedMethods, List.Contains (method));
    }

    [Test]
    public void SupportedMethod_LastOrDefault_WithoutPredicate ()
    {
      MethodInfo method = GetGenericMethodDefinition (q => q.LastOrDefault ());
      Assert.That (LastExpressionNode.SupportedMethods, List.Contains (method));
    }

    [Test]
    public void SupportedMethod_LastOrDefault_WithPredicate ()
    {
      MethodInfo method = GetGenericMethodDefinition (q => q.LastOrDefault (i => i > 5));
      Assert.That (LastExpressionNode.SupportedMethods, List.Contains (method));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException))]
    public void Resolve_ThrowsInvalidOperationException ()
    {
      var node = new LastExpressionNode (CreateParseInfo (), null);
      node.Resolve (ExpressionHelper.CreateParameterExpression (), ExpressionHelper.CreateExpression (), QuerySourceClauseMapping);
    }

    [Test]
    public void GetResolvedPredicate ()
    {
      var predicate = ExpressionHelper.CreateLambdaExpression<int, bool> (i => i > 5);
      var node = new LastExpressionNode (CreateParseInfo (), predicate);

      var expectedResult = Expression.MakeBinary (ExpressionType.GreaterThan, SourceReference, Expression.Constant (5));

      var result = node.GetResolvedOptionalPredicate (QuerySourceClauseMapping);

      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void GetResolvedPredicate_Null ()
    {
      var sourceMock = MockRepository.GenerateMock<IExpressionNode> ();
      var node = new LastExpressionNode (CreateParseInfo (sourceMock), null);
      var result = node.GetResolvedOptionalPredicate (QuerySourceClauseMapping);
      Assert.That (result, Is.Null);
    }

    [Test]
    public void GetResolvedPredicate_Cached ()
    {
      var sourceMock = new MockRepository ().StrictMock<IExpressionNode> ();
      var predicate = ExpressionHelper.CreateLambdaExpression<int, bool> (i => i > 5);
      var node = new LastExpressionNode (CreateParseInfo (sourceMock), predicate);
      var expectedResult = ExpressionHelper.CreateLambdaExpression ();

      sourceMock.Expect (mock => mock.Resolve (Arg<ParameterExpression>.Is.Anything, Arg<Expression>.Is.Anything, Arg<QuerySourceClauseMapping>.Is.Anything)).Repeat.Once ().Return (expectedResult);

      sourceMock.Replay ();

      node.GetResolvedOptionalPredicate (QuerySourceClauseMapping);
      node.GetResolvedOptionalPredicate (QuerySourceClauseMapping);

      sourceMock.VerifyAllExpectations ();
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException))]
    public void CreateParameterForOutput ()
    {
      var node = new LastExpressionNode (CreateParseInfo (), null);
      node.CreateParameterForOutput ();
    }

    [Test]
    public void CreateClause_WithoutOptionalPredicate_PreviousClauseIsSelect ()
    {
      var node = new LastExpressionNode (CreateParseInfo (), null);

      TestCreateClause_PreviousClauseIsSelect (node, typeof (LastResultModification));
    }

    [Test]
    public void CreateClause_WithoutOptionalPredicate_PreviousClauseIsNoSelect ()
    {
      var source = new ConstantExpressionNode ("i1", typeof (int[]), new[] { 1, 2, 3 });
      var node = new LastExpressionNode (CreateParseInfo (source), null);

      TestCreateClause_PreviousClauseIsNoSelect (node, typeof (LastResultModification));
    }

    [Test]
    public void CreateClause_WithOptionalPredicate_CreatesWhereClause ()
    {
      var node = new LastExpressionNode (CreateParseInfo (), ExpressionHelper.CreateLambdaExpression<int, bool> (i => i > 5));
      TestCreateClause_WithOptionalPredicate (node, node.OptionalPredicate);
    }

    [Test]
    public void CreateClause_NoDefaultAllowed ()
    {
      var node = new LastExpressionNode (CreateParseInfo (LastExpressionNode.SupportedMethods[0].MakeGenericMethod (typeof (Student))), null);
      var clause = (SelectClause) node.CreateClause (ExpressionHelper.CreateClause (), QuerySourceClauseMapping);

      Assert.That (((LastResultModification) clause.ResultModifications[0]).ReturnDefaultWhenEmpty, Is.False);
    }

    [Test]
    public void CreateClause_DefaultAllowed ()
    {
      var node = new LastExpressionNode (CreateParseInfo (LastExpressionNode.SupportedMethods[3].MakeGenericMethod (typeof (Student))), null);
      var clause = (SelectClause) node.CreateClause (ExpressionHelper.CreateClause (), QuerySourceClauseMapping);

      Assert.That (((LastResultModification) clause.ResultModifications[0]).ReturnDefaultWhenEmpty, Is.True);
    }
  }
}
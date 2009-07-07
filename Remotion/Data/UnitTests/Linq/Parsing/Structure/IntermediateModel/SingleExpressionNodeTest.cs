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
using Remotion.Data.Linq.Clauses.ResultOperators;
using Remotion.Data.Linq.Parsing.Structure.IntermediateModel;
using System.Linq;
using Rhino.Mocks;

namespace Remotion.Data.UnitTests.Linq.Parsing.Structure.IntermediateModel
{
  [TestFixture]
  public class SingleExpressionNodeTest : ExpressionNodeTestBase
  {
    private SingleExpressionNode _node;
    private SingleExpressionNode _nodeWithPredicate;
    private Expression<Func<int, bool>> _predicate;

    [SetUp]
    public override void SetUp ()
    {
      base.SetUp ();

      _node = new SingleExpressionNode (CreateParseInfo (), null);
      _predicate = ExpressionHelper.CreateLambdaExpression<int, bool> (i => i > 5);
      _nodeWithPredicate = new SingleExpressionNode (CreateParseInfo (), _predicate);

    }

    [Test]
    public void SupportedMethod_WithoutPredicate ()
    {
      AssertSupportedMethod_Generic (SingleExpressionNode.SupportedMethods, q => q.Single (), e => e.Single ());
    }

    [Test]
    public void SupportedMethod_WithPredicate ()
    {
      AssertSupportedMethod_Generic (SingleExpressionNode.SupportedMethods, q => q.Single (o => o == null), e => e.Single (o => o == null));
    }

    [Test]
    public void SupportedMethod_SingleOrDefault_WithoutPredicate ()
    {
      AssertSupportedMethod_Generic (SingleExpressionNode.SupportedMethods, q => q.SingleOrDefault (), e => e.SingleOrDefault ());
    }

    [Test]
    public void SupportedMethod_SingleOrDefault_WithPredicate ()
    {
      AssertSupportedMethod_Generic (SingleExpressionNode.SupportedMethods, q => q.SingleOrDefault (o => o == null), e => e.SingleOrDefault (o => o == null));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException))]
    public void Resolve_ThrowsInvalidOperationException ()
    {
      _node.Resolve (ExpressionHelper.CreateParameterExpression (), ExpressionHelper.CreateExpression (), ClauseGenerationContext);
    }

    [Test]
    public void GetResolvedPredicate ()
    {
      var expectedResult = Expression.MakeBinary (ExpressionType.GreaterThan, SourceReference, Expression.Constant (5));

      var result = _nodeWithPredicate.GetResolvedOptionalPredicate (ClauseGenerationContext);

      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void GetResolvedPredicate_Null ()
    {
      var sourceMock = MockRepository.GenerateMock<IExpressionNode> ();
      var node = new SingleExpressionNode (CreateParseInfo (sourceMock), null);
      var result = node.GetResolvedOptionalPredicate (ClauseGenerationContext);
      Assert.That (result, Is.Null);
    }

    [Test]
    public void Apply_WithoutOptionalPredicate ()
    {
      TestApply (_node, typeof (SingleResultOperator));
    }

    [Test]
    public void Apply_WithOptionalPredicate_CreatesWhereClause ()
    {
      TestApply_WithOptionalPredicate (_nodeWithPredicate);
    }

    [Test]
    public void Apply_NoDefaultAllowed ()
    {
      var node = new SingleExpressionNode (CreateParseInfo (SingleExpressionNode.SupportedMethods[0].MakeGenericMethod (typeof (Student))), null);
      node.Apply (QueryModel, ClauseGenerationContext);

      Assert.That (((SingleResultOperator) QueryModel.ResultOperators[0]).ReturnDefaultWhenEmpty, Is.False);
    }

    [Test]
    public void Apply_DefaultAllowed ()
    {
      var node = new SingleExpressionNode (CreateParseInfo (SingleExpressionNode.SupportedMethods[3].MakeGenericMethod (typeof (Student))), null);
      node.Apply (QueryModel, ClauseGenerationContext);

      Assert.That (((SingleResultOperator) QueryModel.ResultOperators[0]).ReturnDefaultWhenEmpty, Is.True);
    }
  }
}
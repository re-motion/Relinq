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
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Clauses.ResultModifications;
using Remotion.Data.Linq.Parsing.Structure.IntermediateModel;
using System.Linq;
using Rhino.Mocks;

namespace Remotion.Data.UnitTests.Linq.Parsing.Structure.IntermediateModel
{
  [TestFixture]
  public class CountExpressionNodeTest : ExpressionNodeTestBase
  {
    private CountExpressionNode _node;
    private CountExpressionNode _nodeWithPredicate;
    private Expression<Func<int, bool>> _predicate;

    public override void SetUp ()
    {
      base.SetUp ();
      _node = new CountExpressionNode (CreateParseInfo (), null);
      _predicate = ExpressionHelper.CreateLambdaExpression<int, bool> (i => i > 5);
      _nodeWithPredicate = new CountExpressionNode (CreateParseInfo (), _predicate);
    }

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
      _node.Resolve (ExpressionHelper.CreateParameterExpression (), ExpressionHelper.CreateExpression (), ClauseGenerationContext);
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException))]
    public void CreateParameterForOutput ()
    {
      _node.CreateParameterForOutput ();
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
      var node = new CountExpressionNode (CreateParseInfo (sourceMock), null);
      var result = node.GetResolvedOptionalPredicate (ClauseGenerationContext);
      Assert.That (result, Is.Null);
    }

    [Test]
    public void CreateClause ()
    {
      var previousClause = ExpressionHelper.CreateClause();
      var result = _node.CreateClause (previousClause, ClauseGenerationContext);
      Assert.That (result, Is.SameAs (previousClause));
      Assert.That (ClauseGenerationContext.ResultModificationNodeRegistry.ToArray(), List.Contains (_node));
    }

    [Test]
    public void Apply_WithoutOptionalPredicate ()
    {
      TestApply (_node, typeof (CountResultModification));
    }

    [Test]
    public void Apply_WithOptionalPredicate_CreatesWhereClause ()
    {
      TestApply_WithOptionalPredicate (_nodeWithPredicate);
    }

    [Test]
    public void CreateSelectClause ()
    {
      var previousClause = ExpressionHelper.CreateClause ();

      var selectClause = _node.CreateSelectClause (previousClause, ClauseGenerationContext);
      Assert.That (((QuerySourceReferenceExpression) selectClause.Selector).ReferencedClause, Is.SameAs (SourceClause));
    }
  }
}
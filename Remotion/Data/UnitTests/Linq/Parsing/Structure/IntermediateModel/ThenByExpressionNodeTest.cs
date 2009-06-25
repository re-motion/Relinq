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
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.Parsing.Structure.IntermediateModel;
using System.Linq;
using Rhino.Mocks;

namespace Remotion.Data.UnitTests.Linq.Parsing.Structure.IntermediateModel
{
  [TestFixture]
  public class ThenByExpressionNodeTest : ExpressionNodeTestBase
  {
    private ThenByExpressionNode _node;

    public override void SetUp ()
    {
      base.SetUp ();

      var selector = ExpressionHelper.CreateLambdaExpression<int, bool> (i => i > 5);
      _node = new ThenByExpressionNode (CreateParseInfo (), selector);
    }

    [Test]
    public void SupportedMethod_WithoutComparer ()
    {
      MethodInfo method = GetGenericMethodDefinition (q => ((IOrderedQueryable<object>) q).ThenBy (i => i));
      Assert.That (ThenByExpressionNode.SupportedMethods, List.Contains (method));
    }

    [Test]
    public void Resolve_PassesExpressionToSource ()
    {
      var sourceMock = MockRepository.GenerateMock<IExpressionNode>();
      var selector = ExpressionHelper.CreateLambdaExpression<int, int> (i => i);
      var node = new ThenByExpressionNode (CreateParseInfo (sourceMock), selector);
      var expression = ExpressionHelper.CreateLambdaExpression();
      var parameter = ExpressionHelper.CreateParameterExpression();
      var expectedResult = ExpressionHelper.CreateExpression();
      sourceMock.Expect (mock => mock.Resolve (parameter, expression, ClauseGenerationContext)).Return (expectedResult);

      var result = node.Resolve (parameter, expression, ClauseGenerationContext);

      sourceMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (expectedResult));
    }

    [Test]
    public void GetResolvedSelector ()
    {
      var expectedResult = Expression.MakeBinary (ExpressionType.GreaterThan, SourceReference, Expression.Constant (5));

      var result = _node.GetResolvedKeySelector (ClauseGenerationContext);

      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void Apply ()
    {
      var clause = new OrderByClause ();
      QueryModel.BodyClauses.Add (clause);

      _node.Apply (QueryModel, ClauseGenerationContext);

      Assert.That (clause.Orderings.Count, Is.EqualTo (1));
      Assert.That (clause.Orderings[0].OrderingDirection, Is.EqualTo (OrderingDirection.Asc));
      Assert.That (clause.Orderings[0].Expression, Is.SameAs (_node.GetResolvedKeySelector (ClauseGenerationContext)));
    }

    [Test]
    public void Apply_UsesLastClause ()
    {
      var clause = new OrderByClause ();
      QueryModel.BodyClauses.Add (new OrderByClause ());
      QueryModel.BodyClauses.Add (clause);

      _node.Apply (QueryModel, ClauseGenerationContext);

      Assert.That (clause.Orderings.Count, Is.EqualTo (1));
    }

    [Test]
    [ExpectedException (typeof (ParserException))]
    public void Apply_NoPreviousClause ()
    {
      _node.Apply (QueryModel, ClauseGenerationContext);
    }

    [Test]
    [ExpectedException (typeof (ParserException))]
    public void Apply_InvalidPreviousClause ()
    {
      QueryModel.BodyClauses.Add (new WhereClause (ExpressionHelper.CreateExpression ()));
      _node.Apply (QueryModel, ClauseGenerationContext);
    }
  }
}
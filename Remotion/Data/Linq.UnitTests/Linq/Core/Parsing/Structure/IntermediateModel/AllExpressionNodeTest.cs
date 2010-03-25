// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// as published by the Free Software Foundation; either version 2.1 of the 
// License, or (at your option) All later version.
// 
// re-motion is distributed in the hope that it will be useful, 
// but WITHOUT All WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-motion; if not, see http://www.gnu.org/licenses.
// 
using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Clauses.ResultOperators;
using Remotion.Data.Linq.Parsing.Structure.IntermediateModel;

namespace Remotion.Data.Linq.UnitTests.Linq.Core.Parsing.Structure.IntermediateModel
{
  [TestFixture]
  public class AllExpressionNodeTest : ExpressionNodeTestBase
  {
    private Expression<Func<int, bool>> _predicate;
    private AllExpressionNode _node;

    public override void SetUp ()
    {
      base.SetUp ();

      _predicate = ExpressionHelper.CreateLambdaExpression<int, bool> (i => i > 2);
      _node = new AllExpressionNode (CreateParseInfo (), _predicate);
    }

    [Test]
    public void SupportedMethods ()
    {
      AssertSupportedMethod_Generic (AllExpressionNode.SupportedMethods, q => q.All (o => o == null), e => e.All (o => o == null));
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
      var predicate = ExpressionHelper.CreateLambdaExpression<int, bool> (i => i > 5);
      var node = new AllExpressionNode (CreateParseInfo (), predicate);

      var expectedResult = Expression.MakeBinary (ExpressionType.GreaterThan, SourceReference, Expression.Constant (5));

      var result = node.GetResolvedPredicate (ClauseGenerationContext);

      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void Apply ()
    {
      var result = _node.Apply (QueryModel, ClauseGenerationContext);
      Assert.That (result, Is.SameAs (QueryModel));

      Assert.That (QueryModel.ResultOperators.Count, Is.EqualTo (1));
      Assert.That (QueryModel.ResultOperators[0], Is.InstanceOfType (typeof (AllResultOperator)));

      var resultOperator = ((AllResultOperator) QueryModel.ResultOperators[0]);
      Assert.That (resultOperator.Predicate, Is.EqualTo (_node.GetResolvedPredicate (ClauseGenerationContext)));
    }
  }
}

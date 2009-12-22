// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// as published by the Free Software Foundation; either version 2.1 of the 
// License, or (at your option) any later version.
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
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Clauses.ResultOperators;
using Remotion.Data.Linq.Parsing.Structure.IntermediateModel;

namespace Remotion.Data.Linq.UnitTests.Parsing.Structure.IntermediateModel
{
  [TestFixture]
  public class AggregateFromSeedExpressionNodeTest : ExpressionNodeTestBase
  {
    private ConstantExpression _seed;
    private Expression<Func<int, int, int>> _func;
    private Expression<Func<int, string>> _resultSelector;

    private AggregateFromSeedExpressionNode _nodeWithoutResultSelector;
    private AggregateFromSeedExpressionNode _nodeWithResultSelector;

    public override void SetUp ()
    {
      base.SetUp ();
      _seed = Expression.Constant (0);
      _func = ExpressionHelper.CreateLambdaExpression<int, int, int> ((total, i) => total + i);
      _resultSelector = ExpressionHelper.CreateLambdaExpression<int, string> (total => total.ToString());

      _nodeWithoutResultSelector = new AggregateFromSeedExpressionNode (CreateParseInfo (), _seed, _func, null);
      _nodeWithResultSelector = new AggregateFromSeedExpressionNode (CreateParseInfo (), _seed, _func, _resultSelector);
    }

    [Test]
    public void SupportedMethods_WithoutResultSelector ()
    {
      AssertSupportedMethod_Generic (
          AggregateFromSeedExpressionNode.SupportedMethods,
          q => q.Aggregate<object, object> (null, (i, j) => null),
          e => e.Aggregate<object, object> (null, (i, j) => null));
    }

    [Test]
    public void SupportedMethods_WithResultSelector ()
    {
      AssertSupportedMethod_Generic (
          AggregateFromSeedExpressionNode.SupportedMethods,
          q => q.Aggregate<object, object, object> (null, (i, j) => null, o => null),
          e => e.Aggregate<object, object, object> (null, (i, j) => null, o => null));
    }

    [Test]
    public void Initialization_WithoutResultSelector ()
    {
      var parseInfo = CreateParseInfo();
      var node = new AggregateFromSeedExpressionNode (parseInfo, _seed, _func, null);

      Assert.That (node.Source, Is.SameAs (SourceNode));
      Assert.That (node.Seed, Is.SameAs (_seed));
      Assert.That (node.Func, Is.SameAs (_func));
      Assert.That (node.OptionalResultSelector, Is.Null);
    }

    [Test]
    public void Initialization_WithResultSelector ()
    {
      var parseInfo = CreateParseInfo ();
      var node = new AggregateFromSeedExpressionNode (parseInfo, _seed, _func, _resultSelector);

      Assert.That (node.Source, Is.SameAs (SourceNode));
      Assert.That (node.Seed, Is.SameAs (_seed));
      Assert.That (node.Func, Is.SameAs (_func));
      Assert.That (node.OptionalResultSelector, Is.SameAs (_resultSelector));
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = "Func must have exactly two parameters.\r\nParameter name: func")]
    public void Initialization_WrongNumberOfParametersInFunc ()
    {
      var parseInfo = CreateParseInfo ();
      new AggregateFromSeedExpressionNode (parseInfo, _seed, ExpressionHelper.CreateLambdaExpression<int, bool> (i => false), _resultSelector);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = 
        "Result selector must have exactly one parameter.\r\nParameter name: optionalResultSelector")]
    public void Initialization_WrongNumberOfParametersInResultSelector ()
    {
      var parseInfo = CreateParseInfo ();
      new AggregateFromSeedExpressionNode (parseInfo, _seed, _func, ExpressionHelper.CreateLambdaExpression<int, int, bool> ((i, j) => false));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException))]
    public void Resolve_ThrowsInvalidOperationException ()
    {
      _nodeWithResultSelector.Resolve (ExpressionHelper.CreateParameterExpression (), ExpressionHelper.CreateExpression (), ClauseGenerationContext);
    }

    [Test]
    public void GetResolvedFunc ()
    {
      var expectedResult = Expression.Lambda (Expression.Add (_func.Parameters[0], SourceReference), _func.Parameters[0]);

      var result = _nodeWithResultSelector.GetResolvedFunc (ClauseGenerationContext);

      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void Apply_WithoutResultSelector ()
    {
      var result = _nodeWithoutResultSelector.Apply (QueryModel, ClauseGenerationContext);
      Assert.That (result, Is.SameAs (QueryModel));

      var resultOperator = (AggregateFromSeedResultOperator) QueryModel.ResultOperators[0];
      Assert.That (resultOperator.Seed, Is.SameAs (_seed));
      Assert.That (resultOperator.Func, Is.EqualTo (_nodeWithoutResultSelector.GetResolvedFunc (ClauseGenerationContext)));
      Assert.That (resultOperator.OptionalResultSelector, Is.Null);
    }

    [Test]
    public void Apply_WithResultSelector ()
    {
      var result = _nodeWithResultSelector.Apply (QueryModel, ClauseGenerationContext);
      Assert.That (result, Is.SameAs (QueryModel));

      var resultOperator = (AggregateFromSeedResultOperator) QueryModel.ResultOperators[0];
      Assert.That (resultOperator.Seed, Is.SameAs (_seed));
      Assert.That (resultOperator.Func, Is.EqualTo (_nodeWithResultSelector.GetResolvedFunc (ClauseGenerationContext)));
      Assert.That (resultOperator.OptionalResultSelector, Is.SameAs (_nodeWithResultSelector.OptionalResultSelector));
    }
  }
}

// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (C) rubicon IT GmbH, www.rubicon.eu
// 
// re-linq is free software; you can redistribute it and/or modify it under 
// the terms of the GNU Lesser General Public License as published by the 
// Free Software Foundation; either version 2.1 of the License, 
// or (at your option) any later version.
// 
// re-linq is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-linq; if not, see http://www.gnu.org/licenses.
// 
using System;
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Parsing.Structure.IntermediateModel;

namespace Remotion.Linq.UnitTests.Linq.Core.Parsing.Structure.IntermediateModel
{
  [TestFixture]
  public class ResultOperatorExpressionNodeBaseTest : ExpressionNodeTestBase
  {
    private Expression<Func<int, bool>> _predicate;
    private Expression<Func<int, string>> _selector;

    private TestResultOperatorExpressionNode _nodeWithPredicate;
    private TestResultOperatorExpressionNode _nodeWithSelector;
    private TestResultOperatorExpressionNode _nodeWithoutOptionals;
    private TestResultOperatorExpressionNode _nodeWithBothOptionals;

    public override void SetUp ()
    {
      base.SetUp ();

      _predicate = ExpressionHelper.CreateLambdaExpression<int, bool> (i => i > 5);
      _selector = (i => i.ToString ());

      _nodeWithPredicate = new TestResultOperatorExpressionNode (CreateParseInfo (), _predicate, null);
      _nodeWithSelector = new TestResultOperatorExpressionNode (CreateParseInfo (), null, _selector);
      _nodeWithoutOptionals = new TestResultOperatorExpressionNode (CreateParseInfo (), null, null);
      _nodeWithBothOptionals = new TestResultOperatorExpressionNode (CreateParseInfo (), _predicate, _selector);
    }

    [Test]
    public void Initialization_WithPredicate ()
    {
      Assert.That (_nodeWithPredicate.Source, Is.Not.SameAs (SourceNode));
      Assert.That (_nodeWithPredicate.Source, Is.InstanceOf (typeof (WhereExpressionNode)));

      var source = (WhereExpressionNode) _nodeWithPredicate.Source;
      Assert.That (source.Source, Is.SameAs (SourceNode));
      Assert.That (source.Predicate, Is.SameAs (_predicate));
    }

    [Test]
    public void Initialization_WithSelector ()
    {
      Assert.That (_nodeWithSelector.Source, Is.Not.SameAs (SourceNode));
      Assert.That (_nodeWithSelector.Source, Is.InstanceOf (typeof (SelectExpressionNode)));

      var source = (SelectExpressionNode) _nodeWithSelector.Source;
      Assert.That (source.Source, Is.SameAs (SourceNode));
      Assert.That (source.Selector, Is.SameAs (_selector));
    }

    [Test]
    public void Initialization_WithPredicateAndSelector ()
    {
      Assert.That (_nodeWithBothOptionals.Source, Is.Not.SameAs (SourceNode));
      Assert.That (_nodeWithBothOptionals.Source, Is.InstanceOf (typeof (SelectExpressionNode)));

      var selectSource = (SelectExpressionNode) _nodeWithBothOptionals.Source;
      Assert.That (selectSource.Selector, Is.SameAs (_selector));
      Assert.That (selectSource.Source, Is.InstanceOf (typeof (WhereExpressionNode)));

      var source = (WhereExpressionNode) selectSource.Source;
      Assert.That (source.Predicate, Is.SameAs (_predicate));
      Assert.That (source.Source, Is.SameAs (SourceNode));
    }

    [Test]
    public void Apply_DoesNotWrapQueryModel_AfterResultOperator ()
    {
      QueryModel.ResultOperators.Add (new DistinctResultOperator ());
      var result = _nodeWithoutOptionals.Apply (QueryModel, ClauseGenerationContext);
      Assert.That (result, Is.SameAs (QueryModel));
    }

    [Test]
    public void IntegrationTest_Apply_WithOptionalPredicate ()
    {
      _nodeWithPredicate.Source.Apply (QueryModel, ClauseGenerationContext);
      _nodeWithPredicate.Apply (QueryModel, ClauseGenerationContext);

      Assert.That (QueryModel.BodyClauses[0], Is.InstanceOf (typeof (WhereClause)));

      var whereClause = (WhereClause) QueryModel.BodyClauses[0];
      var expectedNewPredicate = ExpressionHelper.Resolve<int, bool> (SourceClause, i => i > 5);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedNewPredicate, whereClause.Predicate);
    }

    [Test]
    public void IntegrationTest_Apply_WithOptionalSelector ()
    {
      _nodeWithSelector.Source.Apply (QueryModel, ClauseGenerationContext);
      _nodeWithSelector.Apply (QueryModel, ClauseGenerationContext);

      var selectClause = QueryModel.SelectClause;
      var expectedNewSelector = (MethodCallExpression) ExpressionHelper.Resolve<int, string> (SourceClause, i => i.ToString ());
      ExpressionTreeComparer.CheckAreEqualTrees (expectedNewSelector, selectClause.Selector);
    }


  }
}

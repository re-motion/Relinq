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
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.ResultOperators;
using Remotion.Data.Linq.Parsing.Structure.IntermediateModel;

namespace Remotion.Data.Linq.UnitTests.Linq.Core.Parsing.Structure.IntermediateModel
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
      Assert.That (_nodeWithPredicate.Source, Is.InstanceOfType (typeof (WhereExpressionNode)));

      var source = (WhereExpressionNode) _nodeWithPredicate.Source;
      Assert.That (source.Source, Is.SameAs (SourceNode));
      Assert.That (source.Predicate, Is.SameAs (_predicate));
    }

    [Test]
    public void Initialization_WithSelector ()
    {
      Assert.That (_nodeWithSelector.Source, Is.Not.SameAs (SourceNode));
      Assert.That (_nodeWithSelector.Source, Is.InstanceOfType (typeof (SelectExpressionNode)));

      var source = (SelectExpressionNode) _nodeWithSelector.Source;
      Assert.That (source.Source, Is.SameAs (SourceNode));
      Assert.That (source.Selector, Is.SameAs (_selector));
    }

    [Test]
    public void Initialization_WithPredicateAndSelector ()
    {
      Assert.That (_nodeWithBothOptionals.Source, Is.Not.SameAs (SourceNode));
      Assert.That (_nodeWithBothOptionals.Source, Is.InstanceOfType (typeof (SelectExpressionNode)));

      var selectSource = (SelectExpressionNode) _nodeWithBothOptionals.Source;
      Assert.That (selectSource.Selector, Is.SameAs (_selector));
      Assert.That (selectSource.Source, Is.InstanceOfType (typeof (WhereExpressionNode)));

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

      Assert.That (QueryModel.BodyClauses[0], Is.InstanceOfType (typeof (WhereClause)));

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

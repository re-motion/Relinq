// Copyright (c) rubicon IT GmbH, www.rubicon.eu
//
// See the NOTICE file distributed with this work for additional information
// regarding copyright ownership.  rubicon licenses this file to you under 
// the Apache License, Version 2.0 (the "License"); you may not use this 
// file except in compliance with the License.  You may obtain a copy of the 
// License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, WITHOUT 
// WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.  See the 
// License for the specific language governing permissions and limitations
// under the License.
// 
using System;
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.Parsing.Structure.IntermediateModel;

namespace Remotion.Linq.UnitTests.Parsing.Structure.IntermediateModel
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

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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Parsing.Structure.IntermediateModel;

namespace Remotion.Data.UnitTests.Linq.Parsing.Structure.IntermediateModel
{
  [TestFixture]
  public class GroupJoinExpressionNodeTest : ExpressionNodeTestBase
  {
    private Expression _innerSequence;
    private Expression<Func<string, string>> _outerKeySelector;
    private Expression<Func<string, string>> _innerKeySelector;
    private Expression<Func<string,IEnumerable<string>,string>>  _resultSelector;
    private GroupJoinExpressionNode _node;
    private GroupJoinClause _groupJoinClause;

    public override void SetUp ()
    {
      base.SetUp ();
      _innerSequence = ExpressionHelper.CreateExpression ();
      _outerKeySelector = ExpressionHelper.CreateLambdaExpression<string, string> (o => o.ToString ());
      _innerKeySelector = ExpressionHelper.CreateLambdaExpression<string, string> (i => i.ToString ());
      _resultSelector = ExpressionHelper.CreateLambdaExpression<string, IEnumerable<string>, string> ((o, into) => o.ToString() + into.ToString());

      _node = new GroupJoinExpressionNode (
          CreateParseInfo (SourceNode, "groupJoin"), _innerSequence, _outerKeySelector, _innerKeySelector, _resultSelector);
      _groupJoinClause = ExpressionHelper.CreateGroupJoinClause();
    }

    [Test]
    public void SupportedMethods ()
    {
      AssertSupportedMethod_Generic (
        GroupJoinExpressionNode.SupportedMethods,
        q => q.GroupJoin (new string[0], o => o.ToString (), i => i, (o, i) => o),
        e => e.GroupJoin (new string[0], o => o.ToString (), i => i, (o, i) => o));
    }

    [Test]
    public void GetResolvedResultSelector ()
    {
      ClauseGenerationContext.AddContextInfo (_node, _groupJoinClause);

      var resolvedExpression = _node.GetResolvedResultSelector (ClauseGenerationContext);

      var expectedExpression = ExpressionHelper.Resolve<string, IEnumerable<string>, string> (
          SourceClause, 
          _groupJoinClause, 
          (o, into) => o.ToString () + into.ToString());
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, resolvedExpression);
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "Cannot retrieve an IQuerySource for the given GroupJoinExpressionNode. "
        + "Be sure to call Apply before calling methods that require IQuerySources, and pass in the same QuerySourceClauseMapping to both.")]
    public void GetResolvedResultSelector_WithoutClause ()
    {
      _node.GetResolvedResultSelector (ClauseGenerationContext);
    }

    [Test]
    public void Resolve ()
    {
      ClauseGenerationContext.AddContextInfo (_node, _groupJoinClause);

      var parameter = Expression.Parameter (typeof (string), "s");
      var result = _node.Resolve (parameter, parameter, ClauseGenerationContext);

      var expectedResult = ExpressionHelper.Resolve<string, IEnumerable<string>, string> (
          SourceClause, 
          _groupJoinClause, 
          (o, into) => o.ToString () + into.ToString());
      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void Apply ()
    {
      var result = _node.Apply (QueryModel, ClauseGenerationContext);
      Assert.That (result, Is.SameAs (QueryModel));

      var clause = (GroupJoinClause) QueryModel.BodyClauses[0];

      Assert.That (clause.ItemName, Is.EqualTo ("into"));
      Assert.That (clause.ItemType, Is.SameAs (typeof (IEnumerable<string>)));
      Assert.That (clause.JoinClause.ItemName, Is.EqualTo ("i"));
      Assert.That (clause.JoinClause.ItemType, Is.SameAs (typeof (string)));
      Assert.That (clause.JoinClause.InnerSequence, Is.SameAs (_innerSequence));
      Assert.That (clause.JoinClause.OuterKeySelector, Is.SameAs (_node.JoinExpressionNode.GetResolvedOuterKeySelector (ClauseGenerationContext)));
      Assert.That (clause.JoinClause.InnerKeySelector, Is.SameAs (_node.JoinExpressionNode.GetResolvedInnerKeySelector (ClauseGenerationContext)));
    }

    [Test]
    public void Apply_AddsMapping ()
    {
      _node.Apply (QueryModel, ClauseGenerationContext);
      var clause = (GroupJoinClause) QueryModel.BodyClauses[0];

      Assert.That (ClauseGenerationContext.GetContextInfo (_node), Is.SameAs (clause));
    }

    [Test]
    public void Apply_AdaptsSelector ()
    {
      _node.Apply (QueryModel, ClauseGenerationContext);
      var clause = QueryModel.SelectClause;

      Assert.That (clause.Selector, Is.SameAs (_node.GetResolvedResultSelector (ClauseGenerationContext)));
    }
  }
}
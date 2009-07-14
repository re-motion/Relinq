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
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Parsing.Structure.IntermediateModel;
using System.Linq;

namespace Remotion.Data.UnitTests.Linq.Parsing.Structure.IntermediateModel
{
  [TestFixture]
  public class JoinExpressionNodeTest : ExpressionNodeTestBase
  {
    private JoinExpressionNode _node;
    private Expression _innerSequence;
    private Expression<Func<string, string>> _outerKeySelector;
    private Expression<Func<string, string>> _innerKeySelector;
    private Expression<Func<string, string, string>> _resultSelector;
    private JoinClause _joinClause;

    [SetUp]
    public override void SetUp ()
    {
      base.SetUp ();

      _innerSequence = ExpressionHelper.CreateExpression();
      _outerKeySelector = ExpressionHelper.CreateLambdaExpression<string, string> (o => o.ToString ());
      _innerKeySelector = ExpressionHelper.CreateLambdaExpression<string, string> (i => i.ToString ());
      _resultSelector = ExpressionHelper.CreateLambdaExpression<string, string, string> ((o, i) => o.ToString () + i.ToString ());

      _node = new JoinExpressionNode (CreateParseInfo (SourceNode, "join"), _innerSequence, _outerKeySelector, _innerKeySelector, _resultSelector);
      _joinClause = ExpressionHelper.CreateJoinClause ();
    }

    [Test]
    public void SupportedMethods ()
    {
      AssertSupportedMethod_Generic (
        JoinExpressionNode.SupportedMethods,
        q => q.Join (new string[0], o => o.ToString (), i => i, (o, i) => o),
        e => e.Join (new string[0], o => o.ToString (), i => i, (o, i) => o));      
    }

    [Test]
    public void GetResolvedOuterKeySelector ()
    {
      var resolvedExpression = _node.GetResolvedOuterKeySelector (ClauseGenerationContext);
      var expectedExpression = ExpressionHelper.Resolve<string, string> (SourceClause, o => o.ToString());

      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, resolvedExpression);
    }

    [Test]
    public void GetResolvedInnerKeySelector ()
    {
      ClauseGenerationContext.ClauseMapping.AddMapping (_node, _joinClause);

      var resolvedExpression = _node.GetResolvedInnerKeySelector (ClauseGenerationContext);
      
      var expectedExpression = ExpressionHelper.Resolve<string, string> (_joinClause, i => i.ToString ());
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, resolvedExpression);
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "Cannot resolve with a JoinExpressionNode for which no clause was "
        + "created. Be sure to call Apply before calling GetResolved..., and pass in the same QuerySourceClauseMapping to both methods.")]
    public void GetResolvedInnerKeySelector_WithoutClause ()
    {
      _node.GetResolvedInnerKeySelector (ClauseGenerationContext);
    }

    [Test]
    public void GetResolvedResultSelector ()
    {
      ClauseGenerationContext.ClauseMapping.AddMapping (_node, _joinClause);

      var resolvedExpression = _node.GetResolvedResultSelector (ClauseGenerationContext);
      
      var expectedExpression = ExpressionHelper.Resolve<string, string, string> (SourceClause, _joinClause, (o, i) => o.ToString () + i.ToString ());
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, resolvedExpression);
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "Cannot resolve with a JoinExpressionNode for which no clause was "
       + "created. Be sure to call Apply before calling GetResolved..., and pass in the same QuerySourceClauseMapping to both methods.")]
    public void GetResolvedResultSelector_WithoutClause ()
    {
      _node.GetResolvedResultSelector (ClauseGenerationContext);
    }

    [Test]
    public void Resolve ()
    {
      ClauseGenerationContext.ClauseMapping.AddMapping (_node, _joinClause);

      var parameter = Expression.Parameter (typeof (string), "s");
      var result = _node.Resolve (parameter, parameter, ClauseGenerationContext);

      var expectedResult = ExpressionHelper.Resolve<string, string, string> (SourceClause, _joinClause, (o, i) => o.ToString() + i.ToString());
      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void Apply ()
    {
      var result = _node.Apply (QueryModel, ClauseGenerationContext);
      Assert.That (result, Is.SameAs (QueryModel));

      var clause = (JoinClause) QueryModel.BodyClauses[0];

      Assert.That (clause.ItemName, Is.EqualTo ("join"));
      Assert.That (clause.ItemType, Is.SameAs (typeof (string)));
      Assert.That (clause.InnerSequence, Is.SameAs (_innerSequence));
      Assert.That (clause.OuterKeySelector, Is.SameAs (_node.GetResolvedOuterKeySelector (ClauseGenerationContext)));
      Assert.That (clause.InnerKeySelector, Is.SameAs (_node.GetResolvedInnerKeySelector (ClauseGenerationContext)));     
    }

    [Test]
    public void Apply_AddsMapping ()
    {
      _node.Apply (QueryModel, ClauseGenerationContext);
      var clause = (JoinClause) QueryModel.BodyClauses[0];

      Assert.That (QuerySourceClauseMapping.GetClause (_node), Is.SameAs (clause));
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
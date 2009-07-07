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
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Clauses.ResultOperators;
using Remotion.Data.Linq.Parsing.Structure.IntermediateModel;
using System.Linq.Expressions;
using System.Linq;
using Remotion.Data.UnitTests.Linq.Parsing.Structure.IntermediateModel.TestDomain;

namespace Remotion.Data.UnitTests.Linq.Parsing.Structure.IntermediateModel
{
  [TestFixture]
  public class MethodCallExpressionNodeBaseTest : ExpressionNodeTestBase
  {
    private MethodCallExpressionNodeBase _node;
    private WhereClause _clauseToAddInApply;

    public override void SetUp ()
    {
      base.SetUp ();
      _clauseToAddInApply = new WhereClause (Expression.Constant (false));
      _node = new TestMethodCallExpressionNode (CreateParseInfo (SourceNode, "test"), _clauseToAddInApply);
    }

    [Test]
    public void Apply_LeavesQueryModel_WithoutResultOperator ()
    {
      var newQueryModel = _node.Apply (QueryModel, ClauseGenerationContext);
      Assert.That (newQueryModel, Is.SameAs (QueryModel));
    }

    [Test]
    public void Apply_CallsSpecificApply ()
    {
      var newQueryModel = _node.Apply (QueryModel, ClauseGenerationContext);
      Assert.That (newQueryModel, Is.SameAs (QueryModel));
      Assert.That (newQueryModel.BodyClauses[0], Is.SameAs (_clauseToAddInApply));
    }

    [Test]
    public void Apply_WrapsQueryModel_AfterResultOperator ()
    {
      QueryModel.ResultOperators.Add (new DistinctResultOperator ());
      var newQueryModel = _node.Apply (QueryModel, ClauseGenerationContext);

      Assert.That (newQueryModel, Is.Not.SameAs (QueryModel));
      Assert.That (newQueryModel.MainFromClause.ItemType, Is.SameAs (typeof (int))); // because SourceNode is of type int[]
      Assert.That (newQueryModel.MainFromClause.ItemName, Is.EqualTo (SourceNode.AssociatedIdentifier));
      Assert.That (newQueryModel.MainFromClause.FromExpression, Is.InstanceOfType (typeof (SubQueryExpression)));
      Assert.That (((SubQueryExpression) newQueryModel.MainFromClause.FromExpression).QueryModel, Is.SameAs (QueryModel));

      var newSelectClause = ((SelectClause) newQueryModel.SelectOrGroupClause);
      Assert.That (((QuerySourceReferenceExpression) newSelectClause.Selector).ReferencedClause, Is.SameAs (newQueryModel.MainFromClause));
    }

    [Test]
    public void Apply_WrapsQueryModel_WithCorrectResultTypes ()
    {
      QueryModel.ResultOperators.Add (new DistinctResultOperator ());
      var oldResultType = QueryModel.ResultType;

      var newQueryModel = _node.Apply (QueryModel, ClauseGenerationContext);

      Assert.That (newQueryModel, Is.Not.SameAs (QueryModel));
      Assert.That (newQueryModel.ResultType, Is.SameAs (oldResultType));
      Assert.That (QueryModel.ResultType, Is.SameAs (SourceNode.ParsedExpression.Type));
    }

    [Test]
    public void Apply_WrapsQueryModel_AfterResultOperator_BeforeApplyingNodeSpecificSemantics ()
    {
      QueryModel.ResultOperators.Add (new DistinctResultOperator ());
      var newQueryModel = _node.Apply (QueryModel, ClauseGenerationContext);

      Assert.That (newQueryModel, Is.Not.SameAs (QueryModel));
      Assert.That (newQueryModel.BodyClauses[0], Is.SameAs (_clauseToAddInApply));
    }

    [Test]
    public void Apply_WrapsQueryModel_AndEnsuresResolveWorksCorrectly ()
    {
      QueryModel.ResultOperators.Add (new DistinctResultOperator ());
      var newQueryModel = _node.Apply (QueryModel, ClauseGenerationContext);

      Expression<Func<int, string>> selector = i => i.ToString();
      var selectCall = (MethodCallExpression) ExpressionHelper.MakeExpression<IQueryable<int>, IQueryable<string>> (q => q.Select (selector));
      var selectExpressionNode = new SelectExpressionNode (new MethodCallExpressionParseInfo ("y", _node, selectCall), selector);

      selectExpressionNode.Apply (newQueryModel, ClauseGenerationContext);

      var newSelector = (MethodCallExpression) ((SelectClause) newQueryModel.SelectOrGroupClause).Selector;
      Assert.That (((QuerySourceReferenceExpression) newSelector.Object).ReferencedClause, Is.SameAs (newQueryModel.MainFromClause));
    }
  }
}
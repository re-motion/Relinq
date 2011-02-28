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
using Remotion.Linq.UnitTests.Linq.Core.Parsing.Structure.IntermediateModel.TestDomain;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Parsing.Structure.IntermediateModel;

namespace Remotion.Linq.UnitTests.Linq.Core.Parsing.Structure.IntermediateModel
{
  [TestFixture]
  public class MethodCallExpressionNodeBaseTest : ExpressionNodeTestBase
  {
    private MethodCallExpressionNodeBase _node;
    private WhereClause _clauseToAddInApply;

    private TestMethodCallExpressionNode _nodeWithResultOperatorSource;
    private QueryModel _queryModelWithResultOperator;
    private DistinctExpressionNode _resultOperatorSource;

    public override void SetUp ()
    {
      base.SetUp ();
      _clauseToAddInApply = new WhereClause (Expression.Constant (false));
      _node = new TestMethodCallExpressionNode (CreateParseInfo (SourceNode, "test"), _clauseToAddInApply);
      
      var distinctMethod = ReflectionUtility.GetMethod (() => new int[0].Distinct ());
      _resultOperatorSource = new DistinctExpressionNode (CreateParseInfo (SourceNode, "distinct", distinctMethod));
      var method = ReflectionUtility.GetMethod (() => Queryable.Take<int> (null, 0));
      _nodeWithResultOperatorSource = new TestMethodCallExpressionNode (CreateParseInfo (_resultOperatorSource, "test", method), _clauseToAddInApply);
      _queryModelWithResultOperator = QueryModel.Clone ();
      _queryModelWithResultOperator.ResultOperators.Add (new DistinctResultOperator ());
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
      var newQueryModel = _nodeWithResultOperatorSource.Apply (_queryModelWithResultOperator, ClauseGenerationContext);

      Assert.That (newQueryModel, Is.Not.SameAs (_queryModelWithResultOperator));
      Assert.That (newQueryModel.MainFromClause.ItemType, Is.SameAs (typeof (int))); // because SourceNode is of type int[]
      Assert.That (newQueryModel.MainFromClause.ItemName, Is.EqualTo ("distinct"));
      Assert.That (newQueryModel.MainFromClause.FromExpression, Is.InstanceOfType (typeof (SubQueryExpression)));
      Assert.That (((SubQueryExpression) newQueryModel.MainFromClause.FromExpression).QueryModel, Is.SameAs (_queryModelWithResultOperator));

      var newSelectClause = newQueryModel.SelectClause;
      Assert.That (((QuerySourceReferenceExpression) newSelectClause.Selector).ReferencedQuerySource, Is.SameAs (newQueryModel.MainFromClause));
    }

    [Test]
    public void Apply_WrapsQueryModel_WithCorrectResultTypes ()
    {
      var newQueryModel = _nodeWithResultOperatorSource.Apply (_queryModelWithResultOperator, ClauseGenerationContext);

      Assert.That (newQueryModel, Is.Not.SameAs (_queryModelWithResultOperator));
      Assert.That (newQueryModel.GetOutputDataInfo ().DataType, Is.SameAs (typeof (IQueryable<int>)));
      Assert.That (_queryModelWithResultOperator.GetOutputDataInfo ().DataType, Is.SameAs (typeof (int[])));
    }

    [Test]
    public void Apply_WrapsQueryModel_AfterResultOperator_BeforeApplyingNodeSpecificSemantics ()
    {
      var newQueryModel = _nodeWithResultOperatorSource.Apply (_queryModelWithResultOperator, ClauseGenerationContext);

      Assert.That (newQueryModel, Is.Not.SameAs (_queryModelWithResultOperator));
      Assert.That (newQueryModel.BodyClauses[0], Is.SameAs (_clauseToAddInApply));
    }

    [Test]
    public void Apply_WrapsQueryModel_AndEnsuresResolveWorksCorrectly ()
    {
      var newQueryModel = _nodeWithResultOperatorSource.Apply (_queryModelWithResultOperator, ClauseGenerationContext);

      Expression<Func<int, string>> selector = i => i.ToString();
      var selectCall = (MethodCallExpression) ExpressionHelper.MakeExpression<IQueryable<int>, IQueryable<string>> (q => q.Select (selector));
      var selectExpressionNode = new SelectExpressionNode (new MethodCallExpressionParseInfo ("y", _nodeWithResultOperatorSource, selectCall), selector);

      selectExpressionNode.Apply (newQueryModel, ClauseGenerationContext);

      var newSelector = (MethodCallExpression) newQueryModel.SelectClause.Selector;
      Assert.That (((QuerySourceReferenceExpression) newSelector.Object).ReferencedQuerySource, Is.SameAs (newQueryModel.MainFromClause));
    }

    [Test]
    public void Apply_SetsResultTypeOverride ()
    {
      var newQueryModel = _node.Apply (QueryModel, ClauseGenerationContext);
      Assert.That (newQueryModel.ResultTypeOverride, Is.SameAs (typeof (int)));
    }
  }
}

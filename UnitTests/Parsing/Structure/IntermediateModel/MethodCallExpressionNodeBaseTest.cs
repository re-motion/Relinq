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
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.Parsing.Structure.IntermediateModel;
using Remotion.Linq.UnitTests.Parsing.Structure.IntermediateModel.TestDomain;
using Remotion.Linq.Utilities;

namespace Remotion.Linq.UnitTests.Parsing.Structure.IntermediateModel
{
  [TestFixture]
  public class MethodCallExpressionNodeBaseTest : ExpressionNodeTestBase
  {
    private abstract class TestableMethodCallExpressionNodeBase : MethodCallExpressionNodeBase
    {
      protected TestableMethodCallExpressionNodeBase (MethodCallExpressionParseInfo parseInfo)
          : base(parseInfo)
      {
      }
    }

    private class Generic<T1, T2>
    {
      public bool NonAmbiguousMethod (int p)
      {
        return false;
      }

      public bool AmbiguousMethod (T1 p)
      {
        return false;
      }

      public bool AmbiguousMethod (T2 p)
      {
        return false;
      }
    }

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
      Assert.That (newQueryModel.MainFromClause.FromExpression, Is.InstanceOf (typeof (SubQueryExpression)));
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

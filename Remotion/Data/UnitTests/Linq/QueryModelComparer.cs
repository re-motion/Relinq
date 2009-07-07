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
using Remotion.Collections;
using Remotion.Data.Linq;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.UnitTests.Linq.Parsing;
using Remotion.Utilities;

namespace Remotion.Data.UnitTests.Linq
{
  public class QueryModelComparer : QueryModelVisitorBase
  {
    public static void CheckAreEqualModels (QueryModel expected, QueryModel actual)
    {
      ArgumentUtility.CheckNotNull ("expected", expected);
      ArgumentUtility.CheckNotNull ("actual", actual);

      var comparer = new QueryModelComparer (expected);
      actual.Accept (comparer);
    }

    private readonly QueryModel _expected;

    private QueryModelComparer (QueryModel expected)
    {
      _expected = expected;
    }

    public override void VisitQueryModel (QueryModel queryModel)
    {
      Assert.That (queryModel.ResultType, Is.EqualTo (_expected.ResultType));

      base.VisitQueryModel (queryModel);
    }

    public override void VisitMainFromClause (MainFromClause fromClause, QueryModel queryModel)
    {
      Assert.That (fromClause.GetType(), Is.SameAs (queryModel.MainFromClause.GetType()));
      Assert.That (fromClause.ItemName, Is.EqualTo (_expected.MainFromClause.ItemName));
      Assert.That (fromClause.ItemType, Is.SameAs (_expected.MainFromClause.ItemType));
      ExpressionTreeComparer.CheckAreEqualTrees (_expected.MainFromClause.FromExpression, fromClause.FromExpression);

      base.VisitMainFromClause (fromClause, queryModel);
    }

    public override void VisitAdditionalFromClause (AdditionalFromClause fromClause, QueryModel queryModel, int index)
    {
      Assert.That (fromClause.GetType(), Is.SameAs (_expected.MainFromClause.GetType()));
      Assert.That (fromClause.ItemName, Is.EqualTo (_expected.MainFromClause.ItemName));
      Assert.That (fromClause.ItemType, Is.SameAs (_expected.MainFromClause.ItemType));
      ExpressionTreeComparer.CheckAreEqualTrees (_expected.MainFromClause.FromExpression, fromClause.FromExpression);

      base.VisitAdditionalFromClause (fromClause, queryModel, index);
    }

    public override void VisitJoinClause (JoinClause joinClause, QueryModel queryModel, FromClauseBase fromClause, int index)
    {
      JoinClause expectedJoinClause;
      if (fromClause is MainFromClause)
        expectedJoinClause = _expected.MainFromClause.JoinClauses[index];
      else
      {
        var fromClauseIndex = queryModel.BodyClauses.IndexOf ((AdditionalFromClause) fromClause);
        expectedJoinClause = ((AdditionalFromClause) _expected.BodyClauses[fromClauseIndex]).JoinClauses[index];
      }

      Assert.That (joinClause.GetType(), Is.SameAs (expectedJoinClause.GetType()));
      Assert.That (joinClause.ItemName, Is.EqualTo (expectedJoinClause.ItemName));
      Assert.That (joinClause.ItemType, Is.SameAs (expectedJoinClause.ItemType));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedJoinClause.InExpression, joinClause.InExpression);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedJoinClause.OnExpression, joinClause.OnExpression);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedJoinClause.EqualityExpression, joinClause.EqualityExpression);

      base.VisitJoinClause (joinClause, queryModel, fromClause, index);
    }

    public override void VisitWhereClause (WhereClause whereClause, QueryModel queryModel, int index)
    {
      var expectedWhereClause = (WhereClause) _expected.BodyClauses[index];

      Assert.That (whereClause.GetType(), Is.SameAs (expectedWhereClause.GetType()));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedWhereClause.Predicate, whereClause.Predicate);

      base.VisitWhereClause (whereClause, queryModel, index);
    }

    public override void VisitOrderByClause (OrderByClause orderByClause, QueryModel queryModel, int index)
    {
      var expectedOrderByClause = (OrderByClause) _expected.BodyClauses[index];
      Assert.That (orderByClause.GetType(), Is.SameAs (expectedOrderByClause.GetType()));

      base.VisitOrderByClause (orderByClause, queryModel, index);
    }

    public override void VisitOrdering (Ordering ordering, QueryModel queryModel, OrderByClause orderByClause, int index)
    {
      var orderByClauseIndex = queryModel.BodyClauses.IndexOf (orderByClause);
      var expectedOrdering = ((OrderByClause) _expected.BodyClauses[orderByClauseIndex]).Orderings[index];

      Assert.That (ordering.GetType(), Is.SameAs (expectedOrdering.GetType()));
      Assert.That (ordering.OrderingDirection, Is.EqualTo (expectedOrdering.OrderingDirection));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedOrdering.Expression, ordering.Expression);

      base.VisitOrdering (ordering, queryModel, orderByClause, index);
    }

    public override void VisitSelectClause (SelectClause selectClause, QueryModel queryModel)
    {
      var expectedSelectClause = ((SelectClause) _expected.SelectOrGroupClause);
      Assert.That (selectClause.GetType(), Is.SameAs (expectedSelectClause.GetType()));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedSelectClause.Selector, selectClause.Selector);

      base.VisitSelectClause (selectClause, queryModel);
    }

    public override void VisitResultOperator (
        ResultOperatorBase resultOperator, QueryModel queryModel, SelectClause selectClause, int index)
    {
      var expectedSelectClause = ((SelectClause) _expected.SelectOrGroupClause);
      var expectedResultModification = expectedSelectClause.ResultOperators[index];

      Assert.That (resultOperator.GetType(), Is.SameAs (expectedResultModification.GetType()));
      var comparer = new ExpressionTreeComparer (expectedResultModification, resultOperator);
      comparer.CheckAreEqualObjects (expectedResultModification, resultOperator);

      base.VisitResultOperator (resultOperator, queryModel, selectClause, index);
    }

    public override void VisitGroupClause (GroupClause groupClause, QueryModel queryModel)
    {
      var expectedGroupClause = ((GroupClause) _expected.SelectOrGroupClause);
      Assert.That (groupClause.GetType(), Is.SameAs (expectedGroupClause.GetType()));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedGroupClause.GroupExpression, groupClause.GroupExpression);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedGroupClause.ByExpression, groupClause.ByExpression);

      base.VisitGroupClause (groupClause, queryModel);
    }

    protected override void VisitBodyClauses (ObservableCollection<IBodyClause> bodyClauses, QueryModel queryModel)
    {
      Assert.That (queryModel.BodyClauses.Count, Is.EqualTo (_expected.BodyClauses.Count));
      base.VisitBodyClauses (bodyClauses, queryModel);
    }

    protected override void VisitJoinClauses (ObservableCollection<JoinClause> joinClauses, QueryModel queryModel, FromClauseBase fromClause)
    {
      FromClauseBase expectedFromClause;
      if (fromClause is MainFromClause)
        expectedFromClause = _expected.MainFromClause;
      else
      {
        var fromClauseIndex = queryModel.BodyClauses.IndexOf ((AdditionalFromClause) fromClause);
        expectedFromClause = (AdditionalFromClause) _expected.BodyClauses[fromClauseIndex];
      }

      Assert.That (fromClause.JoinClauses.Count, Is.EqualTo (expectedFromClause.JoinClauses.Count));
      base.VisitJoinClauses (joinClauses, queryModel, fromClause);
    }

    protected override void VisitOrderings (ObservableCollection<Ordering> orderings, QueryModel queryModel, OrderByClause orderByClause)
    {
      var orderByClauseIndex = queryModel.BodyClauses.IndexOf (orderByClause);
      var expectedOrderByClause = (OrderByClause) _expected.BodyClauses[orderByClauseIndex];

      Assert.That (orderByClause.Orderings.Count, Is.EqualTo (expectedOrderByClause.Orderings.Count));
      base.VisitOrderings (orderings, queryModel, orderByClause);
    }

    protected override void VisitResultOperators (
        ObservableCollection<ResultOperatorBase> resultOperators, QueryModel queryModel, SelectClause selectClause)
    {
      var expectedSelectClause = (SelectClause) _expected.SelectOrGroupClause;

      Assert.That (selectClause.ResultOperators.Count, Is.EqualTo (expectedSelectClause.ResultOperators.Count));
      base.VisitResultOperators (resultOperators, queryModel, selectClause);
    }
  }
}
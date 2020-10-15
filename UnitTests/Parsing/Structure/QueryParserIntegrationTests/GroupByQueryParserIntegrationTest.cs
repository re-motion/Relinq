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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.UnitTests.TestDomain;
using Remotion.Linq.UnitTests.TestUtilities;

namespace Remotion.Linq.UnitTests.Parsing.Structure.QueryParserIntegrationTests
{
  [TestFixture]
  public class GroupByQueryParserIntegrationTest : QueryParserIntegrationTestBase
  {
    [Test]
    public void GroupBy ()
    {
      var query = (from s in QuerySource group s.ID by s.IsStarredCook);
      
      var queryModel = QueryParser.GetParsedQuery (query.Expression);
      Assert.That (queryModel.GetOutputDataInfo ().DataType, Is.SameAs (typeof (IQueryable<IGrouping<bool, int>>)));

      var mainFromClause = queryModel.MainFromClause;
      CheckConstantQuerySource (mainFromClause.FromExpression, QuerySource);
      Assert.That (mainFromClause.ItemType, Is.SameAs (typeof (Cook)));
      Assert.That (mainFromClause.ItemName, Is.EqualTo ("s"));

      var selectClause = queryModel.SelectClause;
      CheckResolvedExpression<Cook, Cook> (selectClause.Selector, mainFromClause, s => s);

      var groupResultOperator = (GroupResultOperator) queryModel.ResultOperators[0];
      CheckResolvedExpression<Cook, bool> (groupResultOperator.KeySelector, mainFromClause, s => s.IsStarredCook);
      CheckResolvedExpression<Cook, int> (groupResultOperator.ElementSelector, mainFromClause, s => s.ID);
    }

    [Test]
    public void GroupByWithoutElementSelector ()
    {
      var query = QuerySource.GroupBy (s => s.IsStarredCook);
      
      var queryModel = QueryParser.GetParsedQuery (query.Expression);
      Assert.That (queryModel.GetOutputDataInfo ().DataType, Is.SameAs (typeof (IQueryable<IGrouping<bool, Cook>>)));

      var mainFromClause = queryModel.MainFromClause;
      CheckConstantQuerySource (mainFromClause.FromExpression, QuerySource);
      Assert.That (mainFromClause.ItemType, Is.SameAs (typeof (Cook)));
      Assert.That (mainFromClause.ItemName, Is.EqualTo ("s"));

      var selectClause = queryModel.SelectClause;
      CheckResolvedExpression<Cook, Cook> (selectClause.Selector, mainFromClause, s => s);

      var groupResultOperator = (GroupResultOperator) queryModel.ResultOperators[0];
      CheckResolvedExpression<Cook, bool> (groupResultOperator.KeySelector, mainFromClause, s => s.IsStarredCook);
      CheckResolvedExpression<Cook, Cook> (groupResultOperator.ElementSelector, mainFromClause, s => s);
    }

    [Test]
    public void GroupBy_WithResultSelector_AfterElementSelector ()
    {
      var query = QuerySource.GroupBy (s => s.IsStarredCook, s => s.ID, (key, group) => new NonTransformedTuple<bool, IEnumerable<int>> (key, group));

      var queryModel = QueryParser.GetParsedQuery (query.Expression);
      Assert.That (queryModel.GetOutputDataInfo ().DataType, Is.SameAs (typeof (IQueryable<NonTransformedTuple<bool, IEnumerable<int>>>)));

      var outerMainFromClause = queryModel.MainFromClause;
      Assert.That (outerMainFromClause.FromExpression, Is.InstanceOf (typeof (SubQueryExpression)));
      Assert.That (outerMainFromClause.ItemType, Is.SameAs (typeof (IGrouping<bool, int>)));
      Assert.That (outerMainFromClause.ItemName, Is.EqualTo ("<generated>_0"));

      Assert.That (queryModel.BodyClauses.Count, Is.EqualTo (0));
      Assert.That (queryModel.ResultOperators.Count, Is.EqualTo (0));

      var outerSelectClause = queryModel.SelectClause;

      CheckResolvedExpression<IGrouping<bool, int>, NonTransformedTuple<bool, IEnumerable<int>>> (
          outerSelectClause.Selector, 
          outerMainFromClause,
          x => new NonTransformedTuple<bool, IEnumerable<int>> (x.Key, x));

      var innerQueryModel = ((SubQueryExpression) outerMainFromClause.FromExpression).QueryModel;
      Assert.That (innerQueryModel.GetOutputDataInfo ().DataType, Is.SameAs (typeof (IEnumerable<IGrouping<bool, int>>)));

      var innerMainFromClause = innerQueryModel.MainFromClause;
      CheckConstantQuerySource (innerMainFromClause.FromExpression, QuerySource);
      Assert.That (innerMainFromClause.ItemType, Is.SameAs (typeof (Cook)));
      Assert.That (innerMainFromClause.ItemName, Is.EqualTo ("s"));

      Assert.That (innerQueryModel.BodyClauses.Count, Is.EqualTo (0));

      var innerSelectClause = innerQueryModel.SelectClause;
      CheckResolvedExpression<Cook, Cook> (innerSelectClause.Selector, innerMainFromClause, s => s);

      Assert.That (innerQueryModel.ResultOperators.Count, Is.EqualTo (1));
      Assert.That (innerQueryModel.ResultOperators[0], Is.TypeOf (typeof (GroupResultOperator)));

      var groupResultOperator = ((GroupResultOperator) innerQueryModel.ResultOperators[0]);

      CheckResolvedExpression<Cook, bool> (groupResultOperator.KeySelector, innerMainFromClause, c => c.IsStarredCook);
      CheckResolvedExpression<Cook, int> (groupResultOperator.ElementSelector, innerMainFromClause, c => c.ID);
    }

    [Test]
    public void GroupBy_WithResultSelector_NoElementSelector ()
    {
      var query = QuerySource.GroupBy (s => s.IsStarredCook, (key, group) => new NonTransformedTuple<bool, IEnumerable<Cook>> (key, group));

      var queryModel = QueryParser.GetParsedQuery (query.Expression);
      Assert.That (queryModel.GetOutputDataInfo ().DataType, Is.SameAs (typeof (IQueryable<NonTransformedTuple<bool, IEnumerable<Cook>>>)));

      var outerMainFromClause = queryModel.MainFromClause;
      Assert.That (outerMainFromClause.FromExpression, Is.InstanceOf (typeof (SubQueryExpression)));
      Assert.That (outerMainFromClause.ItemType, Is.SameAs (typeof (IGrouping<bool, Cook>)));
      Assert.That (outerMainFromClause.ItemName, Is.EqualTo ("<generated>_0"));

      Assert.That (queryModel.BodyClauses.Count, Is.EqualTo (0));
      Assert.That (queryModel.ResultOperators.Count, Is.EqualTo (0));

      var outerSelectClause = queryModel.SelectClause;

      CheckResolvedExpression<IGrouping<bool, Cook>, NonTransformedTuple<bool, IEnumerable<Cook>>> (
          outerSelectClause.Selector,
          outerMainFromClause,
          x => new NonTransformedTuple<bool, IEnumerable<Cook>> (x.Key, x));

      var innerQueryModel = ((SubQueryExpression) outerMainFromClause.FromExpression).QueryModel;
      Assert.That (innerQueryModel.GetOutputDataInfo ().DataType, Is.SameAs (typeof (IEnumerable<IGrouping<bool, Cook>>)));

      var innerMainFromClause = innerQueryModel.MainFromClause;
      CheckConstantQuerySource (innerMainFromClause.FromExpression, QuerySource);
      Assert.That (innerMainFromClause.ItemType, Is.SameAs (typeof (Cook)));
      Assert.That (innerMainFromClause.ItemName, Is.EqualTo ("s"));

      Assert.That (innerQueryModel.BodyClauses.Count, Is.EqualTo (0));

      var innerSelectClause = innerQueryModel.SelectClause;
      CheckResolvedExpression<Cook, Cook> (innerSelectClause.Selector, innerMainFromClause, s => s);

      Assert.That (innerQueryModel.ResultOperators.Count, Is.EqualTo (1));
      Assert.That (innerQueryModel.ResultOperators[0], Is.TypeOf (typeof (GroupResultOperator)));

      var groupResultOperator = (GroupResultOperator) innerQueryModel.ResultOperators[0];

      CheckResolvedExpression<Cook, bool> (groupResultOperator.KeySelector, innerMainFromClause, c => c.IsStarredCook);
      CheckResolvedExpression<Cook, Cook> (groupResultOperator.ElementSelector, innerMainFromClause, c => c);
    }

    [Test]
    public void GroupBy_WithResultSelector_BeforeAll ()
    {
      var queryExpression = ExpressionHelper.MakeExpression (
          () => QuerySource
              .GroupBy (s => s.IsStarredCook, s => s.ID, (key, group) => new NonTransformedTuple<bool, int> (key, group.GetHashCode()))
              .All (kvp => kvp.Item2 > 0));

      var queryModel = QueryParser.GetParsedQuery (queryExpression);
      Assert.That (queryModel.GetOutputDataInfo ().DataType, Is.SameAs (typeof (bool)));

      var outerMainFromClause = queryModel.MainFromClause;
      Assert.That (outerMainFromClause.FromExpression, Is.InstanceOf (typeof (SubQueryExpression)));
      Assert.That (outerMainFromClause.ItemType, Is.SameAs (typeof (IGrouping<bool, int>)));
      Assert.That (outerMainFromClause.ItemName, Is.EqualTo ("kvp"));

      Assert.That (queryModel.BodyClauses.Count, Is.EqualTo (0));

      var outerSelectClause = queryModel.SelectClause;

      CheckResolvedExpression<IGrouping<bool, int>, NonTransformedTuple<bool, int>> (
          outerSelectClause.Selector,
          outerMainFromClause,
          x => new NonTransformedTuple<bool, int> (x.Key, x.GetHashCode()));

      Assert.That (queryModel.ResultOperators.Count, Is.EqualTo (1));
      Assert.That (queryModel.ResultOperators[0], Is.TypeOf (typeof (AllResultOperator)));
      
      var allResultOperator = (AllResultOperator) queryModel.ResultOperators[0];
      CheckResolvedExpression<IGrouping<bool, int>, bool> (
          allResultOperator.Predicate, 
          outerMainFromClause, 
          x => new NonTransformedTuple<bool, int> (x.Key, x.GetHashCode()).Item2 > 0);
    }

    [Test]
    public void GroupIntoWithAggregate ()
    {
      var query = from s in QuerySource 
                  group s.ID by s.IsStarredCook 
                  into x 
                      where x.Count() > 0
                      select x;

      // equivalent to:
      //var query2 = from x in
      //               (from s in _querySource
      //                group s.ID by s.IsStarredCook)
      //             where x.Count () > 0
      //             select x;

      // parsed as:
      //var query2 = from x in
      //               (from s in _querySource
      //                group s.ID by s.IsStarredCook)
      //             where (from generated in x select generated).Count () > 0
      //             select x;

      var queryModel = QueryParser.GetParsedQuery (query.Expression);
      Assert.That (queryModel.GetOutputDataInfo ().DataType, Is.SameAs (typeof (IQueryable<IGrouping<bool, int>>)));

      var mainFromClause = queryModel.MainFromClause;
      Assert.That (mainFromClause.FromExpression, Is.InstanceOf (typeof (SubQueryExpression)));
      Assert.That (mainFromClause.ItemType, Is.SameAs (typeof (IGrouping<bool, int>)));
      Assert.That (mainFromClause.ItemName, Is.EqualTo ("x"));

      var subQueryModel = ((SubQueryExpression) mainFromClause.FromExpression).QueryModel;
      var subQuerySelectClause = queryModel.SelectClause;
      CheckResolvedExpression<Cook, Cook> (subQuerySelectClause.Selector, mainFromClause, s => s);
      
      var subQueryGroupResultOperator = (GroupResultOperator) subQueryModel.ResultOperators[0];
      CheckResolvedExpression<Cook, bool> (subQueryGroupResultOperator.KeySelector, subQueryModel.MainFromClause, s => s.IsStarredCook);
      CheckResolvedExpression<Cook, int> (subQueryGroupResultOperator.ElementSelector, subQueryModel.MainFromClause, s => s.ID);
      
      Assert.That (subQueryModel.GetOutputDataInfo ().DataType, Is.SameAs (typeof (IQueryable<IGrouping<bool, int>>)));

      var whereClause = (WhereClause) queryModel.BodyClauses[0];
      Assert.That (whereClause.Predicate, Is.InstanceOf (typeof (BinaryExpression)));
      var predicateLeftSide = ((BinaryExpression) whereClause.Predicate).Left;
      Assert.That (predicateLeftSide, Is.InstanceOf (typeof (SubQueryExpression)));
      var predicateSubQueryModel = ((SubQueryExpression) predicateLeftSide).QueryModel;
      Assert.That (predicateSubQueryModel.MainFromClause.ItemType, Is.SameAs (typeof (int)));
      Assert.That (predicateSubQueryModel.MainFromClause.ItemName, Does.StartWith ("<generated>"));
      Assert.That (((QuerySourceReferenceExpression) predicateSubQueryModel.MainFromClause.FromExpression).ReferencedQuerySource, Is.SameAs (mainFromClause));
      Assert.That (((QuerySourceReferenceExpression) predicateSubQueryModel.SelectClause.Selector).ReferencedQuerySource, 
                   Is.SameAs (predicateSubQueryModel.MainFromClause));
      Assert.That (predicateSubQueryModel.ResultOperators[0], Is.InstanceOf (typeof (CountResultOperator)));
      
      var selectClause = queryModel.SelectClause;
      Assert.That (((QuerySourceReferenceExpression) selectClause.Selector).ReferencedQuerySource, Is.SameAs (mainFromClause));
    }

    [Test]
    public void GroupByFollowedByWhere ()
    {
      var query = (from s in ExpressionHelper.CreateQueryable<Cook> ()
                   group s by s.IsStarredCook).Where (g => g.Key);

      var queryModel = QueryParser.GetParsedQuery (query.Expression);
      Assert.That (queryModel.GetOutputDataInfo ().DataType, Is.SameAs (typeof (IQueryable<IGrouping<bool, Cook>>)));

      var mainFromClause = queryModel.MainFromClause;
      Assert.That (mainFromClause.FromExpression, Is.InstanceOf (typeof (SubQueryExpression)));
      Assert.That (mainFromClause.ItemType, Is.SameAs (typeof (IGrouping<bool, Cook>)));
      Assert.That (mainFromClause.ItemName, Is.EqualTo ("g"));

      var subQueryModel = ((SubQueryExpression) mainFromClause.FromExpression).QueryModel;
      var subQuerySelectClause = subQueryModel.SelectClause;
      CheckResolvedExpression<Cook, Cook> (subQuerySelectClause.Selector, subQueryModel.MainFromClause, s => s);

      Assert.That (subQueryModel.ResultOperators[0], Is.InstanceOf (typeof (GroupResultOperator)));

      Assert.That (subQueryModel.GetOutputDataInfo ().DataType, Is.SameAs (typeof (IQueryable<IGrouping<bool, Cook>>)));

      var whereClause = (WhereClause) queryModel.BodyClauses[0];
      CheckResolvedExpression<IGrouping<bool, Cook>, bool> (whereClause.Predicate, mainFromClause, g => g.Key);

      var selectClause = queryModel.SelectClause;
      Assert.That (((QuerySourceReferenceExpression) selectClause.Selector).ReferencedQuerySource, Is.SameAs (mainFromClause));
    }
  }
}

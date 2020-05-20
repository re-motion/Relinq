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
using Remotion.Linq.Transformations;
using Remotion.Linq.UnitTests.TestDomain;
using Remotion.Linq.UnitTests.TestUtilities;

namespace Remotion.Linq.UnitTests.Transformations
{
  [TestFixture]
  public class SubQueryFromClauseFlattenerTest
  {
    private QueryModel _queryModel;
    private MainFromClause _mainFromClause;
    private AdditionalFromClause _additionalFromClause1;
    private AdditionalFromClause _additionalFromClause2;
    private WhereClause _whereClause;
    private SelectClause _selectClause;

    private MainFromClause _innerMainFromClauseA;
    private WhereClause _innerWhereClauseA;

    private SubQueryFromClauseFlattener _visitor;
    private IQueryable<Kitchen> _detailSource;
    private IQueryable<Restaurant> _sectorSource;

    [SetUp]
    public void SetUp ()
    {
      _detailSource = ExpressionHelper.CreateQueryable<Kitchen>();
      _sectorSource = ExpressionHelper.CreateQueryable<Restaurant>();

      var query = from s1 in ExpressionHelper.CreateQueryable<Cook>()
                  from sd in
                      (from sector in _sectorSource
                       where sector.ID > 10
                       select sector.SubKitchen)
                  from s2 in s1.Assistants
                  where sd.Name == "Maths"
                  select new NonTransformedTuple<Cook, Kitchen> (s1, sd);
      _queryModel = ExpressionHelper.ParseQuery (query);

      _mainFromClause = _queryModel.MainFromClause;
      _additionalFromClause1 = (AdditionalFromClause) _queryModel.BodyClauses[0];
      _additionalFromClause2 = (AdditionalFromClause) _queryModel.BodyClauses[1];
      _whereClause = (WhereClause) _queryModel.BodyClauses[2];
      _selectClause = _queryModel.SelectClause;

      var subQueryExpressionA = (SubQueryExpression) _additionalFromClause1.FromExpression;
      _innerMainFromClauseA = subQueryExpressionA.QueryModel.MainFromClause;
      _innerWhereClauseA = (WhereClause) subQueryExpressionA.QueryModel.BodyClauses[0];

      _visitor = new SubQueryFromClauseFlattener();
    }

    [Test]
    public void VisitAdditionalFromClause_IgnoresNonSubQueries ()
    {
      _visitor.VisitAdditionalFromClause (_additionalFromClause2, _queryModel, 1);

      Assert.That (_queryModel.BodyClauses[1], Is.SameAs (_additionalFromClause2));
      var expectedExpression = ExpressionHelper.Resolve<Cook, IEnumerable<Cook>> (_mainFromClause, s => s.Assistants);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, _additionalFromClause2.FromExpression);
    }

    [Test]
    public void VisitAdditionalFromClause_ReplacesFromDataWithInnerMainFromData ()
    {
      _visitor.VisitAdditionalFromClause (_additionalFromClause1, _queryModel, 0);

      Assert.That (_queryModel.BodyClauses[0], Is.SameAs (_additionalFromClause1));
      Assert.That (_additionalFromClause1.FromExpression, Is.Not.InstanceOf (typeof (SubQueryExpression)));
      Assert.That (_additionalFromClause1.FromExpression, Is.SameAs (_innerMainFromClauseA.FromExpression));
      Assert.That (_additionalFromClause1.ItemName, Is.EqualTo ("sector"));
      Assert.That (_additionalFromClause1.ItemType, Is.SameAs (typeof (Restaurant)));
    }

    [Test]
    public void VisitAdditionalFromClause_PullsOutInnerBodyClauses ()
    {
      _visitor.VisitAdditionalFromClause (_additionalFromClause1, _queryModel, 0);

      Assert.That (_queryModel.BodyClauses[1], Is.SameAs (_innerWhereClauseA));
      Assert.That (_queryModel.BodyClauses[2], Is.SameAs (_additionalFromClause2));
      Assert.That (_queryModel.BodyClauses[3], Is.SameAs (_whereClause));
    }

    [Test]
    public void VisitAdditionalFromClause_AdaptsReferencesOfInnerBodyClauses ()
    {
      _visitor.VisitAdditionalFromClause (_additionalFromClause1, _queryModel, 0);

      var predicateLeftSide = (MemberExpression) ((BinaryExpression) _innerWhereClauseA.Predicate).Left;
      var referenceExpression = (QuerySourceReferenceExpression) predicateLeftSide.Expression;
      Assert.That (referenceExpression.ReferencedQuerySource, Is.SameAs (_additionalFromClause1));
    }

    [Test]
    public void VisitAdditionalFromClause_AdaptsReferencesToFromClause_WithInnerSelector ()
    {
      _visitor.VisitAdditionalFromClause (_additionalFromClause1, _queryModel, 0);

      var expectedPredicate = 
          ExpressionHelper.Resolve<Restaurant, bool> (_additionalFromClause1, sector => sector.SubKitchen.Name == "Maths");
      ExpressionTreeComparer.CheckAreEqualTrees (expectedPredicate, _whereClause.Predicate);

      var expectedSelector = ExpressionHelper.Resolve<Cook, Restaurant, NonTransformedTuple<Cook, Kitchen>> (
          _mainFromClause,
          _additionalFromClause1,
          (s1, sector) => new NonTransformedTuple<Cook, Kitchen> (s1, sector.SubKitchen));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedSelector, _selectClause.Selector);
    }

    [Test]
    public void VisitAdditionalFromClause_ThrowsOnResultOperator ()
    {
      var queryModel = ExpressionHelper.CreateQueryModel<Cook> ();
      queryModel.ResultOperators.Add (new DistinctResultOperator ());
      var clause = new AdditionalFromClause ("x", typeof (Cook), new SubQueryExpression (queryModel));
      Assert.That (
          () => _visitor.VisitAdditionalFromClause (clause, _queryModel, 0),
          Throws.InstanceOf<NotSupportedException>()
              .With.Message.EqualTo (
                  "The subquery "
                  + "'TestQueryable<Cook>() => Distinct()' cannot be flattened and pulled out of the from clause because it "
                  + "contains result operators."));
    }

    [Test]
    public void VisitAdditionalFromClause_ThrowsOnOrderBy ()
    {
      var queryModel = ExpressionHelper.CreateQueryModel<Cook> ();
      var orderByClause = new OrderByClause ();
      orderByClause.Orderings.Add (new Ordering (Expression.Constant (0), OrderingDirection.Asc));
      queryModel.BodyClauses.Add (orderByClause);
      var clause = new AdditionalFromClause ("x", typeof (Cook), new SubQueryExpression (queryModel));
      Assert.That (
          () => _visitor.VisitAdditionalFromClause (clause, _queryModel, 0),
          Throws.InstanceOf<NotSupportedException>()
              .With.Message.EqualTo (
                  "The subquery "
                  + "'from Cook s in TestQueryable<Cook>() orderby 0 asc select [s]' cannot be flattened and pulled out of the from clause because it "
                  + "contains an OrderByClause."));
    }

    [Test]
    public void VisitMainFromClause_AlsoFlattens ()
    {
      var mainFromSubQuery = from sd in _detailSource
                             where sd.Name == "Maths"
                             select sd.Cook;
      var parsedMainFromSubQuery = ExpressionHelper.ParseQuery (mainFromSubQuery);

      var query = from s in ExpressionHelper.CreateQueryable<Cook>()
                  select s.FirstName;
      var parsedQuery = ExpressionHelper.ParseQuery (query);
      parsedQuery.MainFromClause.FromExpression = new SubQueryExpression (parsedMainFromSubQuery);

      parsedQuery.Accept (_visitor);

      var expectedSelector = ExpressionHelper.Resolve<Kitchen, string> (parsedQuery.MainFromClause, sd => sd.Cook.FirstName);

      Assert.That (parsedQuery.MainFromClause.FromExpression, Is.Not.InstanceOf (typeof (SubQueryExpression)));
      Assert.That (parsedQuery.BodyClauses.Count, Is.EqualTo (1));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedSelector, parsedQuery.SelectClause.Selector);
    }

    [Test]
    public void IntegrationTest_TransformedQueryModel ()
    {
      var query = from s1 in ExpressionHelper.CreateQueryable<Cook> ()
                  from sd in
                    (from sector in _sectorSource
                     where sector.ID > 10
                     select sector.SubKitchen)
                  from s2 in s1.Assistants
                  where sd.Name == "Maths"
                  from s3 in
                    (from a in s1.Assistants
                     from b in sd.Cook.Assistants
                     select new NonTransformedTuple<Cook, Cook> (a, b))
                  select new NonTransformedTuple<Cook, Kitchen, Cook> (s1, sd, s3.Item1);

      var queryModel = ExpressionHelper.ParseQuery (query);
      var mainFromSubQuery = from sd in _detailSource
                             where sd.Name == "Maths"
                             select sd.Cook;
      var parsedMainFromSubQuery = ExpressionHelper.ParseQuery (mainFromSubQuery);
      queryModel.MainFromClause.FromExpression = new SubQueryExpression (parsedMainFromSubQuery);

      queryModel.Accept (_visitor);

      var expectedQuery = from sd in _detailSource
                          where sd.Name == "Maths"
                          from sector in _sectorSource
                          where sector.ID > 10
                          from s2 in sd.Cook.Assistants
                          where sector.SubKitchen.Name == "Maths"
                          from a in sd.Cook.Assistants
                          from b in sector.SubKitchen.Cook.Assistants
                          select new NonTransformedTuple<Cook, Kitchen, Cook> (
                              sd.Cook, 
                              sector.SubKitchen, 
                              new NonTransformedTuple<Cook, Cook> (a, b).Item1);

      var expectedQueryModel = ExpressionHelper.ParseQuery (expectedQuery);
      Assert.That (expectedQueryModel.ToString(), Is.EqualTo (queryModel.ToString()));
    }
  }
}

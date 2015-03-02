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
#if !NET_3_5
using System;
using System.Linq;
using NUnit.Framework;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.UnitTests.TestDomain;

namespace Remotion.Linq.UnitTests.Parsing.Structure.QueryParserIntegrationTests
{
  [TestFixture]
  public class CovariantQueryParserIntegrationTest : QueryParserIntegrationTestBase
  {
    [Test]
    public void CovariantQueryTest ()
    {
      IQueryable<object> queryable = QuerySource.Where (c => c.Name != "john");
      var queryExpression = ExpressionHelper.MakeExpression (() => (queryable.Distinct ()));

      var queryModel = QueryParser.GetParsedQuery (queryExpression);
      var outputDataInfo = (StreamedSequenceInfo) queryModel.GetOutputDataInfo ();
      Assert.That (outputDataInfo.DataType, Is.SameAs (typeof (IQueryable<object>)));
      Assert.That (outputDataInfo.ItemExpression.Type, Is.SameAs (typeof (Cook)));
    }

    [Test]
    public void CovariantSubqueryTest ()
    {
      IQueryable<object> subQueryable = (QuerySource.Select (c => c)).Distinct ();
      var queryExpression = ExpressionHelper.MakeExpression (() => (
          from o in subQueryable
          select o));
      
      var queryModel = QueryParser.GetParsedQuery (queryExpression);
      Assert.That (queryModel.GetOutputDataInfo().DataType, Is.SameAs (typeof (IQueryable<object>)));

      var subQuery = ((SubQueryExpression) queryModel.MainFromClause.FromExpression).QueryModel;

      var selectOutputInfo = subQuery.SelectClause.GetOutputDataInfo ();
      Assert.That (selectOutputInfo.ItemExpression.Type, Is.SameAs (typeof (Cook)));
      Assert.That (selectOutputInfo.ResultItemType, Is.SameAs (typeof (Cook)));
      Assert.That (selectOutputInfo.DataType, Is.SameAs (typeof (IQueryable<Cook>)));

      var distinctOperator = (DistinctResultOperator) subQuery.ResultOperators.Single ();
      var distinctOperatorOutputInfo = (StreamedSequenceInfo) distinctOperator.GetOutputDataInfo (selectOutputInfo);
      Assert.That (distinctOperatorOutputInfo.ItemExpression.Type, Is.SameAs (typeof (Cook)));
      Assert.That (distinctOperatorOutputInfo.ResultItemType, Is.SameAs (typeof (Cook)));
      Assert.That (distinctOperatorOutputInfo.DataType, Is.SameAs (typeof (IQueryable<Cook>)));

      var queryOutputInfo = (StreamedSequenceInfo) subQuery.GetOutputDataInfo();
      Assert.That (queryOutputInfo.ItemExpression.Type, Is.SameAs (typeof (Cook)));
      Assert.That (queryOutputInfo.ResultItemType, Is.SameAs (typeof (Cook)));
      Assert.That (queryOutputInfo.DataType, Is.SameAs (typeof (IQueryable<Cook>)));
    }

    [Test]
    public void Aggregate_NoSeed_MoreGeneralFunc ()
    {
      var expression = ExpressionHelper.MakeExpression (
          () => (from s in QuerySource
                 select s.Name).Aggregate<IComparable> ((allNames, name) => allNames + name.ToString ()));

      var queryModel = QueryParser.GetParsedQuery (expression);
      Assert.That (queryModel.GetOutputDataInfo ().DataType, Is.SameAs (typeof (IComparable)));

      CheckConstantQuerySource (queryModel.MainFromClause.FromExpression, QuerySource);

      Assert.That (queryModel.ResultOperators.Count, Is.EqualTo (1));
      Assert.That (queryModel.ResultOperators[0], Is.InstanceOf (typeof (AggregateResultOperator)));

      var resultOperator = (AggregateResultOperator) queryModel.ResultOperators[0];

      var expectedFunc = ExpressionHelper.ResolveLambdaParameter<IComparable, Cook, string> (
          1,
          queryModel.MainFromClause,
          (allNames, cook) => allNames + cook.Name.ToString ());
      ExpressionTreeComparer.CheckAreEqualTrees (expectedFunc, resultOperator.Func);
    }
  }
}
#endif

// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (c) rubicon IT GmbH, www.rubicon.eu
// 
// re-linq is free software; you can redistribute it and/or modify it under 
// the terms of the GNU Lesser General Public License as published by the 
// Free Software Foundation; either version 2.1 of the License, 
// or (at your option) any later version.
// 
// re-linq is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-linq; if not, see http://www.gnu.org/licenses.
// 

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

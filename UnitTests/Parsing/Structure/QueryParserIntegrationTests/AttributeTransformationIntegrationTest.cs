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
using Remotion.Linq.UnitTests.TestDomain;

namespace Remotion.Linq.UnitTests.Parsing.Structure.QueryParserIntegrationTests
{
  [TestFixture]
  public class AttributeTransformationIntegrationTest : QueryParserIntegrationTestBase
  {
    [Test]
    public void Transformation_ViaAttributedInstanceMethod ()
    {
      var query = QuerySource.Select (c => c.GetFullName());

      var queryModel = QueryParser.GetParsedQuery (query.Expression);

      var selectClause = queryModel.SelectClause;

      CheckResolvedExpression<Cook, string> (selectClause.Selector, queryModel.MainFromClause, c => c.FirstName + " " + c.Name);
    }

    [Test]
    public void Transformation_ViaAttributedInstanceProperty ()
    {
      var query = QuerySource.Select (c => c.WeightInLbs);

      var queryModel = QueryParser.GetParsedQuery (query.Expression);

      var selectClause = queryModel.SelectClause;

      CheckResolvedExpression<Cook, double> (selectClause.Selector, queryModel.MainFromClause, c => c.Weight * 2.20462262);
    }

    [Test]
    public void Transformation_Recursively ()
    {
      var query = from c in QuerySource
                  from a in c.Assistants
                  select c.Equals (a);

      var queryModel = QueryParser.GetParsedQuery (query.Expression);

      var selectClause = queryModel.SelectClause;

      CheckResolvedExpression<Cook, Cook, bool> (
          selectClause.Selector,
          queryModel.MainFromClause,
          (AdditionalFromClause) queryModel.BodyClauses[0],
          (c, a) => (c.FirstName + " " + c.Name) == (a.FirstName + " " + a.Name));
    }

    [Test]
    public void Transformation_IntroducingSubQuery ()
    {
      var query = QuerySource.Select (c => c.GetAssistantCount());

      var queryModel = QueryParser.GetParsedQuery (query.Expression);

      var selectClause = queryModel.SelectClause;

      Assert.That (selectClause.Selector, Is.TypeOf<SubQueryExpression>());
      var subQueryModel = ((SubQueryExpression) selectClause.Selector).QueryModel;

      CheckResolvedExpression<Cook, IQueryable<Cook>> (subQueryModel.MainFromClause.FromExpression, queryModel.MainFromClause, c => c.Assistants);
      Assert.That (subQueryModel.ResultOperators, Has.Some.TypeOf<CountResultOperator>());
    }

    [Test]
    public void Transformation_ViaAttributedIndexedProperty ()
    {
      var query = QuerySource.Select (c => c[7]);

      var queryModel = QueryParser.GetParsedQuery (query.Expression);

      var selectClause = queryModel.SelectClause;

      Assert.That (selectClause.Selector, Is.TypeOf<SubQueryExpression>());
      var subQueryModel = ((SubQueryExpression) selectClause.Selector).QueryModel;

      CheckResolvedExpression<Cook, IQueryable<Cook>> (subQueryModel.MainFromClause.FromExpression, queryModel.MainFromClause, c => c.Assistants);
      Assert.That (subQueryModel.ResultOperators[0], Is.TypeOf<TakeResultOperator> ());
      Assert.That (
          ((TakeResultOperator) subQueryModel.ResultOperators[0]).Count, Is.InstanceOf<ConstantExpression>().With.Property ("Value").EqualTo (7));
      Assert.That (subQueryModel.ResultOperators[1], Is.TypeOf<FirstResultOperator>());
    }
  }
}

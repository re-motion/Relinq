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
using Remotion.Linq.UnitTests.Linq.Core.TestDomain;

namespace Remotion.Linq.UnitTests.Linq.Core.Parsing.Structure.QueryParserIntegrationTests
{
  [TestFixture]
  [Ignore ("TODO 5302")]
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
      var other = new Cook();
      var query = QuerySource.Select (c => c.Equals (other));

      var queryModel = QueryParser.GetParsedQuery (query.Expression);

      var selectClause = queryModel.SelectClause;

      CheckResolvedExpression<Cook, bool> (
          selectClause.Selector, queryModel.MainFromClause, c => (c.FirstName + " " + c.Name) == (other.FirstName + " " + other.Name));
    }

  }
}

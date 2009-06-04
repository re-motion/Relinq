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
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Parsing.Structure;
using Remotion.Data.UnitTests.Linq.TestQueryGenerators;

namespace Remotion.Data.UnitTests.Linq.Parsing.Structure
{
  [TestFixture]
  public class QueryParserIntegrationTest
  {
    private IQueryable<Student> _querySource;
    private QueryParser _queryParser;

    [SetUp]
    public void SetUp ()
    {
      _querySource = ExpressionHelper.CreateQuerySource();
      _queryParser = new QueryParser();
    }

    [Test]
    public void SimpleWhere ()
    {
      var query = WhereTestQueryGenerator.CreateSimpleWhereQuery (_querySource);
      var queryModel = _queryParser.GetParsedQuery (query.Expression);

      Assert.That (queryModel.MainFromClause.Identifier.Name, Is.EqualTo ("s"));
      Assert.That (queryModel.MainFromClause.JoinClauses, Is.Empty);
      Assert.That (queryModel.MainFromClause.PreviousClause, Is.Null);

      var selectClause = (SelectClause) queryModel.SelectOrGroupClause;
      var whereClause = (WhereClause) queryModel.BodyClauses[0];
      Assert.That (selectClause.PreviousClause, Is.EqualTo (whereClause));

      Assert.That (whereClause.PreviousClause, Is.EqualTo (queryModel.MainFromClause));
    }
  }
}
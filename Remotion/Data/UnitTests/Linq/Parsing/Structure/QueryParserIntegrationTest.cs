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
using System.Linq.Expressions;
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

    [Test]
    public void ThreeWheres ()
    {
      var query = WhereTestQueryGenerator.CreateMultiWhereQuery (_querySource);
      var queryModel = _queryParser.GetParsedQuery (query.Expression);
      
      var selectClause = (SelectClause) queryModel.SelectOrGroupClause;
      var whereClause1 = (WhereClause) queryModel.BodyClauses[0];
      var whereClause2 = (WhereClause) queryModel.BodyClauses[1];
      var whereClause3 = (WhereClause) queryModel.BodyClauses[2];
      
      Assert.That (queryModel.MainFromClause.PreviousClause, Is.Null);
      
      Assert.That (queryModel.BodyClauses.Count, Is.EqualTo (3));
      Assert.That (whereClause1.PreviousClause, Is.EqualTo (queryModel.MainFromClause));
      Assert.That (whereClause2.PreviousClause, Is.EqualTo (whereClause1));
      Assert.That (selectClause.PreviousClause, Is.EqualTo (whereClause3));
      Assert.That (whereClause3.PreviousClause, Is.EqualTo (whereClause2));
    }

    [Test]
    public void WhereWithDifferentComparisons ()
    {
      var query = WhereTestQueryGenerator.CreateWhereQueryWithDifferentComparisons (_querySource);
      var expression = query.Expression;
      var queryModel = _queryParser.GetParsedQuery (expression);

      var selectClause = (SelectClause) queryModel.SelectOrGroupClause;
      var whereClause = (WhereClause) queryModel.BodyClauses[0];

      Assert.That (queryModel.MainFromClause.PreviousClause, Is.Null);
      Assert.That (selectClause.PreviousClause, Is.EqualTo (whereClause));
      Assert.That (whereClause.PreviousClause, Is.EqualTo (queryModel.MainFromClause));

      var operand = ((UnaryExpression)((MethodCallExpression)expression).Arguments[1]).Operand;
      Assert.That (whereClause.Predicate, Is.SameAs (operand));
    }
  }
}
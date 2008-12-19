// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2008 rubicon informationstechnologie gmbh, www.rubicon.eu
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
using Remotion.Data.Linq.Clauses;
using Remotion.Data.UnitTests.Linq.ParsingTest.StructureTest.QueryParserIntegrationTest;
using Remotion.Data.UnitTests.Linq.TestQueryGenerators;

namespace Remotion.Data.UnitTests.Linq.ParsingTest.StructureTest.QueryParserIntegrationTest
{
  [TestFixture]
  public class SimpleWhereQueryTest : SimpleQueryTest
  {
    protected override System.Linq.IQueryable<Student> CreateQuery ()
    {
      return WhereTestQueryGenerator.CreateSimpleWhereQuery(QuerySource);
    }

    [Test]
    public override void CheckBodyClauses ()
    {
      Assert.AreEqual (1, ParsedQuery.BodyClauses.Count);
      WhereClause whereClause = ParsedQuery.BodyClauses.First() as WhereClause;
      Assert.IsNotNull (whereClause);

      ExpressionTreeNavigator navigator = new ExpressionTreeNavigator (whereClause.BoolExpression);
      Assert.IsNotNull (whereClause.BoolExpression);
      Assert.IsInstanceOfType (typeof (LambdaExpression), whereClause.BoolExpression);
      Assert.AreSame (ParsedQuery.MainFromClause.Identifier, navigator.Parameters[0].Expression);
    }

    [Test]
    public override void CheckSelectOrGroupClause ()
    {
      Assert.IsNotNull (ParsedQuery.SelectOrGroupClause);
      SelectClause clause = ParsedQuery.SelectOrGroupClause as SelectClause;
      Assert.IsNotNull (clause);
      Assert.IsNull (clause.ProjectionExpression);
    }
  }
}

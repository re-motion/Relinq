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
using System.Linq;
using NUnit.Framework;
using Remotion.Data.Linq;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.UnitTests.Linq.TestQueryGenerators;

namespace Remotion.Data.UnitTests.Linq.ParsingTest.StructureTest.QueryParserIntegrationTest
{
  [TestFixture]
  public class SimpleSubQueryInAdditionalFromClauseTest : QueryTestBase<Student>
  {
    protected override IQueryable<Student> CreateQuery ()
    {
      return SubQueryTestQueryGenerator.CreateSimpleSubQueryInAdditionalFromClause (QuerySource);
    }

    [Test]
    public override void CheckBodyClauses ()
    {
      Assert.AreEqual (1, ParsedQuery.BodyClauses.Count);

      SubQueryFromClause fromClause1 = ParsedQuery.BodyClauses[0] as SubQueryFromClause;
      Assert.IsNotNull (fromClause1);
      AssertEquivalent (SourceExpressionNavigator.Arguments[1].Operand.Body.Expression, fromClause1.SubQueryModel.GetExpressionTree ());
      AssertEquivalent (SourceExpressionNavigator.Arguments[2].Operand.Expression, fromClause1.ProjectionExpression);

      CheckSubQuery (fromClause1.SubQueryModel);
    }

    private void CheckSubQuery (QueryModel subQuery)
    {
      ExpressionTreeNavigator subExpressionNavigator = SourceExpressionNavigator.Arguments[1].Operand.Body;

      Assert.IsNotNull (subQuery.MainFromClause);
      AssertEquivalent (subExpressionNavigator.Arguments[0].Expression, subQuery.MainFromClause.QuerySource);
      AssertEquivalent (subExpressionNavigator.Arguments[1].Operand.Parameters[0].Expression, subQuery.MainFromClause.Identifier);

      Assert.AreEqual (0, subQuery.BodyClauses.Count);

      SelectClause selectClause = subQuery.SelectOrGroupClause as SelectClause;
      Assert.IsNotNull (selectClause);
      AssertEquivalent (subExpressionNavigator.Arguments[1].Operand.Expression, selectClause.ProjectionExpression);
    }

    [Test]
    public override void CheckSelectOrGroupClause ()
    {
      SelectClause selectClause = ParsedQuery.SelectOrGroupClause as SelectClause;
      Assert.IsNotNull (selectClause);
      AssertEquivalent (SourceExpressionNavigator.Arguments[2].Operand.Expression, selectClause.ProjectionExpression);
    }

    [Test]
    public override void OutputResult ()
    {
      base.OutputResult ();
    }
    
  }
}

/* Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh
 *
 * This program is free software: you can redistribute it and/or modify it under 
 * the terms of the re:motion license agreement in license.txt. If you did not 
 * receive it, please visit http://www.re-motion.org/licensing.
 * 
 * Unless otherwise provided, this software is distributed on an "AS IS" basis, 
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 */

using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.UnitTests.Linq.TestQueryGenerators;

namespace Remotion.Data.UnitTests.Linq.ParsingTest.StructureTest.QueryParserIntegrationTest
{
  [TestFixture]
  public class SelectWhereQueryTest : QueryTestBase<string>
  {
    protected override IQueryable<string> CreateQuery ()
    {
      return WhereTestQueryGenerator.CreateSelectWhereQuery (QuerySource);
    }

    [Test]
    public override void CheckBodyClauses ()
    {
      Assert.AreEqual (1, ParsedQuery.BodyClauses.Count);
      WhereClause whereClause = ParsedQuery.BodyClauses.First () as WhereClause;
      Assert.IsNotNull (whereClause);

      Assert.AreSame (SourceExpressionNavigator.Arguments[0].Arguments[1].Operand.Expression, whereClause.BoolExpression);
    }

    [Test]
    public override void CheckSelectOrGroupClause ()
    {
      Assert.IsNotNull (ParsedQuery.SelectOrGroupClause);
      SelectClause clause = ParsedQuery.SelectOrGroupClause as SelectClause;
      Assert.IsNotNull (clause);
      Assert.IsNotNull (clause.ProjectionExpression);
      Assert.IsInstanceOfType (typeof (MemberExpression), clause.ProjectionExpression.Body,
          "from s in ... select s.First => select expression must be member access");
      Assert.AreEqual ("First", ((MemberExpression) clause.ProjectionExpression.Body).Member.Name,
          "from s in ... select s.First => select expression must be access to First member");
    }
  }
}

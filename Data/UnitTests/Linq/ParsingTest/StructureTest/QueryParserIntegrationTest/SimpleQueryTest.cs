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
using NUnit.Framework;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.UnitTests.Linq.TestQueryGenerators;

namespace Remotion.Data.UnitTests.Linq.ParsingTest.StructureTest.QueryParserIntegrationTest
{
  [TestFixture]
  public class SimpleQueryTest : QueryTestBase<Student>
  {
    protected override IQueryable<Student> CreateQuery ()
    {
      return SelectTestQueryGenerator.CreateSimpleQuery (QuerySource);
    }

    [Test]
    public override void CheckBodyClauses ()
    {
      Assert.AreEqual (0, ParsedQuery.BodyClauses.Count);
    }

    [Test]
    public override void CheckSelectOrGroupClause ()
    {
      Assert.IsNotNull (ParsedQuery.SelectOrGroupClause);
      SelectClause clause = ParsedQuery.SelectOrGroupClause as SelectClause;
      Assert.IsNotNull (clause);
      Assert.IsNotNull (clause.ProjectionExpression);
      Assert.AreSame (ParsedQuery.MainFromClause.Identifier, clause.ProjectionExpression.Body,
          "from s in ... select s => select expression must be same as from-identifier");
    }
  }
}

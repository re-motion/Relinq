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
  public class ThreeFromWhereQueryTest : QueryTestBase<Student>
  {
    private IQueryable<Student> _querySource2;
    private IQueryable<Student> _querySource3;

    public override void SetUp ()
    {
      _querySource2 = ExpressionHelper.CreateQuerySource();
      _querySource3 = ExpressionHelper.CreateQuerySource ();
      base.SetUp ();
    }

    protected override IQueryable<Student> CreateQuery ()
    {
      return MixedTestQueryGenerator.CreateThreeFromWhereQuery (QuerySource, _querySource2, _querySource3);
    }

    [Test]
    public override void CheckMainFromClause ()
    {
      Assert.IsNotNull (ParsedQuery.MainFromClause);
      Assert.AreEqual ("s1", ParsedQuery.MainFromClause.Identifier.Name);
      Assert.AreSame (typeof (Student), ParsedQuery.MainFromClause.Identifier.Type);
      ExpressionTreeComparer.CheckAreEqualTrees (QuerySourceExpression, ParsedQuery.MainFromClause.QuerySource);
      Assert.AreEqual (0, ParsedQuery.MainFromClause.JoinClauses.Count);
    }


    [Test]
    public override void CheckBodyClauses ()
    {
      Assert.AreEqual (3, ParsedQuery.BodyClauses.Count);
      AdditionalFromClause fromClause1 = ParsedQuery.BodyClauses.First() as AdditionalFromClause;
      Assert.IsNotNull (fromClause1);
      AssertEquivalent (SourceExpressionNavigator.Arguments[0].Arguments[0].Arguments[1].Operand.Expression, fromClause1.FromExpression);
      AssertEquivalent (SourceExpressionNavigator.Arguments[0].Arguments[0].Arguments[2].Operand.Expression, fromClause1.ProjectionExpression);

      WhereClause whereClause = ParsedQuery.BodyClauses.Skip (1).First() as WhereClause;
      Assert.IsNotNull (whereClause);
      AssertEquivalent (SourceExpressionNavigator.Arguments[0].Arguments[1].Operand.Expression, whereClause.BoolExpression);

      AdditionalFromClause fromClause2 = ParsedQuery.BodyClauses.Last () as AdditionalFromClause;
      Assert.IsNotNull (fromClause2);
      AssertEquivalent (SourceExpressionNavigator.Arguments[1].Operand.Expression, fromClause2.FromExpression);
      AssertEquivalent (SourceExpressionNavigator.Arguments[2].Operand.Expression, fromClause2.ProjectionExpression);
    }



    
    [Test]
    public override void CheckSelectOrGroupClause ()
    {
      Assert.IsNotNull (ParsedQuery.SelectOrGroupClause);
      SelectClause clause = ParsedQuery.SelectOrGroupClause as SelectClause;
      Assert.IsNotNull (clause);
      Assert.IsNotNull (clause.ProjectionExpression);

      AssertEquivalent (SourceExpressionNavigator.Arguments[2].Operand.Expression, clause.ProjectionExpression);
    }
  }
}

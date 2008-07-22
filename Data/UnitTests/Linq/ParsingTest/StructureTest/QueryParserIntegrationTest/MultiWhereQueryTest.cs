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
  public class MultiWhereQueryTest : QueryTestBase<Student>
  {
    
    protected override IQueryable<Student> CreateQuery ()
    {
      return WhereTestQueryGenerator.CreateMultiWhereQuery (QuerySource);
    }

    [Test]
    public override void CheckBodyClauses ()
    {
      Assert.AreEqual (3, ParsedQuery.BodyClauses.Count);
      Assert.IsNotNull (ParsedQuery.BodyClauses);
      WhereClause[] whereClauses = ParsedQuery.BodyClauses.Cast<WhereClause>().ToArray();

      ExpressionTreeNavigator navigator = new ExpressionTreeNavigator (whereClauses[0].BoolExpression);

      Assert.IsNotNull (whereClauses[0].BoolExpression);
      Assert.IsInstanceOfType (typeof (LambdaExpression), whereClauses[0].BoolExpression);
      Assert.AreSame (ParsedQuery.MainFromClause.Identifier, navigator.Parameters[0].Expression);
      Assert.IsInstanceOfType (typeof (BinaryExpression), whereClauses[0].BoolExpression.Body);
      Assert.AreEqual ("Garcia", navigator.Body.Right.Value);

      navigator = new ExpressionTreeNavigator (whereClauses[1].BoolExpression);

      Assert.IsNotNull (whereClauses[1].BoolExpression);
      Assert.IsInstanceOfType (typeof (LambdaExpression), whereClauses[1].BoolExpression);
      Assert.IsInstanceOfType (typeof (BinaryExpression), whereClauses[1].BoolExpression.Body);
      Assert.AreEqual ("Hugo", navigator.Body.Right.Value);

      navigator = new ExpressionTreeNavigator (whereClauses[2].BoolExpression);

      Assert.IsNotNull (whereClauses[2].BoolExpression);
      Assert.IsInstanceOfType (typeof (LambdaExpression), whereClauses[2].BoolExpression);
      Assert.IsInstanceOfType (typeof (BinaryExpression), whereClauses[2].BoolExpression.Body);
      Assert.AreEqual (100, navigator.Body.Right.Value);
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

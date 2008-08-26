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
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq;
using Remotion.Data.UnitTests.Linq;
using Rhino.Mocks;
using Remotion.Data.Linq.Clauses;

namespace Remotion.Data.UnitTests.Linq.ClausesTest
{
  [TestFixture]
  public class AdditionalFromClauseTest
  {
    [Test]
    public void Initialize_WithIDAndExpression ()
    {
      ParameterExpression id = ExpressionHelper.CreateParameterExpression ();
      LambdaExpression fromExpression = ExpressionHelper.CreateLambdaExpression ();
      LambdaExpression projectionExpression = ExpressionHelper.CreateLambdaExpression ();

      IClause clause = ExpressionHelper.CreateClause();
      
      var fromClause = new AdditionalFromClause (clause,id, fromExpression, projectionExpression);

      Assert.AreSame (id, fromClause.Identifier);
      Assert.AreSame (fromExpression, fromClause.FromExpression);
      Assert.AreSame (projectionExpression, fromClause.ProjectionExpression);
      Assert.AreSame (clause, fromClause.PreviousClause);

      Assert.That (fromClause.JoinClauses, Is.Empty);
      Assert.AreEqual (0, fromClause.JoinClauses.Count);
    }

    [Test]
    public void ImplementInterface_IFromLetWhereClause ()
    {
      AdditionalFromClause fromClause = ExpressionHelper.CreateAdditionalFromClause ();
      Assert.IsInstanceOfType (typeof (IBodyClause), fromClause);
    }

    [Test]
    public void GetQueriedEntityType ()
    {
      IQueryable<Student> querySource = ExpressionHelper.CreateQuerySource();
      LambdaExpression fromExpression = Expression.Lambda (Expression.Constant (querySource), Expression.Parameter (typeof (Student), "student"));
      var fromClause =
          new AdditionalFromClause (ExpressionHelper.CreateClause(), ExpressionHelper.CreateParameterExpression(), 
          fromExpression, ExpressionHelper.CreateLambdaExpression());
      Assert.AreSame (typeof (TestQueryable<Student>), fromClause.GetQuerySourceType());
    }

    [Test]
    public void Accept ()
    {
      AdditionalFromClause fromClause = ExpressionHelper.CreateAdditionalFromClause ();

      var repository = new MockRepository ();
      var visitorMock = repository.StrictMock<IQueryVisitor> ();

      visitorMock.VisitAdditionalFromClause (fromClause);

      repository.ReplayAll ();

      fromClause.Accept (visitorMock);

      repository.VerifyAll ();

    }

    [Test]
    public void QueryModelAtInitialization ()
    {
      AdditionalFromClause additionalFromClause = ExpressionHelper.CreateAdditionalFromClause ();
      Assert.IsNull (additionalFromClause.QueryModel);
    }

    [Test]
    public void SetQueryModel ()
    {
      AdditionalFromClause additionalFromClause = ExpressionHelper.CreateAdditionalFromClause ();
      QueryModel model = ExpressionHelper.CreateQueryModel ();
      additionalFromClause.SetQueryModel (model);
      Assert.IsNotNull (additionalFromClause.QueryModel);
    }

    [Test]
    [ExpectedException (typeof (ArgumentNullException))]
    public void SetQueryModelWithNull_Exception ()
    {
      AdditionalFromClause additionalFromClause = ExpressionHelper.CreateAdditionalFromClause ();
      additionalFromClause.SetQueryModel (null);
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "QueryModel is already set")]
    public void SetQueryModelTwice_Exception ()
    {
      AdditionalFromClause additionalFromClause = ExpressionHelper.CreateAdditionalFromClause ();     
      QueryModel model = ExpressionHelper.CreateQueryModel ();
      additionalFromClause.SetQueryModel (model);
      additionalFromClause.SetQueryModel (model);
    }
  }
}

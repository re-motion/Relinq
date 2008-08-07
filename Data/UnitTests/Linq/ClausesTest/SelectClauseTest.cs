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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Data.Linq;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.UnitTests.Linq.TestQueryGenerators;
using Rhino.Mocks;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.DataObjectModel;

namespace Remotion.Data.UnitTests.Linq.ClausesTest
{
  [TestFixture]
  public class SelectClauseTest
  {
    [Test]
    public void InitializeWithExpression ()
    {
      LambdaExpression expression = ExpressionHelper.CreateLambdaExpression ();
      IClause clause = ExpressionHelper.CreateClause();

      SelectClause selectClause = new SelectClause (clause, expression, null);
      Assert.AreSame (clause, selectClause.PreviousClause);
      Assert.AreEqual (expression, selectClause.ProjectionExpression);
    }

    [Test]
    public void InitializeWithExpression_New ()
    {
      LambdaExpression expression = ExpressionHelper.CreateLambdaExpression ();
      IClause clause = ExpressionHelper.CreateClause ();
      var query = SelectTestQueryGenerator.CreateSimpleQuery (ExpressionHelper.CreateQuerySource());
      var methodInfo = ParserUtility.GetMethod (() => Enumerable.Count (query));
      MethodCallExpression methodCallExpression = Expression.Call (methodInfo, query.Expression);
      List<MethodCallExpression> methodCallExpressions = new List<MethodCallExpression>();
      methodCallExpressions.Add (methodCallExpression);
      
      SelectClause selectClause = new SelectClause (clause, expression, methodCallExpressions);
      Assert.AreSame (clause, selectClause.PreviousClause);
      Assert.AreEqual (expression, selectClause.ProjectionExpression);
    }

    [Test]
    public void InitializeWithoutExpression ()
    {
      SelectClause selectClause = new SelectClause (ExpressionHelper.CreateClause (), null, null);
      Assert.IsNull (selectClause.ProjectionExpression);
    }
    
    [Test]
    public void SelectClause_ImplementISelectGroupClause()
    {
      SelectClause selectClause = ExpressionHelper.CreateSelectClause();

      Assert.IsInstanceOfType (typeof(ISelectGroupClause),selectClause);
    }
        
    [Test]
    public void SelectClause_ImplementIQueryElement()
    {
      SelectClause selectClause = ExpressionHelper.CreateSelectClause();
      Assert.IsInstanceOfType (typeof (IQueryElement), selectClause);
    }

    [Test]
    public void Accept()
    {
      SelectClause selectClause = ExpressionHelper.CreateSelectClause();

      MockRepository repository = new MockRepository();
      IQueryVisitor visitorMock = repository.CreateMock<IQueryVisitor>();

      visitorMock.VisitSelectClause (selectClause);

      repository.ReplayAll();

      selectClause.Accept (visitorMock);

      repository.VerifyAll();
    }
  }
}

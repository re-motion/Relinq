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
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Data.Linq;
using Rhino.Mocks;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.DataObjectModel;

namespace Remotion.Data.UnitTests.Linq.ClausesTest
{
  [TestFixture]
  public class SubQueryFromClauseTest
  {
    private IClause _previousClause;
    private ParameterExpression _identifier;
    private QueryModel _subQueryModel;
    private SubQueryFromClause _subQueryFromClause;
    private LambdaExpression _projectionExpression;

    [SetUp]
    public void SetUp ()
    {
      _previousClause = ExpressionHelper.CreateMainFromClause ();
      _identifier = ExpressionHelper.CreateParameterExpression ();
      _subQueryModel = ExpressionHelper.CreateQueryModel ();
      _projectionExpression = ExpressionHelper.CreateLambdaExpression();

      _subQueryFromClause = new SubQueryFromClause (_previousClause, _identifier, _subQueryModel, _projectionExpression);
    }

    [Test]
    public void Initialize()
    {
      Assert.AreSame (_previousClause, _subQueryFromClause.PreviousClause);
      Assert.AreSame (_identifier, _subQueryFromClause.Identifier);
      Assert.AreSame (_subQueryModel, _subQueryFromClause.SubQueryModel);
      Assert.AreSame (_projectionExpression, _subQueryFromClause.ProjectionExpression);
    }

    [Test]
    public void Accept ()
    {
      MockRepository mockRepository = new MockRepository();
      IQueryVisitor visitorMock = mockRepository.CreateMock<IQueryVisitor>();

      // expectation
      visitorMock.VisitSubQueryFromClause (_subQueryFromClause);

      mockRepository.ReplayAll ();
      _subQueryFromClause.Accept (visitorMock);
      mockRepository.VerifyAll ();
    }

    [Test]
    public void GetQueriedEntityType ()
    {
      Assert.AreEqual (null, _subQueryFromClause.GetQuerySourceType ());
    }

    [Test]
    public void GetFromSource ()
    {
      IColumnSource columnSource = _subQueryFromClause.GetFromSource (StubDatabaseInfo.Instance);
      SubQuery subQuery = (SubQuery) columnSource;
      Assert.AreEqual (_identifier.Name, subQuery.Alias);
      Assert.AreSame (_subQueryModel, subQuery.QueryModel);
    }

    [Test]
    public void GetFromSource_FromSourceIsCached ()
    {
      SubQuery subQuery1 = (SubQuery) _subQueryFromClause.GetFromSource (StubDatabaseInfo.Instance);
      SubQuery subQuery2 = (SubQuery) _subQueryFromClause.GetFromSource (StubDatabaseInfo.Instance);
      Assert.AreSame (subQuery1, subQuery2);
    }

    [Test]
    public void QueryModelAtInitialization ()
    {
      SubQueryFromClause subQueryFromClause = ExpressionHelper.CreateSubQueryFromClause ();
      Assert.IsNull (subQueryFromClause.QueryModel);
    }

    [Test]
    public void SetQueryModel ()
    {
      SubQueryFromClause subQueryFromClause = ExpressionHelper.CreateSubQueryFromClause ();
      QueryModel model = ExpressionHelper.CreateQueryModel ();
      subQueryFromClause.SetQueryModel (model);
      Assert.IsNotNull (subQueryFromClause.QueryModel);
    }

    [Test]
    [ExpectedException (typeof (ArgumentNullException))]
    public void SetQueryModelWithNull_Exception ()
    {
      SubQueryFromClause subQueryFromClause = ExpressionHelper.CreateSubQueryFromClause ();
      subQueryFromClause.SetQueryModel (null);
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "QueryModel is already set")]
    public void SetQueryModelTwice_Exception ()
    {
      SubQueryFromClause subQueryFromClause = ExpressionHelper.CreateSubQueryFromClause ();
      QueryModel model = ExpressionHelper.CreateQueryModel ();
      subQueryFromClause.SetQueryModel (model);
      subQueryFromClause.SetQueryModel (model);
    }
  }
}

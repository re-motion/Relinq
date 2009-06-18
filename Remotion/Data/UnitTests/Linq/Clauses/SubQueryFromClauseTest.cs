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
using System.Collections.Generic;
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq;
using Remotion.Data.Linq.Clauses.Expressions;
using Rhino.Mocks;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.DataObjectModel;

namespace Remotion.Data.UnitTests.Linq.Clauses
{
  [TestFixture]
  public class SubQueryFromClauseTest
  {
    private IClause _previousClause;
    private ParameterExpression _identifier;
    private QueryModel _subQueryModel;
    private SubQueryFromClause _subQueryFromClause;
    private LambdaExpression _projectionExpression;
    private CloneContext _cloneContext;

    [SetUp]
    public void SetUp ()
    {
      _previousClause = ExpressionHelper.CreateMainFromClause ();
      _identifier = ExpressionHelper.CreateParameterExpression ();
      _subQueryModel = ExpressionHelper.CreateQueryModel ();
      _projectionExpression = ExpressionHelper.CreateLambdaExpression();

      _subQueryFromClause = new SubQueryFromClause (_previousClause, _identifier, _subQueryModel, _projectionExpression);
      _cloneContext = new CloneContext (new ClonedClauseMapping (), new SubQueryRegistry());
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
      var mockRepository = new MockRepository();
      var visitorMock = mockRepository.StrictMock<IQueryVisitor>();

      visitorMock.Expect (mock => mock.VisitSubQueryFromClause (_subQueryFromClause));

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
      IColumnSource columnSource = _subQueryFromClause.GetColumnSource (StubDatabaseInfo.Instance);
      var subQuery = (SubQuery) columnSource;
      Assert.AreEqual (_identifier.Name, subQuery.Alias);
      Assert.AreSame (_subQueryModel, subQuery.QueryModel);
    }

    [Test]
    public void GetFromSource_FromSourceIsCached ()
    {
      var subQuery1 = (SubQuery) _subQueryFromClause.GetColumnSource (StubDatabaseInfo.Instance);
      var subQuery2 = (SubQuery) _subQueryFromClause.GetColumnSource (StubDatabaseInfo.Instance);
      Assert.AreSame (subQuery1, subQuery2);
    }

    [Test]
    public void QueryModelAtInitialization ()
    {
      Assert.IsNull (_subQueryFromClause.QueryModel);
    }

    [Test]
    public void SetQueryModel ()
    {
      QueryModel model = ExpressionHelper.CreateQueryModel ();
      _subQueryFromClause.SetQueryModel (model);
      Assert.IsNotNull (_subQueryFromClause.QueryModel);
    }

    [Test]
    [ExpectedException (typeof (ArgumentNullException))]
    public void SetQueryModelWithNull_Exception ()
    {
      _subQueryFromClause.SetQueryModel (null);
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "QueryModel is already set")]
    public void SetQueryModelTwice_Exception ()
    {
      QueryModel model = ExpressionHelper.CreateQueryModel ();
      _subQueryFromClause.SetQueryModel (model);
      _subQueryFromClause.SetQueryModel (model);
    }

    [Test]
    public void SetQueryModel_SetsParentQueryOfSubQueryModel ()
    {
      Assert.That (_subQueryFromClause.SubQueryModel.ParentQuery, Is.Null);

      QueryModel model = ExpressionHelper.CreateQueryModel ();
      _subQueryFromClause.SetQueryModel (model);
      Assert.That (_subQueryFromClause.SubQueryModel.ParentQuery, Is.SameAs (model));
    }

    [Test]
    public void Clone ()
    {
      var newPreviousClause = ExpressionHelper.CreateMainFromClause ();
      _cloneContext.ClonedClauseMapping.AddMapping (_subQueryFromClause.PreviousClause, newPreviousClause);
      var clone = _subQueryFromClause.Clone (_cloneContext);

      Assert.That (clone, Is.Not.Null);
      Assert.That (clone, Is.Not.SameAs (_subQueryFromClause));
      Assert.That (clone.Identifier, Is.SameAs (_subQueryFromClause.Identifier));
      Assert.That (clone.ProjectionExpression, Is.SameAs (_subQueryFromClause.ProjectionExpression));
      Assert.That (clone.PreviousClause, Is.SameAs (newPreviousClause));
      Assert.That (clone.QueryModel, Is.Null);
    }

    [Test]
    public void Clone_ClonesSubQueryModel ()
    {
      var newPreviousClause = ExpressionHelper.CreateMainFromClause ();
      _cloneContext.ClonedClauseMapping.AddMapping (_subQueryFromClause.PreviousClause, newPreviousClause);
      var clone = _subQueryFromClause.Clone (_cloneContext);

      Assert.That (clone.SubQueryModel, Is.Not.SameAs (_subQueryFromClause.SubQueryModel));
      Assert.That (clone.SubQueryModel.MainFromClause.QuerySource, Is.SameAs (_subQueryFromClause.SubQueryModel.MainFromClause.QuerySource));
      Assert.That (clone.SubQueryModel.ParentQuery, Is.Null);
    }

    [Test]
    public void Clone_ClonesSubQueryModel_WithCorrectMapping ()
    {
      var newPreviousClause = ExpressionHelper.CreateMainFromClause ();
      _cloneContext.ClonedClauseMapping.AddMapping (_subQueryFromClause.PreviousClause, newPreviousClause);
      var clone = _subQueryFromClause.Clone (_cloneContext);

      Assert.That (clone.SubQueryModel, Is.Not.SameAs (_subQueryFromClause.SubQueryModel));
      Assert.That (clone.SubQueryModel.MainFromClause.QuerySource, Is.SameAs (_subQueryFromClause.SubQueryModel.MainFromClause.QuerySource));
      Assert.That (clone.SubQueryModel.ParentQuery, Is.Null);

      Assert.That (_cloneContext.ClonedClauseMapping.GetClause (_subQueryFromClause.SubQueryModel.MainFromClause), Is.SameAs (clone.SubQueryModel.MainFromClause));
    }

    [Test]
    public void Clone_ViaInterface_PassesMapping ()
    {
      _cloneContext.ClonedClauseMapping.AddMapping (_subQueryFromClause.PreviousClause, ExpressionHelper.CreateClause ());
      var clone = ((IBodyClause) _subQueryFromClause).Clone (_cloneContext);
      Assert.That (_cloneContext.ClonedClauseMapping.GetClause (_subQueryFromClause), Is.SameAs (clone));
    }

    [Test]
    public void Clone_SubQueryModelsParent_SetWhenQueryModelIsSet ()
    {
      var newPreviousClause = ExpressionHelper.CreateMainFromClause ();
      _cloneContext.ClonedClauseMapping.AddMapping (_subQueryFromClause.PreviousClause, newPreviousClause);
      var clone = _subQueryFromClause.Clone (_cloneContext);

      Assert.That (clone, Is.Not.Null);
      Assert.That (clone, Is.Not.SameAs (_subQueryFromClause));
      Assert.That (clone.SubQueryModel.ParentQuery, Is.Null);
      Assert.That (clone.QueryModel, Is.Null);

      var newModel = ExpressionHelper.CreateQueryModel ();
      clone.SetQueryModel (newModel);
      Assert.That (clone.QueryModel, Is.SameAs (newModel));
      Assert.That (clone.SubQueryModel.ParentQuery, Is.SameAs (newModel));
    }

    [Test]
    public void Clone_JoinClauses ()
    {
      var originalJoinClause1 = ExpressionHelper.CreateJoinClause (_subQueryFromClause, _subQueryFromClause);
      _subQueryFromClause.AddJoinClause (originalJoinClause1);

      var originalJoinClause2 = ExpressionHelper.CreateJoinClause (originalJoinClause1, _subQueryFromClause);
      _subQueryFromClause.AddJoinClause (originalJoinClause2);

      var newPreviousClause = ExpressionHelper.CreateClause ();
      _cloneContext.ClonedClauseMapping.AddMapping (_subQueryFromClause.PreviousClause, newPreviousClause);
      var clone = _subQueryFromClause.Clone (_cloneContext);
      Assert.That (clone.JoinClauses.Count, Is.EqualTo (2));

      Assert.That (clone.JoinClauses[0], Is.Not.SameAs (originalJoinClause1));
      Assert.That (clone.JoinClauses[0].EqualityExpression, Is.SameAs (originalJoinClause1.EqualityExpression));
      Assert.That (clone.JoinClauses[0].InExpression, Is.SameAs (originalJoinClause1.InExpression));
      Assert.That (clone.JoinClauses[0].FromClause, Is.SameAs (clone));
      Assert.That (clone.JoinClauses[0].PreviousClause, Is.SameAs (clone));

      Assert.That (clone.JoinClauses[1], Is.Not.SameAs (originalJoinClause2));
      Assert.That (clone.JoinClauses[1].EqualityExpression, Is.SameAs (originalJoinClause2.EqualityExpression));
      Assert.That (clone.JoinClauses[1].InExpression, Is.SameAs (originalJoinClause2.InExpression));
      Assert.That (clone.JoinClauses[1].FromClause, Is.SameAs (clone));
      Assert.That (clone.JoinClauses[1].PreviousClause, Is.SameAs (clone.JoinClauses[0]));
    }

    [Test]
    [Ignore ("TODO 1229")]
    public void Clone_JoinClauses_PassesMapping ()
    {
      var oldFromClause = ExpressionHelper.CreateMainFromClause ();
      _cloneContext.ClonedClauseMapping.AddMapping (_subQueryFromClause.PreviousClause, ExpressionHelper.CreateClause ());
      var originalJoinClause = new JoinClause (
          _subQueryFromClause,
          _subQueryFromClause,
          ExpressionHelper.CreateParameterExpression (),
          new QuerySourceReferenceExpression (oldFromClause),
          ExpressionHelper.CreateExpression (),
          ExpressionHelper.CreateExpression ());
      _subQueryFromClause.AddJoinClause (originalJoinClause);

      var newFromClause = ExpressionHelper.CreateMainFromClause ();
      _cloneContext.ClonedClauseMapping.AddMapping (oldFromClause, newFromClause);

      var clone = _subQueryFromClause.Clone (_cloneContext);
      Assert.That (((QuerySourceReferenceExpression) clone.JoinClauses[0].InExpression).ReferencedClause, Is.SameAs (newFromClause));
    }

    [Test]
    public void Clone_AddsClauseToMapping ()
    {
      _cloneContext.ClonedClauseMapping.AddMapping (_subQueryFromClause.PreviousClause, ExpressionHelper.CreateClause ());
      var clone = _subQueryFromClause.Clone (_cloneContext);
      Assert.That (_cloneContext.ClonedClauseMapping.GetClause (_subQueryFromClause), Is.SameAs (clone));
    }

    [Test]
    public void Clone_AdjustsExpressions ()
    {
      var mainFromClause = ExpressionHelper.CreateMainFromClause ();
      var projectionExpression = new QuerySourceReferenceExpression (mainFromClause);
      var subQueryFromClause = new SubQueryFromClause (
          mainFromClause, 
          ExpressionHelper.CreateParameterExpression(), 
          ExpressionHelper.CreateQueryModel(), 
          Expression.Lambda (projectionExpression));

      var newMainFromClause = ExpressionHelper.CreateMainFromClause ();
      _cloneContext.ClonedClauseMapping.AddMapping (mainFromClause, newMainFromClause);

      var clone = subQueryFromClause.Clone (_cloneContext);

      Assert.That (((QuerySourceReferenceExpression) clone.ProjectionExpression.Body).ReferencedClause, Is.SameAs (newMainFromClause));
    }
  }
}

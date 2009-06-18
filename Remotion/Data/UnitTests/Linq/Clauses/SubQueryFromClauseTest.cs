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
      var originalClause = ExpressionHelper.CreateSubQueryFromClause ();
      var newPreviousClause = ExpressionHelper.CreateMainFromClause ();
      var clone = originalClause.Clone (newPreviousClause, new FromClauseMapping());

      Assert.That (clone, Is.Not.Null);
      Assert.That (clone, Is.Not.SameAs (originalClause));
      Assert.That (clone.Identifier, Is.SameAs (originalClause.Identifier));
      Assert.That (clone.ProjectionExpression, Is.SameAs (originalClause.ProjectionExpression));
      Assert.That (clone.SubQueryModel, Is.Not.SameAs (originalClause.SubQueryModel));
      Assert.That (clone.SubQueryModel.MainFromClause.QuerySource, Is.SameAs (originalClause.SubQueryModel.MainFromClause.QuerySource));
      Assert.That (clone.SubQueryModel.ParentQuery, Is.Null);
      Assert.That (clone.PreviousClause, Is.SameAs (newPreviousClause));
      Assert.That (clone.QueryModel, Is.Null);
    }

    [Test]
    public void Clone_SubQueryModelsParent_SetWhenQueryModelIsSet ()
    {
      var originalClause = ExpressionHelper.CreateSubQueryFromClause ();
      var newPreviousClause = ExpressionHelper.CreateMainFromClause ();
      var clone = originalClause.Clone (newPreviousClause, new FromClauseMapping());

      Assert.That (clone, Is.Not.Null);
      Assert.That (clone, Is.Not.SameAs (originalClause));
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
      SubQueryFromClause originalClause = ExpressionHelper.CreateSubQueryFromClause ();
      var originalJoinClause1 = ExpressionHelper.CreateJoinClause ();
      originalClause.AddJoinClause (originalJoinClause1);

      var originalJoinClause2 = ExpressionHelper.CreateJoinClause ();
      originalClause.AddJoinClause (originalJoinClause2);

      var newPreviousClause = ExpressionHelper.CreateClause ();
      var clone = originalClause.Clone (newPreviousClause, new FromClauseMapping());
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
      var originalJoinClause = new JoinClause (
          _subQueryFromClause,
          _subQueryFromClause,
          ExpressionHelper.CreateParameterExpression (),
          new QuerySourceReferenceExpression (oldFromClause),
          ExpressionHelper.CreateExpression (),
          ExpressionHelper.CreateExpression ());
      _subQueryFromClause.AddJoinClause (originalJoinClause);

      var mapping = new FromClauseMapping ();
      var newFromClause = ExpressionHelper.CreateMainFromClause ();
      mapping.AddMapping (oldFromClause, newFromClause);

      var clone = _subQueryFromClause.Clone (ExpressionHelper.CreateClause (), mapping);
      Assert.That (((QuerySourceReferenceExpression) clone.JoinClauses[0].InExpression).ReferencedClause, Is.SameAs (newFromClause));
    }
  }
}

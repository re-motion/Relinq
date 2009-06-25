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
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.DataObjectModel;
using Rhino.Mocks;

namespace Remotion.Data.UnitTests.Linq.Clauses
{
  [TestFixture]
  public class SubQueryFromClauseTest
  {
    private QueryModel _subQueryModel;
    private SubQueryFromClause _subQueryFromClause;
    private CloneContext _cloneContext;

    [SetUp]
    public void SetUp ()
    {
      _subQueryModel = ExpressionHelper.CreateQueryModel();
      _subQueryFromClause = new SubQueryFromClause ("s", typeof (Student), _subQueryModel);
      _cloneContext = new CloneContext (new ClonedClauseMapping());
    }

    [Test]
    public void Initialize ()
    {
      Assert.That (_subQueryFromClause.ItemName, Is.EqualTo ("s"));
      Assert.That (_subQueryFromClause.ItemType, Is.EqualTo (typeof (Student)));
      Assert.That (_subQueryFromClause.SubQueryModel, Is.SameAs (_subQueryModel));
    }

    [Test]
    public void Accept ()
    {
      var mockRepository = new MockRepository();
      var visitorMock = mockRepository.StrictMock<IQueryVisitor>();

      visitorMock.Expect (mock => mock.VisitSubQueryFromClause (_subQueryFromClause));

      mockRepository.ReplayAll();
      _subQueryFromClause.Accept (visitorMock);
      mockRepository.VerifyAll();
    }

    [Test]
    public void GetQueriedEntityType ()
    {
      Assert.That (_subQueryFromClause.GetQuerySourceType(), Is.EqualTo (null));
    }

    [Test]
    public void GetFromSource ()
    {
      IColumnSource columnSource = _subQueryFromClause.GetColumnSource (StubDatabaseInfo.Instance);
      var subQuery = (SubQuery) columnSource;
      Assert.That (subQuery.Alias, Is.EqualTo ("s"));
      Assert.That (subQuery.QueryModel, Is.SameAs (_subQueryModel));
    }

    [Test]
    public void GetFromSource_FromSourceIsCached ()
    {
      var subQuery1 = (SubQuery) _subQueryFromClause.GetColumnSource (StubDatabaseInfo.Instance);
      var subQuery2 = (SubQuery) _subQueryFromClause.GetColumnSource (StubDatabaseInfo.Instance);
      Assert.That (subQuery2, Is.SameAs (subQuery1));
    }

    [Test]
    public void Clone ()
    {
      var clone = _subQueryFromClause.Clone (_cloneContext);

      Assert.That (clone, Is.Not.Null);
      Assert.That (clone, Is.Not.SameAs (_subQueryFromClause));
      Assert.That (clone.ItemName, Is.EqualTo (_subQueryFromClause.ItemName));
      Assert.That (clone.ItemType, Is.SameAs (_subQueryFromClause.ItemType));
    }

    [Test]
    public void Clone_ClonesSubQueryModel ()
    {
      var clone = _subQueryFromClause.Clone (_cloneContext);

      Assert.That (clone.SubQueryModel, Is.Not.SameAs (_subQueryFromClause.SubQueryModel));
      Assert.That (clone.SubQueryModel.MainFromClause.FromExpression, Is.SameAs (_subQueryFromClause.SubQueryModel.MainFromClause.FromExpression));
    }

    [Test]
    public void Clone_ClonesSubQueryModel_WithCorrectMapping ()
    {
      var clone = _subQueryFromClause.Clone (_cloneContext);

      Assert.That (clone.SubQueryModel, Is.Not.SameAs (_subQueryFromClause.SubQueryModel));
      Assert.That (clone.SubQueryModel.MainFromClause.FromExpression, Is.SameAs (_subQueryFromClause.SubQueryModel.MainFromClause.FromExpression));
      Assert.That (
          _cloneContext.ClonedClauseMapping.GetClause (_subQueryFromClause.SubQueryModel.MainFromClause),
          Is.SameAs (clone.SubQueryModel.MainFromClause));
    }

    [Test]
    public void Clone_ViaInterface_PassesMapping ()
    {
      var clone = ((IBodyClause) _subQueryFromClause).Clone (_cloneContext);
      Assert.That (_cloneContext.ClonedClauseMapping.GetClause (_subQueryFromClause), Is.SameAs (clone));
    }

    [Test]
    public void Clone_JoinClauses ()
    {
      var originalJoinClause1 = ExpressionHelper.CreateJoinClause (_subQueryFromClause);
      _subQueryFromClause.JoinClauses.Add (originalJoinClause1);

      var originalJoinClause2 = ExpressionHelper.CreateJoinClause (_subQueryFromClause);
      _subQueryFromClause.JoinClauses.Add (originalJoinClause2);

      var clone = _subQueryFromClause.Clone (_cloneContext);
      Assert.That (clone.JoinClauses.Count, Is.EqualTo (2));

      Assert.That (clone.JoinClauses[0], Is.Not.SameAs (originalJoinClause1));
      Assert.That (clone.JoinClauses[0].EqualityExpression, Is.SameAs (originalJoinClause1.EqualityExpression));
      Assert.That (clone.JoinClauses[0].InExpression, Is.SameAs (originalJoinClause1.InExpression));
      Assert.That (clone.JoinClauses[0].FromClause, Is.SameAs (clone));

      Assert.That (clone.JoinClauses[1], Is.Not.SameAs (originalJoinClause2));
      Assert.That (clone.JoinClauses[1].EqualityExpression, Is.SameAs (originalJoinClause2.EqualityExpression));
      Assert.That (clone.JoinClauses[1].InExpression, Is.SameAs (originalJoinClause2.InExpression));
      Assert.That (clone.JoinClauses[1].FromClause, Is.SameAs (clone));
    }

    [Test]
    public void Clone_JoinClauses_PassesMapping ()
    {
      var oldFromClause = ExpressionHelper.CreateMainFromClause();
      var originalJoinClause = new JoinClause (_subQueryFromClause,
          "x",
          typeof(Student),
          new QuerySourceReferenceExpression (oldFromClause),
          ExpressionHelper.CreateExpression(),
          ExpressionHelper.CreateExpression());
      _subQueryFromClause.JoinClauses.Add (originalJoinClause);

      var newFromClause = ExpressionHelper.CreateMainFromClause();
      _cloneContext.ClonedClauseMapping.AddMapping (oldFromClause, newFromClause);

      var clone = _subQueryFromClause.Clone (_cloneContext);
      Assert.That (((QuerySourceReferenceExpression) clone.JoinClauses[0].InExpression).ReferencedClause, Is.SameAs (newFromClause));
    }

    [Test]
    public void Clone_AddsClauseToMapping ()
    {
      var clone = _subQueryFromClause.Clone (_cloneContext);
      Assert.That (_cloneContext.ClonedClauseMapping.GetClause (_subQueryFromClause), Is.SameAs (clone));
    }

    [Test]
    public void SubQueryModel_Set_ChangesColumnSource ()
    {
      var oldSource = _subQueryFromClause.GetColumnSource (StubDatabaseInfo.Instance);
      _subQueryFromClause.SubQueryModel = ExpressionHelper.CreateQueryModel();
      var newSource = _subQueryFromClause.GetColumnSource (StubDatabaseInfo.Instance);

      Assert.That (oldSource, Is.Not.EqualTo (newSource));
      Assert.That (((SubQuery) newSource).QueryModel, Is.SameAs (_subQueryFromClause.SubQueryModel));
    }
  }
}
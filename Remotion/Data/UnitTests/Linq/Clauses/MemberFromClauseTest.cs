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
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.DataObjectModel;
using Rhino.Mocks;

namespace Remotion.Data.UnitTests.Linq.Clauses
{
  [TestFixture]
  public class MemberFromClauseTest
  {
    private MemberFromClause _memberFromClause;

    [SetUp]
    public void SetUp ()
    {
      _memberFromClause = ExpressionHelper.CreateMemberFromClause ();
    }

    [Test]
    public void FromExpressionContainsMemberExpression ()
    {
      Assert.That (_memberFromClause.MemberExpression, Is.SameAs (_memberFromClause.FromExpression.Body));
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = "From expression must contain a MemberExpression.")]
    public void FromExpressionContainNoMemberExpression ()
    {
      var previousClause = ExpressionHelper.CreateClause ();
      var identifier = ExpressionHelper.CreateParameterExpression ();
      var bodyExpression = Expression.Constant ("test");
      var fromExpression = Expression.Lambda (bodyExpression);
      var projectionExpression = ExpressionHelper.CreateLambdaExpression ();
      new MemberFromClause (previousClause, identifier, fromExpression, projectionExpression);
    }

    [Test]
    public void Accept ()
    {
      var visitorMock = MockRepository.GenerateMock<IQueryVisitor> ();
      _memberFromClause.Accept (visitorMock);
      visitorMock.AssertWasCalled (mock => mock.VisitMemberFromClause (_memberFromClause));
    }

    [Test]
    public void GetFromSource ()
    {
      var columnSource = _memberFromClause.GetColumnSource (StubDatabaseInfo.Instance);

      Assert.That (columnSource is Table);
      var tableSource = (Table) columnSource;
      Assert.That (tableSource.Name, Is.EqualTo ("studentTable"));
      Assert.That (tableSource.Alias, Is.EqualTo (_memberFromClause.Identifier.Name));
    }

    [Test]
    public void Clone ()
    {
      var newPreviousClause = ExpressionHelper.CreateMainFromClause ();
      var clone = (MemberFromClause) _memberFromClause.Clone (newPreviousClause, new ClonedClauseMapping());

      Assert.That (clone, Is.Not.Null);
      Assert.That (clone, Is.Not.SameAs (_memberFromClause));
      Assert.That (clone.Identifier, Is.SameAs (_memberFromClause.Identifier));
      Assert.That (clone.FromExpression, Is.SameAs (_memberFromClause.FromExpression));
      Assert.That (clone.ResultSelector, Is.SameAs (_memberFromClause.ResultSelector));
      Assert.That (clone.MemberExpression, Is.SameAs (_memberFromClause.MemberExpression));
      Assert.That (clone.PreviousClause, Is.SameAs (newPreviousClause));
      Assert.That (clone.QueryModel, Is.Null);
    }

    [Test]
    public void Clone_JoinClauses ()
    {
      var originalJoinClause1 = ExpressionHelper.CreateJoinClause ();
      _memberFromClause.AddJoinClause (originalJoinClause1);

      var originalJoinClause2 = ExpressionHelper.CreateJoinClause ();
      _memberFromClause.AddJoinClause (originalJoinClause2);

      var newPreviousClause = ExpressionHelper.CreateClause ();
      var clone = _memberFromClause.Clone (newPreviousClause, new ClonedClauseMapping());
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
          _memberFromClause,
          _memberFromClause,
          ExpressionHelper.CreateParameterExpression (),
          new QuerySourceReferenceExpression (oldFromClause),
          ExpressionHelper.CreateExpression (),
          ExpressionHelper.CreateExpression ());
      _memberFromClause.AddJoinClause (originalJoinClause);

      var mapping = new ClonedClauseMapping ();
      var newFromClause = ExpressionHelper.CreateMainFromClause ();
      mapping.AddMapping (oldFromClause, newFromClause);

      var clone = _memberFromClause.Clone (ExpressionHelper.CreateClause (), mapping);
      Assert.That (((QuerySourceReferenceExpression) clone.JoinClauses[0].InExpression).ReferencedClause, Is.SameAs (newFromClause));
    }
  }
}

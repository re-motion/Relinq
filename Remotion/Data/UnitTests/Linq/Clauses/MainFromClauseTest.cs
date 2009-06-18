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
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq;
using Remotion.Data.Linq.Clauses.Expressions;
using Rhino.Mocks;
using Remotion.Data.Linq.Clauses;

namespace Remotion.Data.UnitTests.Linq.Clauses
{
  [TestFixture]
  public class MainFromClauseTest
  {
    private MainFromClause _mainFromClause;
    private ClonedClauseMapping _clonedClauseMapping;

    [SetUp]
    public void SetUp ()
    {
      _mainFromClause = ExpressionHelper.CreateMainFromClause();
      _clonedClauseMapping = new ClonedClauseMapping ();
    }

    [Test]
    public void Initialize_WithIDAndConstant ()
    {
      ParameterExpression id = ExpressionHelper.CreateParameterExpression ();
      IQueryable querySource = ExpressionHelper.CreateQuerySource ();

      ConstantExpression constantExpression = Expression.Constant (querySource);
      MainFromClause fromClause = new MainFromClause (id, constantExpression);

      Assert.AreSame (id, fromClause.Identifier);
      Assert.AreSame (constantExpression, fromClause.QuerySource);

      Assert.That (fromClause.JoinClauses, Is.Empty);
      Assert.AreEqual (0, fromClause.JoinClauses.Count);

      Assert.IsNull (fromClause.PreviousClause);
    }

    [Test]
    public void Initialize_WithExpression ()
    {
      ParameterExpression id = ExpressionHelper.CreateParameterExpression ();
      IQueryable querySource = ExpressionHelper.CreateQuerySource ();
      var anonymous = new {source = querySource};
      MemberExpression sourceExpression = Expression.MakeMemberAccess (Expression.Constant (anonymous), anonymous.GetType().GetProperty ("source"));

      MainFromClause fromClause = new MainFromClause (id, sourceExpression);

      Assert.AreSame (sourceExpression, fromClause.QuerySource);
    }

    [Test]
    public void Accept ()
    {
      MockRepository repository = new MockRepository ();
      IQueryVisitor visitorMock = repository.StrictMock<IQueryVisitor> ();

      visitorMock.VisitMainFromClause (_mainFromClause);

      repository.ReplayAll ();

      _mainFromClause.Accept (visitorMock);

      repository.VerifyAll ();
    }

    [Test]
    public void GetQueriedEntityType ()
    {
      IQueryable<Student> querySource = ExpressionHelper.CreateQuerySource();
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause(ExpressionHelper.CreateParameterExpression(), querySource);
      Assert.AreSame (typeof (TestQueryable<Student>), fromClause.GetQuerySourceType());
    }

    [Test]
    public void Clone ()
    {
      var clone = _mainFromClause.Clone (_clonedClauseMapping);

      Assert.That (clone, Is.Not.Null);
      Assert.That (clone, Is.Not.SameAs (_mainFromClause));
      Assert.That (clone.Identifier, Is.SameAs (_mainFromClause.Identifier));
      Assert.That (clone.QuerySource, Is.SameAs (_mainFromClause.QuerySource));
    }

    [Test]
    public void Clone_JoinClauses ()
    {
      var originalJoinClause1 = ExpressionHelper.CreateJoinClause ();
      _mainFromClause.AddJoinClause (originalJoinClause1);

      var originalJoinClause2 = ExpressionHelper.CreateJoinClause ();
      _mainFromClause.AddJoinClause (originalJoinClause2);

      var clone = _mainFromClause.Clone (_clonedClauseMapping);
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
          _mainFromClause,
          _mainFromClause,
          ExpressionHelper.CreateParameterExpression(),
          new QuerySourceReferenceExpression (oldFromClause),
          ExpressionHelper.CreateExpression(),
          ExpressionHelper.CreateExpression());
      _mainFromClause.AddJoinClause (originalJoinClause);

      var newFromClause = ExpressionHelper.CreateMainFromClause ();
      _clonedClauseMapping.AddMapping (oldFromClause, newFromClause);

      var clone = _mainFromClause.Clone (_clonedClauseMapping);
      Assert.That (((QuerySourceReferenceExpression) clone.JoinClauses[0].InExpression).ReferencedClause, Is.SameAs (newFromClause));
    }

    [Test]
    public void Clone_AddsClauseToMapping ()
    {
      var clone = _mainFromClause.Clone (_clonedClauseMapping);
      Assert.That (_clonedClauseMapping.GetClause (_mainFromClause), Is.SameAs (clone));
    }
  }
}

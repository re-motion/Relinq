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
    private CloneContext _cloneContext;

    [SetUp]
    public void SetUp ()
    {
      _mainFromClause = ExpressionHelper.CreateMainFromClause();
      _cloneContext = new CloneContext (new ClonedClauseMapping());
    }

    [Test]
    public void Initialize ()
    {
      IQueryable querySource = ExpressionHelper.CreateQuerySource ();

      ConstantExpression constantExpression = Expression.Constant (querySource);
      var fromClause = new MainFromClause ("s", typeof (Student), constantExpression);

      Assert.That (fromClause.ItemName, Is.EqualTo ("s"));
      Assert.That (fromClause.ItemType, Is.SameAs (typeof (Student)));
      Assert.That (fromClause.QuerySource, Is.SameAs (constantExpression));

      Assert.That (fromClause.JoinClauses, Is.Empty);
      Assert.That (fromClause.JoinClauses.Count, Is.EqualTo (0));

      Assert.That (fromClause.PreviousClause, Is.Null);
    }

    [Test]
    public void Initialize_WithNonConstantExpression ()
    {
      IQueryable querySource = ExpressionHelper.CreateQuerySource ();
      var anonymous = new {source = querySource};
      MemberExpression sourceExpression = Expression.MakeMemberAccess (Expression.Constant (anonymous), anonymous.GetType().GetProperty ("source"));

      var fromClause = new MainFromClause ("s", typeof (Student), sourceExpression);
      Assert.That (fromClause.QuerySource, Is.SameAs (sourceExpression));
    }

    [Test]
    public void Accept ()
    {
      var repository = new MockRepository ();
      var visitorMock = repository.StrictMock<IQueryVisitor> ();

      visitorMock.VisitMainFromClause (_mainFromClause);

      repository.ReplayAll ();

      _mainFromClause.Accept (visitorMock);

      repository.VerifyAll ();
    }

    [Test]
    public void GetQueriedEntityType ()
    {
      IQueryable<Student> querySource = ExpressionHelper.CreateQuerySource();
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause ("s", typeof (Student), querySource);
      Assert.That (fromClause.GetQuerySourceType(), Is.SameAs (typeof (TestQueryable<Student>)));
    }

    [Test]
    public void Clone ()
    {
      var clone = _mainFromClause.Clone (_cloneContext);

      Assert.That (clone, Is.Not.Null);
      Assert.That (clone, Is.Not.SameAs (_mainFromClause));
      Assert.That (clone.ItemName, Is.EqualTo (_mainFromClause.ItemName));
      Assert.That (clone.ItemType, Is.SameAs (_mainFromClause.ItemType));
      Assert.That (clone.QuerySource, Is.SameAs (_mainFromClause.QuerySource));
    }

    [Test]
    public void Clone_AdjustsExpressions ()
    {
      var oldReferencedClause = ExpressionHelper.CreateMainFromClause();
      var querySource = new QuerySourceReferenceExpression (oldReferencedClause);
      var mainFromClause = new MainFromClause ("s", typeof (Student), querySource);

      var newReferencedClause = ExpressionHelper.CreateMainFromClause ();
      _cloneContext.ClonedClauseMapping.AddMapping (oldReferencedClause, newReferencedClause);

      var clone = mainFromClause.Clone (_cloneContext);

      Assert.That (((QuerySourceReferenceExpression) clone.QuerySource).ReferencedClause, Is.SameAs (newReferencedClause));
    }

    [Test]
    public void Clone_JoinClauses ()
    {
      var originalJoinClause1 = ExpressionHelper.CreateJoinClause (_mainFromClause, _mainFromClause);
      _mainFromClause.JoinClauses.Add (originalJoinClause1);

      var originalJoinClause2 = ExpressionHelper.CreateJoinClause (originalJoinClause1, _mainFromClause);
      _mainFromClause.JoinClauses.Add (originalJoinClause2);

      var clone = _mainFromClause.Clone (_cloneContext);
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
    public void Clone_JoinClauses_PassesMapping ()
    {
      var oldFromClause = ExpressionHelper.CreateMainFromClause ();
      var originalJoinClause = new JoinClause (
          _mainFromClause,
          _mainFromClause,
          "x", 
          typeof(Student),
          new QuerySourceReferenceExpression (oldFromClause),
          ExpressionHelper.CreateExpression(),
          ExpressionHelper.CreateExpression());
      _mainFromClause.JoinClauses.Add (originalJoinClause);

      var newFromClause = ExpressionHelper.CreateMainFromClause ();
      _cloneContext.ClonedClauseMapping.AddMapping (oldFromClause, newFromClause);

      var clone = _mainFromClause.Clone (_cloneContext);
      Assert.That (((QuerySourceReferenceExpression) clone.JoinClauses[0].InExpression).ReferencedClause, Is.SameAs (newFromClause));
    }

    [Test]
    public void Clone_AddsClauseToMapping ()
    {
      var clone = _mainFromClause.Clone (_cloneContext);
      Assert.That (_cloneContext.ClonedClauseMapping.GetClause (_mainFromClause), Is.SameAs (clone));
    }
  }
}

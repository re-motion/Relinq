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
using Remotion.Utilities;
using Rhino.Mocks;

namespace Remotion.Data.UnitTests.Linq.Clauses
{
  [TestFixture]
  public class MemberFromClauseTest
  {
    private MemberFromClause _memberFromClause;
    private CloneContext _cloneContext;

    [SetUp]
    public void SetUp ()
    {
      _memberFromClause = ExpressionHelper.CreateMemberFromClause ();
      _cloneContext = new CloneContext (new ClonedClauseMapping ());      
    }

    [Test]
    public void FromExpressionContainsMemberExpression ()
    {
      Assert.That (_memberFromClause.MemberExpression, Is.SameAs (_memberFromClause.FromExpression));
    }

    [Test]
    public void MemberExpression_SetModifiesFromExpression ()
    {
      var memberExpression = (MemberExpression) ExpressionHelper.MakeExpression<Student, int> (s => s.ID);
      _memberFromClause.MemberExpression = memberExpression;

      Assert.That (_memberFromClause.MemberExpression, Is.SameAs (memberExpression));
      Assert.That (_memberFromClause.FromExpression, Is.SameAs (memberExpression));
    }

    [Test]
    public void FromExpression_SetModifiesMemberExpression ()
    {
      var memberExpression = (MemberExpression) ExpressionHelper.MakeExpression<Student, int> (s => s.ID);
      _memberFromClause.FromExpression = memberExpression;

      Assert.That (_memberFromClause.FromExpression, Is.SameAs (memberExpression));
      Assert.That (_memberFromClause.MemberExpression, Is.SameAs (memberExpression));
    }

    [Test]
    [ExpectedException (typeof (ArgumentTypeException))]
    public void FromExpression_SetWithNonMemberExpressionThrows ()
    {
      _memberFromClause.FromExpression = Expression.Constant (null);
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
      Assert.That (tableSource.Alias, Is.EqualTo (_memberFromClause.ItemName));
    }

    [Test]
    public void Clone ()
    {
      var newPreviousClause = ExpressionHelper.CreateMainFromClause ();
      _cloneContext.ClonedClauseMapping.AddMapping (_memberFromClause.PreviousClause, newPreviousClause);
      var clone = (MemberFromClause) _memberFromClause.Clone (_cloneContext);

      Assert.That (clone, Is.Not.Null);
      Assert.That (clone, Is.Not.SameAs (_memberFromClause));
      Assert.That (clone.ItemName, Is.EqualTo (_memberFromClause.ItemName));
      Assert.That (clone.ItemType, Is.SameAs (_memberFromClause.ItemType));
      Assert.That (clone.FromExpression, Is.SameAs (_memberFromClause.FromExpression));
      Assert.That (clone.MemberExpression, Is.SameAs (_memberFromClause.MemberExpression));
      Assert.That (clone.PreviousClause, Is.SameAs (newPreviousClause));
    }

    [Test]
    public void Clone_AdjustsExpressions ()
    {
      var mainFromClause = ExpressionHelper.CreateMainFromClause_Student();
      var fromExpression = Expression.MakeMemberAccess (new QuerySourceReferenceExpression (mainFromClause), typeof (Student).GetProperty ("OtherStudent"));
      var memberFromClause = new MemberFromClause (
          mainFromClause, 
          "s", 
          typeof (Student),
          fromExpression);

      var newMainFromClause = ExpressionHelper.CreateMainFromClause_Student();
      _cloneContext.ClonedClauseMapping.AddMapping (mainFromClause, newMainFromClause);

      var clone = (MemberFromClause) memberFromClause.Clone (_cloneContext);

      Assert.That (((QuerySourceReferenceExpression) clone.MemberExpression.Expression).ReferencedClause, Is.SameAs (newMainFromClause));
    }

    [Test]
    public void Clone_ViaInterface_PassesMapping ()
    {
      _cloneContext.ClonedClauseMapping.AddMapping (_memberFromClause.PreviousClause, ExpressionHelper.CreateClause());
      var clone = ((IBodyClause) _memberFromClause).Clone (_cloneContext);
      Assert.That (_cloneContext.ClonedClauseMapping.GetClause (_memberFromClause), Is.SameAs (clone));
    }

    [Test]
    public void Clone_JoinClauses ()
    {
      var originalJoinClause1 = ExpressionHelper.CreateJoinClause (_memberFromClause, _memberFromClause);
      _memberFromClause.JoinClauses.Add (originalJoinClause1);

      var originalJoinClause2 = ExpressionHelper.CreateJoinClause (originalJoinClause1, _memberFromClause);
      _memberFromClause.JoinClauses.Add (originalJoinClause2);

      var newPreviousClause = ExpressionHelper.CreateClause ();
      _cloneContext.ClonedClauseMapping.AddMapping (_memberFromClause.PreviousClause, newPreviousClause);
      var clone = _memberFromClause.Clone (_cloneContext);
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
          _memberFromClause,
          _memberFromClause,
          "x",
          typeof(Student),
          new QuerySourceReferenceExpression (oldFromClause),
          ExpressionHelper.CreateExpression (),
          ExpressionHelper.CreateExpression ());
      _memberFromClause.JoinClauses.Add (originalJoinClause);

      var newFromClause = ExpressionHelper.CreateMainFromClause ();
      _cloneContext.ClonedClauseMapping.AddMapping (_memberFromClause.PreviousClause, ExpressionHelper.CreateClause ());
      _cloneContext.ClonedClauseMapping.AddMapping (oldFromClause, newFromClause);

      var clone = _memberFromClause.Clone (_cloneContext);
      Assert.That (((QuerySourceReferenceExpression) clone.JoinClauses[0].InExpression).ReferencedClause, Is.SameAs (newFromClause));
    }

    [Test]
    public void Clone_AddsClauseToMapping ()
    {
      _cloneContext.ClonedClauseMapping.AddMapping (_memberFromClause.PreviousClause, ExpressionHelper.CreateClause ());
      var clone = _memberFromClause.Clone (_cloneContext);
      Assert.That (_cloneContext.ClonedClauseMapping.GetClause (_memberFromClause), Is.SameAs (clone));
    }
  }
}

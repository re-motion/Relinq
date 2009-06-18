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
using Rhino.Mocks;
using Remotion.Data.Linq.Clauses;

namespace Remotion.Data.UnitTests.Linq.Clauses
{
  [TestFixture]
  public class JoinClauseTest
  {
    private JoinClause _joinClause;
    private ClonedClauseMapping _clonedClauseMapping;

    [SetUp]
    public void SetUp ()
    {
      _joinClause = ExpressionHelper.CreateJoinClause ();
      _clonedClauseMapping = new ClonedClauseMapping ();
    }

    [Test]
    public void Intialize_WithIDAndInExprAndOnExprAndEqExpr()
    {
      ParameterExpression identifier = ExpressionHelper.CreateParameterExpression ();
      Expression inExpression = ExpressionHelper.CreateExpression ();
      Expression onExpression = ExpressionHelper.CreateExpression ();
      Expression equalityExpression = ExpressionHelper.CreateExpression ();

      var previousClause = ExpressionHelper.CreateClause();
      var fromClause = ExpressionHelper.CreateMainFromClause ();
      var joinClause = new JoinClause (previousClause, fromClause, identifier, inExpression, onExpression, equalityExpression);

      Assert.AreSame (fromClause, joinClause.FromClause);
      Assert.AreSame (previousClause, joinClause.PreviousClause);
      Assert.AreSame (identifier, joinClause.Identifier);
      Assert.AreSame (inExpression, joinClause.InExpression);
      Assert.AreSame (onExpression, joinClause.OnExpression);
      Assert.AreSame (equalityExpression, joinClause.EqualityExpression);
      
      Assert.IsNull (joinClause.IntoIdentifier);
    }

    [Test]
    public void Intialize_WithIDAndInExprAndOnExprAndEqExprAndIntoID ()
    {
      ParameterExpression identifier = ExpressionHelper.CreateParameterExpression ();
      Expression inExpression = ExpressionHelper.CreateExpression ();
      Expression onExpression = ExpressionHelper.CreateExpression ();
      Expression equalityExpression = ExpressionHelper.CreateExpression ();
      ParameterExpression intoIdentifier = ExpressionHelper.CreateParameterExpression ();

      var fromClause = ExpressionHelper.CreateMainFromClause ();
      var previousClause = ExpressionHelper.CreateClause ();
      var joinClause = new JoinClause (previousClause, fromClause, identifier, inExpression, onExpression, equalityExpression, intoIdentifier);

      Assert.AreSame (fromClause, joinClause.FromClause);
      Assert.AreSame (previousClause, joinClause.PreviousClause);
      Assert.AreSame (identifier, joinClause.Identifier);
      Assert.AreSame (inExpression, joinClause.InExpression);
      Assert.AreSame (onExpression, joinClause.OnExpression);
      Assert.AreSame (equalityExpression, joinClause.EqualityExpression);
      Assert.AreSame (equalityExpression, joinClause.EqualityExpression);
      Assert.AreSame (intoIdentifier, joinClause.IntoIdentifier);
    }

    [Test]
    public void Accept ()
    {
      var repository = new MockRepository ();
      var visitorMock = repository.StrictMock<IQueryVisitor> ();

      visitorMock.VisitJoinClause (_joinClause);

      repository.ReplayAll ();

      _joinClause.Accept (visitorMock);

      repository.VerifyAll ();
    }

    [Test]
    public void Clone ()
    {
      var newPreviousClause = ExpressionHelper.CreateClause ();
      var newFromClause = ExpressionHelper.CreateMainFromClause ();
      var clone = _joinClause.Clone (newPreviousClause, newFromClause, _clonedClauseMapping);

      Assert.That (clone, Is.Not.Null);
      Assert.That (clone, Is.Not.SameAs (_joinClause));
      Assert.That (clone.EqualityExpression, Is.SameAs (_joinClause.EqualityExpression));
      Assert.That (clone.Identifier, Is.SameAs (_joinClause.Identifier));
      Assert.That (clone.InExpression, Is.SameAs (_joinClause.InExpression));
      Assert.That (clone.IntoIdentifier, Is.SameAs (_joinClause.IntoIdentifier));
      Assert.That (clone.OnExpression, Is.SameAs (_joinClause.OnExpression));
      Assert.That (clone.FromClause, Is.SameAs (newFromClause));
      Assert.That (clone.PreviousClause, Is.SameAs (newPreviousClause));
    }

    [Test]
    public void Clone_AddsClauseToMapping ()
    {
      var clone = _joinClause.Clone (ExpressionHelper.CreateClause (), ExpressionHelper.CreateMainFromClause(), _clonedClauseMapping);
      Assert.That (_clonedClauseMapping.GetClause (_joinClause), Is.SameAs (clone));
    }
  }
}

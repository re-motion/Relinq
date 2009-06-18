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
  public class GroupClauseTest
  {
    private GroupClause _groupClause;
    private ClonedClauseMapping _clonedClauseMapping;

    [SetUp]
    public void SetUp ()
    {
      _groupClause = ExpressionHelper.CreateGroupClause ();
      _clonedClauseMapping = new ClonedClauseMapping ();
    }

    [Test]
    public void InitializeWithGroupExpressionAndByExpression()
    {
      Expression groupExpression = ExpressionHelper.CreateExpression();
      Expression byExpression = ExpressionHelper.CreateExpression();

      IClause clause = ExpressionHelper.CreateClause();
      GroupClause groupClause = new GroupClause (clause, groupExpression, byExpression);

      Assert.AreSame (clause, groupClause.PreviousClause);
      Assert.AreSame (groupExpression, groupClause.GroupExpression);
      Assert.AreSame (byExpression, groupClause.ByExpression);
    }

    [Test]
    public void GroupClause_ImplementISelectGroupClause ()
    {
      Assert.IsInstanceOfType (typeof (ISelectGroupClause), _groupClause);
    }

    [Test]
    public void Accept()
    {
      MockRepository repository = new MockRepository();
      IQueryVisitor visitorMock = repository.StrictMock<IQueryVisitor>();
      visitorMock.VisitGroupClause (_groupClause);

      repository.ReplayAll();

      _groupClause.Accept (visitorMock);

      repository.VerifyAll();
    }

    [Test]
    public void Clone ()
    {
      var newPreviousClause = ExpressionHelper.CreateMainFromClause ();
      _clonedClauseMapping.AddMapping (_groupClause.PreviousClause, newPreviousClause);
      var clone = _groupClause.Clone (_clonedClauseMapping);

      Assert.That (clone, Is.Not.Null);
      Assert.That (clone, Is.Not.SameAs (_groupClause));
      Assert.That (clone.ByExpression, Is.SameAs (_groupClause.ByExpression));
      Assert.That (clone.GroupExpression, Is.SameAs (_groupClause.GroupExpression));
      Assert.That (clone.PreviousClause, Is.SameAs (newPreviousClause));
    }

    [Test]
    public void Clone_ViaInterface_PassesMapping ()
    {
      _clonedClauseMapping.AddMapping (_groupClause.PreviousClause, ExpressionHelper.CreateClause ());
      var clone = ((ISelectGroupClause) _groupClause).Clone (_clonedClauseMapping);
      Assert.That (_clonedClauseMapping.GetClause (_groupClause), Is.SameAs (clone));
    }

    [Test]
    public void Clone_AddsClauseToMapping ()
    {
      _clonedClauseMapping.AddMapping (_groupClause.PreviousClause, ExpressionHelper.CreateClause ());
      var clone = _groupClause.Clone (_clonedClauseMapping);
      Assert.That (_clonedClauseMapping.GetClause (_groupClause), Is.SameAs (clone));
    }
  }
}

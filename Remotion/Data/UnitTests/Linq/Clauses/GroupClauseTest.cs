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

namespace Remotion.Data.UnitTests.Linq.Clauses
{
  [TestFixture]
  public class GroupClauseTest
  {
    private GroupClause _groupClause;
    private CloneContext _cloneContext;

    [SetUp]
    public void SetUp ()
    {
      _groupClause = ExpressionHelper.CreateGroupClause ();
      _cloneContext = new CloneContext (new ClonedClauseMapping());
    }

    [Test]
    public void InitializeWithGroupExpressionAndByExpression()
    {
      Expression groupExpression = ExpressionHelper.CreateExpression();
      Expression byExpression = ExpressionHelper.CreateExpression();

      var groupClause = new GroupClause (groupExpression, byExpression);

      Assert.That (groupClause.GroupExpression, Is.SameAs (groupExpression));
      Assert.That (groupClause.ByExpression, Is.SameAs (byExpression));
    }

    [Test]
    public void GroupClause_ImplementISelectGroupClause ()
    {
      Assert.That (_groupClause, Is.InstanceOfType (typeof (ISelectGroupClause)));
    }

    [Test]
    public void Accept()
    {
      var repository = new MockRepository();
      var visitorMock = repository.StrictMock<IQueryVisitor>();
      visitorMock.VisitGroupClause (_groupClause);

      repository.ReplayAll();

      _groupClause.Accept (visitorMock);

      repository.VerifyAll();
    }

    [Test]
    public void Clone ()
    {
      var clone = _groupClause.Clone (_cloneContext);

      Assert.That (clone, Is.Not.Null);
      Assert.That (clone, Is.Not.SameAs (_groupClause));
      Assert.That (clone.ByExpression, Is.SameAs (_groupClause.ByExpression));
      Assert.That (clone.GroupExpression, Is.SameAs (_groupClause.GroupExpression));
    }

    [Test]
    public void Clone_AdjustsExpressions ()
    {
      var referencedExpression = ExpressionHelper.CreateMainFromClause();
      var groupExpression = new QuerySourceReferenceExpression (referencedExpression);
      var byExpression = new QuerySourceReferenceExpression (referencedExpression);
      var groupClause = new GroupClause (groupExpression, byExpression);

      var newReferencedExpression = ExpressionHelper.CreateMainFromClause ();
      _cloneContext.ClonedClauseMapping.AddMapping (referencedExpression, newReferencedExpression);

      var clone = groupClause.Clone (_cloneContext);

      Assert.That (((QuerySourceReferenceExpression) clone.GroupExpression).ReferencedClause, Is.SameAs (newReferencedExpression));
      Assert.That (((QuerySourceReferenceExpression) clone.ByExpression).ReferencedClause, Is.SameAs (newReferencedExpression));
    }

    [Test]
    public void Clone_ViaInterface_PassesMapping ()
    {
      var clone = ((ISelectGroupClause) _groupClause).Clone (_cloneContext);
      Assert.That (_cloneContext.ClonedClauseMapping.GetClause (_groupClause), Is.SameAs (clone));
    }

    [Test]
    public void Clone_AddsClauseToMapping ()
    {
      var clone = _groupClause.Clone (_cloneContext);
      Assert.That (_cloneContext.ClonedClauseMapping.GetClause (_groupClause), Is.SameAs (clone));
    }
  }
}

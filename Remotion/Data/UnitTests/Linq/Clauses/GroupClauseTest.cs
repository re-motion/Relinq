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
      _cloneContext = new CloneContext (new ClauseMapping());
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
      var queryModel = ExpressionHelper.CreateQueryModel ();
      var repository = new MockRepository();
      var visitorMock = repository.StrictMock<IQueryModelVisitor>();
      visitorMock.VisitGroupClause (_groupClause, queryModel);

      repository.ReplayAll();

      _groupClause.Accept (visitorMock, queryModel);

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
      _cloneContext.ClauseMapping.AddMapping (referencedExpression, new QuerySourceReferenceExpression(newReferencedExpression));

      var clone = groupClause.Clone (_cloneContext);

      Assert.That (((QuerySourceReferenceExpression) clone.GroupExpression).ReferencedClause, Is.SameAs (newReferencedExpression));
      Assert.That (((QuerySourceReferenceExpression) clone.ByExpression).ReferencedClause, Is.SameAs (newReferencedExpression));
    }

    [Test]
    public void Clone_ViaInterface_PassesMapping ()
    {
      var fromClause = ExpressionHelper.CreateMainFromClause ();
      _groupClause.GroupExpression = new QuerySourceReferenceExpression (fromClause);

      var newReferenceExpression = new QuerySourceReferenceExpression (ExpressionHelper.CreateMainFromClause ());
      _cloneContext.ClauseMapping.AddMapping (fromClause, newReferenceExpression);

      var clone = ((ISelectGroupClause) _groupClause).Clone (_cloneContext);
      Assert.That (((GroupClause) clone).GroupExpression, Is.SameAs (newReferenceExpression));
    }

    [Test]
    public void TransformExpressions ()
    {
      var oldGroupExpression = ExpressionHelper.CreateExpression ();
      var oldByExpression = ExpressionHelper.CreateExpression ();
      var newGroupExpression = ExpressionHelper.CreateExpression ();
      var newByExpression = ExpressionHelper.CreateExpression ();
      var clause = new GroupClause (oldGroupExpression, oldByExpression);

      clause.TransformExpressions (ex =>
          {
            if (ex == oldGroupExpression)
              return newGroupExpression;
            else if (ex == oldByExpression)
              return newByExpression;
            else
            {
              Assert.Fail();
              return null;
            }
          });

      Assert.That (clause.GroupExpression, Is.SameAs (newGroupExpression));
      Assert.That (clause.ByExpression, Is.SameAs (newByExpression));
    }

    [Test]
    public new void ToString ()
    {
      var groupClause = new GroupClause (Expression.Constant (0), Expression.Constant (1));

      Assert.That (groupClause.ToString (), Is.EqualTo ("group 0 by 1"));
    }
  }
}

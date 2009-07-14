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
  public class JoinClauseTest
  {
    private JoinClause _joinClause;
    private CloneContext _cloneContext;

    [SetUp]
    public void SetUp ()
    {
      _joinClause = ExpressionHelper.CreateJoinClause ();
      _cloneContext = new CloneContext (new QuerySourceMapping());
    }

    [Test]
    public void Intialize()
    {
      Expression inExpression = ExpressionHelper.CreateExpression ();
      Expression onExpression = ExpressionHelper.CreateExpression ();
      Expression equalityExpression = ExpressionHelper.CreateExpression ();

      var joinClause = new JoinClause ("x", typeof(Student), inExpression, onExpression, equalityExpression);

      Assert.That (joinClause.ItemName, Is.SameAs ("x"));
      Assert.That (joinClause.ItemType, Is.SameAs (typeof (Student)));
      Assert.That (joinClause.InExpression, Is.SameAs (inExpression));
      Assert.That (joinClause.OnExpression, Is.SameAs (onExpression));
      Assert.That (joinClause.EqualityExpression, Is.SameAs (equalityExpression));
    }

    [Test]
    public void Accept ()
    {
      var repository = new MockRepository ();
      var queryModel = ExpressionHelper.CreateQueryModel ();
      var visitorMock = repository.StrictMock<IQueryModelVisitor> ();

      visitorMock.VisitJoinClause (_joinClause, queryModel, 1);

      repository.ReplayAll ();

      _joinClause.Accept (visitorMock, queryModel, 1);

      repository.VerifyAll ();
    }

    [Test]
    public void Clone ()
    {
      var clone = _joinClause.Clone (_cloneContext);

      Assert.That (clone, Is.Not.Null);
      Assert.That (clone, Is.Not.SameAs (_joinClause));
      Assert.That (clone.EqualityExpression, Is.SameAs (_joinClause.EqualityExpression));
      Assert.That (clone.ItemName, Is.SameAs (_joinClause.ItemName));
      Assert.That (clone.ItemType, Is.SameAs (_joinClause.ItemType));
      Assert.That (clone.InExpression, Is.SameAs (_joinClause.InExpression));
      Assert.That (clone.OnExpression, Is.SameAs (_joinClause.OnExpression));
    }

    [Test]
    public void Clone_AddsMapping ()
    {
      var clone = _joinClause.Clone (_cloneContext);
      Assert.That (((QuerySourceReferenceExpression) _cloneContext.QuerySourceMapping.GetExpression (_joinClause)).ReferencedClause,
          Is.SameAs (clone));
    }

    [Test]
    public void TransformExpressions ()
    {
      var oldInExpression = ExpressionHelper.CreateExpression ();
      var oldOnExpression = ExpressionHelper.CreateExpression ();
      var oldEqualityExpression = ExpressionHelper.CreateExpression ();
      var newInExpression = ExpressionHelper.CreateExpression ();
      var newOnExpression = ExpressionHelper.CreateExpression ();
      var newEqualityExpression = ExpressionHelper.CreateExpression ();

      var clause = new JoinClause ("x", typeof (Student), oldInExpression, oldOnExpression, oldEqualityExpression);

      clause.TransformExpressions (ex =>
          {
            if (ex == oldInExpression)
              return newInExpression;
            else if (ex == oldOnExpression)
              return newOnExpression;
            else if (ex == oldEqualityExpression)
              return newEqualityExpression;
            else
            {
              Assert.Fail();
              return null;
            }
          });

      Assert.That (clause.InExpression, Is.SameAs (newInExpression));
      Assert.That (clause.OnExpression, Is.SameAs (newOnExpression));
      Assert.That (clause.EqualityExpression, Is.SameAs (newEqualityExpression));
    }

    [Test]
    public new void ToString ()
    {
      var joinClause = new JoinClause ("x", typeof (Student), Expression.Constant (0), Expression.Constant (1), Expression.Constant (2));
      Assert.That (joinClause.ToString (), Is.EqualTo ("join Student x in 0 on 1 equals 2"));
    }
  }
}

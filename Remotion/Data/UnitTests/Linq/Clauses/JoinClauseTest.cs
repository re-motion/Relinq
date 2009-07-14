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
      Expression innerSequence = ExpressionHelper.CreateExpression ();
      Expression innerKeySelector = ExpressionHelper.CreateExpression ();
      Expression outerKeySelector = ExpressionHelper.CreateExpression ();

      var joinClause = new JoinClause ("x", typeof(Student), innerSequence, innerKeySelector, outerKeySelector);

      Assert.That (joinClause.ItemName, Is.SameAs ("x"));
      Assert.That (joinClause.ItemType, Is.SameAs (typeof (Student)));
      Assert.That (joinClause.InnerSequence, Is.SameAs (innerSequence));
      Assert.That (joinClause.InnerKeySelector, Is.SameAs (innerKeySelector));
      Assert.That (joinClause.OuterKeySelector, Is.SameAs (outerKeySelector));
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
      Assert.That (clone.ItemName, Is.SameAs (_joinClause.ItemName));
      Assert.That (clone.ItemType, Is.SameAs (_joinClause.ItemType));
      Assert.That (clone.InnerSequence, Is.SameAs (_joinClause.InnerSequence));
      Assert.That (clone.InnerKeySelector, Is.SameAs (_joinClause.InnerKeySelector));
      Assert.That (clone.OuterKeySelector, Is.SameAs (_joinClause.OuterKeySelector));
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
      var oldInnerSequence = ExpressionHelper.CreateExpression ();
      var oldOuterKeySelector = ExpressionHelper.CreateExpression ();
      var oldInnerKeySelector = ExpressionHelper.CreateExpression ();
      var newInnerSequence = ExpressionHelper.CreateExpression ();
      var newOuterKeySelector = ExpressionHelper.CreateExpression ();
      var newInnerKeySelector = ExpressionHelper.CreateExpression ();

      var clause = new JoinClause ("x", typeof (Student), oldInnerSequence, oldInnerKeySelector, oldOuterKeySelector);

      clause.TransformExpressions (ex =>
          {
            if (ex == oldInnerSequence)
              return newInnerSequence;
            else if (ex == oldOuterKeySelector)
              return newOuterKeySelector;
            else if (ex == oldInnerKeySelector)
              return newInnerKeySelector;
            else
            {
              Assert.Fail();
              return null;
            }
          });

      Assert.That (clause.InnerSequence, Is.SameAs (newInnerSequence));
      Assert.That (clause.InnerKeySelector, Is.SameAs (newInnerKeySelector));
      Assert.That (clause.OuterKeySelector, Is.SameAs (newOuterKeySelector));
    }

    [Test]
    public new void ToString ()
    {
      var joinClause = new JoinClause ("x", typeof (Student), Expression.Constant (0), Expression.Constant (2), Expression.Constant (1));
      Assert.That (joinClause.ToString (), Is.EqualTo ("join Student x in 0 on 1 equals 2"));
    }
  }
}

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
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.UnitTests.Linq.TestDomain;
using Remotion.Utilities;
using Rhino.Mocks;

namespace Remotion.Data.UnitTests.Linq.Clauses
{
  [TestFixture]
  public class GroupJoinClauseTest
  {
    private GroupJoinClause _groupJoinClause;
    private JoinClause _joinClause;
    private CloneContext _cloneContext;

    [SetUp]
    public void SetUp ()
    {
      _joinClause = ExpressionHelper.CreateJoinClause ();
      _groupJoinClause = ExpressionHelper.CreateGroupJoinClause (_joinClause);
      _cloneContext = new CloneContext (new QuerySourceMapping ());
    }

    [Test]
    public void Intialize ()
    {
      var groupJoinClause = new GroupJoinClause ("x", typeof (IEnumerable<Student>), _joinClause);

      Assert.That (groupJoinClause.ItemName, Is.SameAs ("x"));
      Assert.That (groupJoinClause.ItemType, Is.SameAs (typeof (IEnumerable<Student>)));
      Assert.That (groupJoinClause.JoinClause, Is.SameAs (_joinClause));
    }

    [Test]
    [ExpectedException (typeof (ArgumentTypeException))]
    public void Intialize_WithNonEnumerableType_Throws ()
    {
      new GroupJoinClause ("x", typeof (Student), _joinClause);
    }

    [Test]
    public void Accept ()
    {
      var repository = new MockRepository ();
      var queryModel = ExpressionHelper.CreateQueryModel_Student ();
      var visitorMock = repository.StrictMock<IQueryModelVisitor> ();

      visitorMock.VisitGroupJoinClause (_groupJoinClause, queryModel, 1);

      repository.ReplayAll ();

      _groupJoinClause.Accept (visitorMock, queryModel, 1);

      repository.VerifyAll ();
    }

    [Test]
    public void Clone ()
    {
      var clone = _groupJoinClause.Clone (_cloneContext);

      Assert.That (clone, Is.Not.Null);
      Assert.That (clone, Is.Not.SameAs (_groupJoinClause));
      Assert.That (clone.ItemName, Is.SameAs (_groupJoinClause.ItemName));
      Assert.That (clone.ItemType, Is.SameAs (_groupJoinClause.ItemType));
      Assert.That (clone.JoinClause, Is.Not.SameAs (_groupJoinClause.JoinClause));
      Assert.That (clone.JoinClause.ToString(), Is.EqualTo (_groupJoinClause.JoinClause.ToString()));
    }

    [Test]
    public void Clone_PassesMapping ()
    {
      _groupJoinClause.Clone (_cloneContext);
      Assert.That (_cloneContext.QuerySourceMapping.ContainsMapping (_joinClause), Is.True);
    }

    [Test]
    public void Clone_AddsMapping ()
    {
      var clone = _groupJoinClause.Clone (_cloneContext);
      Assert.That (((QuerySourceReferenceExpression) _cloneContext.QuerySourceMapping.GetExpression (_groupJoinClause)).ReferencedQuerySource,
          Is.SameAs (clone));
    }

    [Test]
    public void TransformExpressions ()
    {
      var joinClauseMock = MockRepository.GenerateMock<JoinClause> (
          "x", 
          typeof (Student), 
          ExpressionHelper.CreateExpression(), 
          ExpressionHelper.CreateExpression(), 
          ExpressionHelper.CreateExpression());
      _groupJoinClause.JoinClause = joinClauseMock;

      Func<Expression, Expression> transformation = ex => ex;
      _groupJoinClause.TransformExpressions (transformation);

      joinClauseMock.AssertWasCalled (mock => mock.TransformExpressions (transformation));
    }

    [Test]
    public new void ToString ()
    {
      var joinClause = new JoinClause ("x", typeof (Student), Expression.Constant (0), Expression.Constant (1), Expression.Constant (2));
      var groupJoinClause = new GroupJoinClause ("y", typeof (IEnumerable<Student>), joinClause);
      Assert.That (groupJoinClause.ToString (), Is.EqualTo ("join Student x in 0 on 1 equals 2 into IEnumerable`1 y"));
    }
  }
}
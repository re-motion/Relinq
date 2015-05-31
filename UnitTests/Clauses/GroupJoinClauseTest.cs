// Copyright (c) rubicon IT GmbH, www.rubicon.eu
//
// See the NOTICE file distributed with this work for additional information
// regarding copyright ownership.  rubicon licenses this file to you under 
// the Apache License, Version 2.0 (the "License"); you may not use this 
// file except in compliance with the License.  You may obtain a copy of the 
// License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, WITHOUT 
// WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.  See the 
// License for the specific language governing permissions and limitations
// under the License.
// 
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.UnitTests.TestDomain;
using Rhino.Mocks;

namespace Remotion.Linq.UnitTests.Clauses
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
      _joinClause = ExpressionHelper.CreateJoinClause<Cook> ();
      _groupJoinClause = ExpressionHelper.CreateGroupJoinClause<Cook> (_joinClause);
      _cloneContext = new CloneContext (new QuerySourceMapping ());
    }

    [Test]
    public void Intialize ()
    {
      var groupJoinClause = new GroupJoinClause ("x", typeof (IEnumerable<Cook>), _joinClause);

      Assert.That (groupJoinClause.ItemName, Is.SameAs ("x"));
      Assert.That (groupJoinClause.ItemType, Is.SameAs (typeof (IEnumerable<Cook>)));
      Assert.That (groupJoinClause.JoinClause, Is.SameAs (_joinClause));
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = 
        "Expected a closed generic type implementing IEnumerable<T>, but found 'Remotion.Linq.UnitTests.TestDomain.Cook'.\r\nParameter name: value")]
    public void Intialize_WithNonEnumerableType_Throws ()
    {
      new GroupJoinClause ("x", typeof (Cook), _joinClause);
    }

    [Test]
    public void Accept ()
    {
      var repository = new MockRepository ();
      var queryModel = ExpressionHelper.CreateQueryModel<Cook> ();
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
      var oldExpression = _joinClause.OuterKeySelector;
      var newExpression = ExpressionHelper.CreateExpression();
      Func<Expression, Expression> transformation = ex => (ex == oldExpression ? newExpression : ex);

      _groupJoinClause.TransformExpressions (transformation);

      Assert.That (_joinClause.OuterKeySelector, Is.SameAs (newExpression));
    }

    [Test]
    public new void ToString ()
    {
      var joinClause = new JoinClause ("x", typeof (Cook), Expression.Constant (0), Expression.Constant (1), Expression.Constant (2));
      var groupJoinClause = new GroupJoinClause ("y", typeof (IEnumerable<Cook>), joinClause);
      Assert.That (groupJoinClause.ToString (), Is.EqualTo ("join Cook x in 0 on 1 equals 2 into IEnumerable`1 y"));
    }
  }
}

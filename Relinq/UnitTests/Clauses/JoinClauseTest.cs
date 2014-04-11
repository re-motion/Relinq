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
  public class JoinClauseTest
  {
    private JoinClause _joinClause;
    private CloneContext _cloneContext;

    [SetUp]
    public void SetUp ()
    {
      _joinClause = ExpressionHelper.CreateJoinClause<Cook> ();
      _cloneContext = new CloneContext (new QuerySourceMapping());
    }

    [Test]
    public void Intialize()
    {
      Expression innerSequence = ExpressionHelper.CreateExpression ();
      Expression outerKeySelector = ExpressionHelper.CreateExpression ();
      Expression innerKeySelector = ExpressionHelper.CreateExpression ();

      var joinClause = new JoinClause ("x", typeof(Cook), innerSequence, outerKeySelector, innerKeySelector);

      Assert.That (joinClause.ItemName, Is.SameAs ("x"));
      Assert.That (joinClause.ItemType, Is.SameAs (typeof (Cook)));
      Assert.That (joinClause.InnerSequence, Is.SameAs (innerSequence));
      Assert.That (joinClause.InnerKeySelector, Is.SameAs (innerKeySelector));
      Assert.That (joinClause.OuterKeySelector, Is.SameAs (outerKeySelector));
    }

    [Test]
    public void Accept ()
    {
      var repository = new MockRepository ();
      var queryModel = ExpressionHelper.CreateQueryModel<Cook> ();
      var visitorMock = repository.StrictMock<IQueryModelVisitor> ();

      visitorMock.VisitJoinClause (_joinClause, queryModel, 1);

      repository.ReplayAll ();

      _joinClause.Accept (visitorMock, queryModel, 1);

      repository.VerifyAll ();
    }

    [Test]
    public void Accept_WithGroupJoinClause ()
    {
      var repository = new MockRepository ();
      var queryModel = ExpressionHelper.CreateQueryModel<Cook> ();
      var groupJoinClause = ExpressionHelper.CreateGroupJoinClause<Cook> ();
      var visitorMock = repository.StrictMock<IQueryModelVisitor> ();

      visitorMock.VisitJoinClause (_joinClause, queryModel, groupJoinClause);

      repository.ReplayAll ();

      _joinClause.Accept (visitorMock, queryModel, groupJoinClause);

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
      Assert.That (((QuerySourceReferenceExpression) _cloneContext.QuerySourceMapping.GetExpression (_joinClause)).ReferencedQuerySource,
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

      var clause = new JoinClause ("x", typeof (Cook), oldInnerSequence, oldOuterKeySelector, oldInnerKeySelector);

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
      Assert.That (clause.OuterKeySelector, Is.SameAs (newOuterKeySelector));
      Assert.That (clause.InnerKeySelector, Is.SameAs (newInnerKeySelector));
    }

    [Test]
    public new void ToString ()
    {
      var joinClause = new JoinClause ("x", typeof (Cook), Expression.Constant (0), Expression.Constant (1), Expression.Constant (2));
      Assert.That (joinClause.ToString (), Is.EqualTo ("join Cook x in 0 on 1 equals 2"));
    }
  }
}

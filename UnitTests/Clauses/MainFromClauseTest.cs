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
using System.Linq;
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
  public class MainFromClauseTest
  {
    private MainFromClause _mainFromClause;
    private CloneContext _cloneContext;

    [SetUp]
    public void SetUp ()
    {
      _mainFromClause = ExpressionHelper.CreateMainFromClause_Int();
      _cloneContext = new CloneContext (new QuerySourceMapping());
    }

    [Test]
    public void Initialize ()
    {
      IQueryable querySource = ExpressionHelper.CreateQueryable<Cook> ();

      ConstantExpression constantExpression = Expression.Constant (querySource);
      var fromClause = new MainFromClause ("s", typeof (Cook), constantExpression);

      Assert.That (fromClause.ItemName, Is.EqualTo ("s"));
      Assert.That (fromClause.ItemType, Is.SameAs (typeof (Cook)));
      Assert.That (fromClause.FromExpression, Is.SameAs (constantExpression));
    }

    [Test]
    public void Initialize_WithNonConstantExpression ()
    {
      IQueryable querySource = ExpressionHelper.CreateQueryable<Cook> ();
      var anonymous = new {source = querySource};
      MemberExpression sourceExpression = Expression.MakeMemberAccess (Expression.Constant (anonymous), anonymous.GetType().GetProperty ("source"));

      var fromClause = new MainFromClause ("s", typeof (Cook), sourceExpression);
      Assert.That (fromClause.FromExpression, Is.SameAs (sourceExpression));
    }

    [Test]
    public void CopyFromSource ()
    {
      ConstantExpression constantExpression = Expression.Constant (ExpressionHelper.CreateQueryable<Cook> ());
      var fromClause = new MainFromClause ("s", typeof (Cook), constantExpression);

      ConstantExpression newExpression = Expression.Constant (ExpressionHelper.CreateQueryable<Kitchen>());
      var sourceStub = MockRepository.GenerateStub<IFromClause>();
      sourceStub.Stub (_ => _.ItemName).Return ("newItemName");
      sourceStub.Stub (_ => _.ItemType).Return (typeof (Kitchen));
      sourceStub.Stub (_ => _.FromExpression).Return (newExpression);

      fromClause.CopyFromSource (sourceStub);

      Assert.That (fromClause.ItemName, Is.EqualTo ("newItemName"));
      Assert.That (fromClause.ItemType, Is.SameAs (typeof (Kitchen)));
      Assert.That (fromClause.FromExpression, Is.SameAs (newExpression));
    }

    [Test]
    public void Accept ()
    {
      var repository = new MockRepository ();
      var visitorMock = repository.StrictMock<IQueryModelVisitor> ();
      var queryModel = ExpressionHelper.CreateQueryModel<Cook> ();

      visitorMock.VisitMainFromClause (_mainFromClause, queryModel);

      repository.ReplayAll ();

      _mainFromClause.Accept (visitorMock, queryModel);

      repository.VerifyAll ();
    }

    [Test]
    public void Clone ()
    {
      var clone = _mainFromClause.Clone (_cloneContext);

      Assert.That (clone, Is.Not.Null);
      Assert.That (clone, Is.Not.SameAs (_mainFromClause));
      Assert.That (clone.ItemName, Is.EqualTo (_mainFromClause.ItemName));
      Assert.That (clone.ItemType, Is.SameAs (_mainFromClause.ItemType));
      Assert.That (clone.FromExpression, Is.SameAs (_mainFromClause.FromExpression));
    }

    [Test]
    public void Clone_AddsClauseToMapping ()
    {
      var clone = _mainFromClause.Clone (_cloneContext);
      Assert.That (((QuerySourceReferenceExpression) _cloneContext.QuerySourceMapping.GetExpression (_mainFromClause)).ReferencedQuerySource, Is.SameAs (clone));
    }
  }
}

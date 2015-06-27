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
using NUnit.Framework;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.UnitTests.TestDomain;
using Rhino.Mocks;

namespace Remotion.Linq.UnitTests.Clauses
{
  [TestFixture]
  public class AdditionalFromClauseTest
  {
    private AdditionalFromClause _additionalFromClause;
    private CloneContext _cloneContext;

    [SetUp]
    public void SetUp ()
    {
      _additionalFromClause = ExpressionHelper.CreateAdditionalFromClause();
      _cloneContext = new CloneContext (new QuerySourceMapping ());      
    }

    [Test]
    public void Initialize ()
    {
      var fromExpression = ExpressionHelper.CreateExpression ();
      var fromClause = new AdditionalFromClause ("s", typeof (Cook), fromExpression);

      Assert.That (fromClause.ItemName, Is.EqualTo ("s"));
      Assert.That (fromClause.ItemType, Is.SameAs (typeof (Cook)));
      Assert.That (fromClause.FromExpression, Is.SameAs (fromExpression));
    }

    [Test]
    public void CopyFromSource ()
    {
      var fromExpression = ExpressionHelper.CreateExpression ();
      var fromClause = new AdditionalFromClause ("s", typeof (Cook), fromExpression);

      var newExpression = ExpressionHelper.CreateExpression ();
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
    public void ImplementInterface_IFromLetWhereClause ()
    {
      Assert.That (_additionalFromClause, Is.InstanceOf (typeof (IBodyClause)));
    }

    [Test]
    public void Accept ()
    {
      var queryModel = ExpressionHelper.CreateQueryModel<Cook> ();
      var visitorMock = MockRepository.GenerateMock<IQueryModelVisitor> ();
      _additionalFromClause.Accept (visitorMock, queryModel, 1);
      visitorMock.AssertWasCalled (mock => mock.VisitAdditionalFromClause (_additionalFromClause, queryModel, 1));
    }
    
    [Test]
    public void Clone ()
    {
      var clone = _additionalFromClause.Clone (_cloneContext);

      Assert.That (clone, Is.Not.Null);
      Assert.That (clone, Is.Not.SameAs (_additionalFromClause));
      Assert.That (clone.ItemName, Is.EqualTo (_additionalFromClause.ItemName));
      Assert.That (clone.ItemType, Is.SameAs (_additionalFromClause.ItemType));
      Assert.That (clone.FromExpression, Is.SameAs (_additionalFromClause.FromExpression));
    }

    [Test]
    public void Clone_AddsClauseToMapping ()
    {
      var clone = _additionalFromClause.Clone (_cloneContext);
      Assert.That (((QuerySourceReferenceExpression) _cloneContext.QuerySourceMapping.GetExpression (_additionalFromClause)).ReferencedQuerySource,
          Is.SameAs (clone));
    }
  }
}

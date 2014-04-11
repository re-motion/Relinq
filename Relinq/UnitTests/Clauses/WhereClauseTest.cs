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
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.UnitTests.TestDomain;
using Rhino.Mocks;

namespace Remotion.Linq.UnitTests.Clauses
{
  [TestFixture]
  public class WhereClauseTest
  {
    private WhereClause _whereClause;
    private CloneContext _cloneContext;

    [SetUp]
    public void SetUp ()
    {
      _whereClause = ExpressionHelper.CreateWhereClause();
      _cloneContext = new CloneContext (new QuerySourceMapping());
    }

    [Test] 
    public void InitializeWithboolExpression()
    {
      var predicate = Expression.Constant (false);
      var whereClause = new WhereClause(predicate);

      Assert.That (whereClause.Predicate, Is.SameAs (predicate));
    }

    [Test]
    public void ImplementInterface()
    {
      Assert.That (_whereClause, Is.InstanceOf (typeof (IBodyClause)));
    }
    
    [Test]
    public void Accept()
    {
      var queryModel = ExpressionHelper.CreateQueryModel<Cook> ();
      var repository = new MockRepository ();
      var visitorMock = repository.StrictMock<IQueryModelVisitor> ();

      visitorMock.VisitWhereClause (_whereClause, queryModel, 1);

      repository.ReplayAll();

      _whereClause.Accept (visitorMock, queryModel, 1);

      repository.VerifyAll();
    }

    [Test]
    public void Clone ()
    {
      var clone = _whereClause.Clone (_cloneContext);

      Assert.That (clone, Is.Not.Null);
      Assert.That (clone, Is.Not.SameAs (_whereClause));
      Assert.That (clone.Predicate, Is.SameAs (_whereClause.Predicate));
    }

    [Test]
    public void TransformExpressions ()
    {
      var oldExpression = ExpressionHelper.CreateExpression ();
      var newExpression = ExpressionHelper.CreateExpression ();
      var clause = new WhereClause (oldExpression);

      clause.TransformExpressions (ex =>
          {
            Assert.That (ex, Is.SameAs (oldExpression));
            return newExpression;
          });

      Assert.That (clause.Predicate, Is.SameAs (newExpression));
    }

    [Test]
    public new void ToString ()
    {
      var whereClause = new WhereClause (Expression.Constant (true));

      Assert.That (whereClause.ToString (), Is.EqualTo ("where True"));
    }
  }
}

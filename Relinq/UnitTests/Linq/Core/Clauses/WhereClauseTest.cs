// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// as published by the Free Software Foundation; either version 2.1 of the 
// License, or (at your option) any later version.
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
using Remotion.Linq.Clauses;
using Rhino.Mocks;

namespace Remotion.Linq.UnitTests.Linq.Core.Clauses
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
      Assert.That (_whereClause, Is.InstanceOfType (typeof (IBodyClause)));
    }
    
    [Test]
    public void Accept()
    {
      var queryModel = ExpressionHelper.CreateQueryModel_Cook ();
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

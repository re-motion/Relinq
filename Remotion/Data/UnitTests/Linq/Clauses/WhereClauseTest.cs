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
using Rhino.Mocks;
using Remotion.Data.Linq.Clauses;

namespace Remotion.Data.UnitTests.Linq.Clauses
{
  [TestFixture]
  public class WhereClauseTest
  {
    private WhereClause _whereClause;

    [SetUp]
    public void SetUp ()
    {
      _whereClause = ExpressionHelper.CreateWhereClause();
    }

    [Test] 
    public void InitializeWithboolExpression()
    {
      var predicate = Expression.Constant (false);
      IClause clause = ExpressionHelper.CreateClause();
      
      var whereClause = new WhereClause(clause, predicate);
      Assert.That (whereClause.PreviousClause, Is.SameAs (clause));
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
      var repository = new MockRepository ();
      var visitorMock = repository.StrictMock<IQueryVisitor> ();

      visitorMock.VisitWhereClause (_whereClause);

      repository.ReplayAll();

      _whereClause.Accept (visitorMock);

      repository.VerifyAll();
    }

    [Test]
    public void QueryModelAtInitialization ()
    {
      Assert.That (_whereClause.QueryModel, Is.Null);
    }

    [Test]
    public void SetQueryModel ()
    {
      QueryModel model = ExpressionHelper.CreateQueryModel ();
      _whereClause.SetQueryModel (model);
      Assert.That (_whereClause.QueryModel, Is.Not.Null);
    }

    [Test]
    [ExpectedException (typeof (ArgumentNullException))]
    public void SetQueryModelWithNull_Exception ()
    {
      _whereClause.SetQueryModel (null);
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "QueryModel is already set")]
    public void SetQueryModelTwice_Exception ()
    {
      QueryModel model = ExpressionHelper.CreateQueryModel ();
      _whereClause.SetQueryModel (model);
      _whereClause.SetQueryModel (model);
    }

    [Test]
    public void Clone ()
    {
      var newPreviousClause = ExpressionHelper.CreateClause ();
      var clone = _whereClause.Clone (newPreviousClause, new FromClauseMapping());

      Assert.That (clone, Is.Not.Null);
      Assert.That (clone, Is.Not.SameAs (_whereClause));
      Assert.That (clone.Predicate, Is.SameAs (_whereClause.Predicate));
      Assert.That (clone.PreviousClause, Is.SameAs (newPreviousClause));
      Assert.That (clone.QueryModel, Is.Null);
    }
  }
}

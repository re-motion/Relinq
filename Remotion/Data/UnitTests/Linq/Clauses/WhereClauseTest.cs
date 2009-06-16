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
    [Test] 
    public void InitializeWithboolExpression()
    {
      var predicate = Expression.Constant (false);
      LambdaExpression boolExpression = Expression.Lambda (predicate);
      IClause clause = ExpressionHelper.CreateClause();
      
      var whereClause = new WhereClause(clause, boolExpression, predicate);
      Assert.That (whereClause.PreviousClause, Is.SameAs (clause));
      Assert.That (whereClause.Predicate, Is.SameAs (predicate));
    }

    [Test]
    public void ImplementInterface()
    {
      WhereClause whereClause = ExpressionHelper.CreateWhereClause();
      Assert.That (whereClause, Is.InstanceOfType (typeof (IBodyClause)));
    }
    
    [Test]
    public void Accept()
    {
      WhereClause whereClause = ExpressionHelper.CreateWhereClause();

      var repository = new MockRepository ();
      var visitorMock = repository.StrictMock<IQueryVisitor> ();

      visitorMock.VisitWhereClause (whereClause);

      repository.ReplayAll();

      whereClause.Accept (visitorMock);

      repository.VerifyAll();
    }

    [Test]
    public void QueryModelAtInitialization ()
    {
      WhereClause whereClause = ExpressionHelper.CreateWhereClause ();
      Assert.That (whereClause.QueryModel, Is.Null);
    }

    [Test]
    public void SetQueryModel ()
    {
      WhereClause whereClause = ExpressionHelper.CreateWhereClause ();
      QueryModel model = ExpressionHelper.CreateQueryModel ();
      whereClause.SetQueryModel (model);
      Assert.That (whereClause.QueryModel, Is.Not.Null);
    }

    [Test]
    [ExpectedException (typeof (ArgumentNullException))]
    public void SetQueryModelWithNull_Exception ()
    {
      WhereClause whereClause = ExpressionHelper.CreateWhereClause ();
      whereClause.SetQueryModel (null);
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "QueryModel is already set")]
    public void SetQueryModelTwice_Exception ()
    {
      WhereClause whereClause = ExpressionHelper.CreateWhereClause ();
      QueryModel model = ExpressionHelper.CreateQueryModel ();
      whereClause.SetQueryModel (model);
      whereClause.SetQueryModel (model);
    }

    [Test]
    public void Clone ()
    {
      var originalClause = ExpressionHelper.CreateWhereClause ();
      var newPreviousClause = ExpressionHelper.CreateClause ();
      var clone = originalClause.Clone (newPreviousClause);

      Assert.That (clone, Is.Not.Null);
      Assert.That (clone, Is.Not.SameAs (originalClause));
      Assert.That (clone.LegacyPredicate, Is.SameAs (originalClause.LegacyPredicate));
      Assert.That (clone.Predicate, Is.SameAs (originalClause.Predicate));
      Assert.That (clone.PreviousClause, Is.SameAs (newPreviousClause));
      Assert.That (clone.QueryModel, Is.Null);
    }
  }
}

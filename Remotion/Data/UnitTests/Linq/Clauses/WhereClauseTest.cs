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
  public class WhereClauseTest
  {
    private WhereClause _whereClause;
    private CloneContext _cloneContext;

    [SetUp]
    public void SetUp ()
    {
      _whereClause = ExpressionHelper.CreateWhereClause();
      _cloneContext = new CloneContext (new ClonedClauseMapping());
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
    public void Clone ()
    {
      var newPreviousClause = ExpressionHelper.CreateClause ();
      _cloneContext.ClonedClauseMapping.AddMapping (_whereClause.PreviousClause, newPreviousClause);
      var clone = _whereClause.Clone (_cloneContext);

      Assert.That (clone, Is.Not.Null);
      Assert.That (clone, Is.Not.SameAs (_whereClause));
      Assert.That (clone.Predicate, Is.SameAs (_whereClause.Predicate));
      Assert.That (clone.PreviousClause, Is.SameAs (newPreviousClause));
    }

    [Test]
    public void Clone_ViaInterface_PassesMapping ()
    {
      _cloneContext.ClonedClauseMapping.AddMapping (_whereClause.PreviousClause, ExpressionHelper.CreateClause ());
      var clone = ((IBodyClause) _whereClause).Clone (_cloneContext);
      Assert.That (_cloneContext.ClonedClauseMapping.GetClause (_whereClause), Is.SameAs (clone));
    }

    [Test]
    public void Clone_AddsClauseToMapping ()
    {
      _cloneContext.ClonedClauseMapping.AddMapping (_whereClause.PreviousClause, ExpressionHelper.CreateClause());
      var clone = _whereClause.Clone (_cloneContext);
      Assert.That (_cloneContext.ClonedClauseMapping.GetClause (_whereClause), Is.SameAs (clone));
    }

    [Test]
    public void Clone_AdjustsExpressions ()
    {
      var mainFromClause = ExpressionHelper.CreateMainFromClause();
      var predicate = new QuerySourceReferenceExpression (mainFromClause);
      var whereClause = new WhereClause (mainFromClause, predicate);

      var newMainFromClause = ExpressionHelper.CreateMainFromClause ();
      _cloneContext.ClonedClauseMapping.AddMapping (mainFromClause, newMainFromClause);

      var clone = whereClause.Clone (_cloneContext);

      Assert.That (((QuerySourceReferenceExpression) clone.Predicate).ReferencedClause, Is.SameAs (newMainFromClause));
    }
  }
}

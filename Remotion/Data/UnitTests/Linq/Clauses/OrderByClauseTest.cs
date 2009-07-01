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
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq;
using Remotion.Data.Linq.Clauses.Expressions;
using Rhino.Mocks;
using Remotion.Data.Linq.Clauses;


namespace Remotion.Data.UnitTests.Linq.Clauses
{
  [TestFixture]
  public class OrderByClauseTest
  {
    private CloneContext _cloneContext;
    private OrderByClause _orderByClause;

    [SetUp]
    public void SetUp ()
    {
      _cloneContext = new CloneContext (new ClauseMapping ());
      _orderByClause = new OrderByClause ();
    }

    [Test]
    public void InitializeWithoutOrdering()
    {
      Assert.That (_orderByClause.Orderings.Count, Is.EqualTo (0));
    }
    
    [Test]
    public void AddOrderings()
    {
      Ordering ordering1 = ExpressionHelper.CreateOrdering ();
      Ordering ordering2 = ExpressionHelper.CreateOrdering ();

      _orderByClause.Orderings.Add (ordering1);
      _orderByClause.Orderings.Add (ordering2);

      Assert.That (_orderByClause.Orderings, Is.EqualTo (new object[] { ordering1, ordering2 }));
      Assert.That (_orderByClause.Orderings.Count, Is.EqualTo (2));
    }

    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void AddOrderings_Null_ThrowsArgumentNullException ()
    {
      _orderByClause.Orderings.Add (null);
    }

    [Test]
    [ExpectedException (typeof (ArgumentNullException))]
    public void ChangeOrdering_WithNull_ThrowsArgumentNullException ()
    {
      Ordering ordering1 = ExpressionHelper.CreateOrdering ();
      _orderByClause.Orderings.Add (ordering1);
      _orderByClause.Orderings[0] = null;
    }

    [Test]
    public void Accept()
    {
      var repository = new MockRepository();
      var queryModel = ExpressionHelper.CreateQueryModel ();
      var visitorMock = repository.StrictMock<IQueryModelVisitor>();

      visitorMock.VisitOrderByClause(_orderByClause, queryModel, 1);

      repository.ReplayAll();

      _orderByClause.Accept (visitorMock, queryModel, 1);

      repository.VerifyAll();
    }

    [Test]
    public void Clone ()
    {
      var clone = _orderByClause.Clone (_cloneContext);

      Assert.That (clone, Is.Not.Null);
      Assert.That (clone, Is.Not.SameAs (_orderByClause));
    }

    [Test]
    public void Clone_ViaInterface_PassesMappingToOrderings ()
    {
      var fromClause = ExpressionHelper.CreateMainFromClause ();
      _orderByClause.Orderings.Add (new Ordering (new QuerySourceReferenceExpression (fromClause), OrderingDirection.Asc));

      var newReferenceExpression = new QuerySourceReferenceExpression (ExpressionHelper.CreateMainFromClause ());
      _cloneContext.ClauseMapping.AddMapping (fromClause, newReferenceExpression);

      var clone = ((IBodyClause) _orderByClause).Clone (_cloneContext);
      Assert.That (((OrderByClause) clone).Orderings[0].Expression, Is.SameAs (newReferenceExpression));
    }

    [Test]
    public void Clone_Orderings ()
    {
      var ordering = ExpressionHelper.CreateOrdering();
      _orderByClause.Orderings.Add (ordering);

      var clone = _orderByClause.Clone (_cloneContext);

      Assert.That (clone.Orderings.Count, Is.EqualTo (1));

      Assert.That (clone.Orderings[0], Is.Not.SameAs (_orderByClause.Orderings[0]));
      Assert.That (clone.Orderings[0].Expression, Is.SameAs (_orderByClause.Orderings[0].Expression));
      Assert.That (clone.Orderings[0].OrderingDirection, Is.EqualTo (_orderByClause.Orderings[0].OrderingDirection));
    }

    [Test]
    public void TransformExpressions_PassedToOrderings ()
    {
      var ordering = ExpressionHelper.CreateOrdering ();
      _orderByClause.Orderings.Add (ordering);

      var expectedReplacement = ExpressionHelper.CreateExpression ();

      _orderByClause.TransformExpressions (ex => expectedReplacement);

      Assert.That (ordering.Expression, Is.SameAs (expectedReplacement));
    }
  }
}

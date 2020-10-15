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
  public class OrderByClauseTest
  {
    private CloneContext _cloneContext;
    private OrderByClause _orderByClause;

    [SetUp]
    public void SetUp ()
    {
      _cloneContext = new CloneContext (new QuerySourceMapping ());
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
    public void AddOrderings_Null_ThrowsArgumentNullException ()
    {
      Assert.That (
          () => _orderByClause.Orderings.Add (null),
          Throws.ArgumentNullException);
    }

    [Test]
    public void ChangeOrdering_WithNull_ThrowsArgumentNullException ()
    {
      Ordering ordering1 = ExpressionHelper.CreateOrdering ();
      _orderByClause.Orderings.Add (ordering1);
      Assert.That (
          () => _orderByClause.Orderings[0] = null,
          Throws.ArgumentNullException);
    }

    [Test]
    public void Accept()
    {
      var repository = new MockRepository();
      var queryModel = ExpressionHelper.CreateQueryModel<Cook> ();
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
      var referencedClause = ExpressionHelper.CreateMainFromClause_Int ();
      var ordering = new Ordering (new QuerySourceReferenceExpression (referencedClause), OrderingDirection.Asc);
      _orderByClause.Orderings.Add (ordering);

      var newReferenceExpression = new QuerySourceReferenceExpression (ExpressionHelper.CreateMainFromClause_Int ());
      _cloneContext.QuerySourceMapping.AddMapping (referencedClause, newReferenceExpression);

      var clone = (OrderByClause) ((IBodyClause) _orderByClause).Clone (_cloneContext);

      Assert.That (clone.Orderings.Count, Is.EqualTo (1));

      Assert.That (clone.Orderings[0], Is.Not.SameAs (ordering));
      Assert.That (clone.Orderings[0].OrderingDirection, Is.EqualTo (ordering.OrderingDirection));
      Assert.That (clone.Orderings[0].Expression, Is.SameAs (ordering.Expression));
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

    [Test]
    public new void ToString ()
    {
      Assert.That (_orderByClause.ToString (), Is.EqualTo ("orderby "));
    }

    [Test]
    public void ToString_WithOrdering ()
    {
      _orderByClause.Orderings.Add (new Ordering (Expression.Constant (0), OrderingDirection.Asc));

      Assert.That (_orderByClause.ToString (), Is.EqualTo ("orderby 0 asc"));
    }

    [Test]
    public void ToString_WithOrderings ()
    {
      _orderByClause.Orderings.Add (new Ordering (Expression.Constant (0), OrderingDirection.Asc));
      _orderByClause.Orderings.Add (new Ordering (Expression.Constant (1), OrderingDirection.Desc));

      Assert.That (_orderByClause.ToString (), Is.EqualTo ("orderby 0 asc, 1 desc"));
    }
  }
}

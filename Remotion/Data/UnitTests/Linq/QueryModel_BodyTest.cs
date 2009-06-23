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
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Data.Linq;
using Remotion.Data.Linq.Clauses;
using NUnit.Framework.SyntaxHelpers;

namespace Remotion.Data.UnitTests.Linq
{
  [TestFixture]
  public class QueryModel_BodyTest
  {
    private ISelectGroupClause _selectOrGroupClause;
    private QueryModel _model;

    [SetUp]
    public void SetUp()
    {
      _selectOrGroupClause = ExpressionHelper.CreateSelectClause ();
      _model = new QueryModel (typeof (IQueryable<int>), ExpressionHelper.CreateMainFromClause(), _selectOrGroupClause);
    }

    [Test]
    public void InitializeWithISelectOrGroupClauseAndOrderByClause()
    {
      var orderByClause = new OrderByClause (_model.MainFromClause);
      var ordering = new Ordering (orderByClause, ExpressionHelper.CreateExpression (), OrderingDirection.Asc);
      orderByClause.AddOrdering (ordering);

      _model.AddBodyClause (orderByClause);

      Assert.That (_model.SelectOrGroupClause, Is.SameAs (_selectOrGroupClause));
      Assert.That (_model.BodyClauses.Count, Is.EqualTo (1));
      Assert.That (_model.BodyClauses, List.Contains (orderByClause));
    }

    [Test]
    public void AddSeveralOrderByClauses()
    {
 
      IBodyClause orderByClause1 = ExpressionHelper.CreateOrderByClause();
      IBodyClause orderByClause2 = ExpressionHelper.CreateOrderByClause ();

      _model.AddBodyClause (orderByClause1);
      _model.AddBodyClause (orderByClause2);

      Assert.That (_model.BodyClauses.Count, Is.EqualTo (2));
      Assert.That (_model.BodyClauses, Is.EqualTo (new object[] { orderByClause1, orderByClause2 }));
    }

    
    [Test]
    public void AddBodyClause()
    {
      IBodyClause clause = ExpressionHelper.CreateWhereClause();
      _model.AddBodyClause (clause);

      Assert.That (_model.BodyClauses.Count, Is.EqualTo (1));
      Assert.That (_model.BodyClauses, List.Contains (clause));
    }
    
    [Test]
    public void AddBodyClause_SetsQueryModelOfBodyClause ()
    {
      IBodyClause clause = ExpressionHelper.CreateWhereClause ();
      _model.AddBodyClause (clause);
      Assert.That (clause.QueryModel, Is.Not.Null);
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "Multiple clauses with the same identifier name ('s') are not supported.")]
    public void AddFromClausesWithSameIdentifiers ()
    {
      IClause previousClause = ExpressionHelper.CreateClause();
      ParameterExpression identifier = Expression.Parameter (typeof (Student), "s");
      Expression fromExpression = ExpressionHelper.CreateExpression ();

      _model.AddBodyClause (new AdditionalFromClause (previousClause, identifier, fromExpression));
      _model.AddBodyClause (new AdditionalFromClause (previousClause, identifier, fromExpression));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "Multiple clauses with the same identifier name ('i') are not supported.")]
    public void AddBodyClause_WithSameIdentifierAsMainFromClause ()
    {
      IBodyClause clause = ExpressionHelper.CreateAdditionalFromClause (_model.MainFromClause.Identifier);
      _model.AddBodyClause (clause);
    }

  }
}

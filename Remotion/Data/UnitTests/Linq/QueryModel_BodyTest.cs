// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2008 rubicon informationstechnologie gmbh, www.rubicon.eu
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
      LambdaExpression orderingExpression = ExpressionHelper.CreateLambdaExpression ();

      var orderByClause = new OrderByClause (_model.MainFromClause);
      var ordering = new Ordering (orderByClause, orderingExpression, OrderingDirection.Asc);
      orderByClause.AddOrdering (ordering);

      _model.AddBodyClause (orderByClause);

      Assert.AreSame (_selectOrGroupClause, _model.SelectOrGroupClause);
      Assert.AreEqual (1, _model.BodyClauses.Count);
      Assert.That (_model.BodyClauses, List.Contains (orderByClause));
    }

    [Test]
    public void AddSeveralOrderByClauses()
    {
 
      IBodyClause orderByClause1 = ExpressionHelper.CreateOrderByClause();
      IBodyClause orderByClause2 = ExpressionHelper.CreateOrderByClause ();

      _model.AddBodyClause (orderByClause1);
      _model.AddBodyClause (orderByClause2);

      Assert.AreEqual (2, _model.BodyClauses.Count);
      Assert.That (_model.BodyClauses, Is.EqualTo (new object[] { orderByClause1, orderByClause2 }));
    }

    
    [Test]
    public void AddBodyClause()
    {
      IBodyClause clause = ExpressionHelper.CreateWhereClause();
      _model.AddBodyClause (clause);

      Assert.AreEqual (1, _model.BodyClauses.Count);
      Assert.That (_model.BodyClauses, List.Contains (clause));
    }

    [Test]
    public void AddBodyClause_RegistersFromClause ()
    {
      ParameterExpression identifier = Expression.Parameter(typeof(int),"j");
      AdditionalFromClause additionalFromClause = ExpressionHelper.CreateAdditionalFromClause (identifier);
      _model.AddBodyClause (additionalFromClause);
      IResolveableClause resolveableClause = _model.GetResolveableClause (identifier.Name, identifier.Type);

      Assert.AreEqual (resolveableClause, additionalFromClause);
    }

    [Test]
    public void AddBodyClause_RegistersLetClause ()
    {
      ParameterExpression identifier = Expression.Parameter (typeof (int), "j");
      LetClause letClause = ExpressionHelper.CreateLetClause (identifier);
      _model.AddBodyClause (letClause);
      IResolveableClause resolveableClause = _model.GetResolveableClause (identifier.Name, identifier.Type);

      Assert.AreEqual (resolveableClause, letClause);
    }

    [Test]
    public void AddBodyClause_SetsQueryModelOfBodyClause ()
    {
      IBodyClause clause = ExpressionHelper.CreateWhereClause ();
      _model.AddBodyClause (clause);
      Assert.IsNotNull (clause.QueryModel);
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "Multiple clauses with the same identifier name ('s') are not supported.")]
    public void AddFromClausesWithSameIdentifiers ()
    {
      IClause previousClause = ExpressionHelper.CreateClause();
      ParameterExpression identifier = Expression.Parameter (typeof (Student), "s");
      LambdaExpression fromExpression = ExpressionHelper.CreateLambdaExpression ();
      LambdaExpression projectionExpression = ExpressionHelper.CreateLambdaExpression ();

      _model.AddBodyClause (new AdditionalFromClause (previousClause, identifier, fromExpression, projectionExpression));
      _model.AddBodyClause (new AdditionalFromClause (previousClause, identifier, fromExpression, projectionExpression));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "Multiple clauses with the same identifier name ('i') are not supported.")]
    public void AddBodyClause_WithSameIdentifierAsMainFromClause ()
    {
      IBodyClause clause = ExpressionHelper.CreateAdditionalFromClause (_model.MainFromClause.Identifier);
      _model.AddBodyClause (clause);
    }

    
    [Test]
    public void GetResolveableClause()
    {
      LambdaExpression fromExpression = ExpressionHelper.CreateLambdaExpression ();
      LambdaExpression projExpression = ExpressionHelper.CreateLambdaExpression ();

      ParameterExpression identifier1 = Expression.Parameter (typeof (Student), "s1");
      ParameterExpression identifier2 = Expression.Parameter (typeof (Student), "s2");
      ParameterExpression identifier3 = Expression.Parameter (typeof (Student), "s3");

      AdditionalFromClause clause1 = new AdditionalFromClause (ExpressionHelper.CreateMainFromClause (), identifier1, fromExpression, projExpression);
      AdditionalFromClause clause2 = new AdditionalFromClause (clause1, identifier2, fromExpression, projExpression);
      AdditionalFromClause clause3 = new AdditionalFromClause (clause2, identifier3, fromExpression, projExpression);

      _model.AddBodyClause (clause1);
      _model.AddBodyClause (clause2);
      _model.AddBodyClause (clause3);

      Assert.AreSame (clause1, _model.GetResolveableClause ("s1", typeof (Student)));
      Assert.AreSame (clause2, _model.GetResolveableClause ("s2", typeof (Student)));
      Assert.AreSame (clause3, _model.GetResolveableClause ("s3", typeof (Student)));
    }

    [Test]
    public void GetResolveableClause_InvalidName ()
    {
      LambdaExpression fromExpression = ExpressionHelper.CreateLambdaExpression ();
      LambdaExpression projExpression = ExpressionHelper.CreateLambdaExpression ();
      ParameterExpression identifier1 = Expression.Parameter (typeof (Student), "s1");
      AdditionalFromClause clause1 = new AdditionalFromClause (ExpressionHelper.CreateMainFromClause (), identifier1, fromExpression, projExpression);

      _model.AddBodyClause (clause1);

      Assert.IsNull (_model.GetResolveableClause ("fzlbf", typeof (Student)));
    }

    [Test]
    [ExpectedException (typeof (ClauseLookupException), ExpectedMessage = "The from clause with identifier 's1' has type "
        + "'Remotion.Data.UnitTests.Linq.Student', but 'System.String' was requested.")]
    public void GetResolveableClause_InvalidType ()
    {
      LambdaExpression fromExpression = ExpressionHelper.CreateLambdaExpression ();
      LambdaExpression projExpression = ExpressionHelper.CreateLambdaExpression ();
      ParameterExpression identifier1 = Expression.Parameter (typeof (Student), "s1");
      AdditionalFromClause clause1 = new AdditionalFromClause (ExpressionHelper.CreateMainFromClause (), identifier1, fromExpression, projExpression);

      _model.AddBodyClause (clause1);
      _model.GetResolveableClause ("s1", typeof (string));
    }
  }
}

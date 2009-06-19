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
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq;
using Remotion.Data.Linq.Clauses.Expressions;
using Rhino.Mocks;
using Remotion.Data.Linq.Clauses;

namespace Remotion.Data.UnitTests.Linq.Clauses
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
      _cloneContext = new CloneContext (new ClonedClauseMapping (), new SubQueryRegistry());      
    }

    [Test]
    public void Initialize_WithIDAndExpression ()
    {
      ParameterExpression id = ExpressionHelper.CreateParameterExpression ();
      LambdaExpression fromExpression = ExpressionHelper.CreateLambdaExpression ();

      IClause clause = ExpressionHelper.CreateClause();
      
      var fromClause = new AdditionalFromClause (clause,id, fromExpression);

      Assert.AreSame (id, fromClause.Identifier);
      Assert.AreSame (fromExpression, fromClause.FromExpression);
      Assert.AreSame (clause, fromClause.PreviousClause);

      Assert.That (fromClause.JoinClauses, Is.Empty);
      Assert.AreEqual (0, fromClause.JoinClauses.Count);
    }

    [Test]
    public void ImplementInterface_IFromLetWhereClause ()
    {
      Assert.IsInstanceOfType (typeof (IBodyClause), _additionalFromClause);
    }

    [Test]
    public void GetQueriedEntityType ()
    {
      IQueryable<Student> querySource = ExpressionHelper.CreateQuerySource();
      LambdaExpression fromExpression = Expression.Lambda (Expression.Constant (querySource), Expression.Parameter (typeof (Student), "student"));
      var fromClause = new AdditionalFromClause (ExpressionHelper.CreateClause(), ExpressionHelper.CreateParameterExpression(), fromExpression);
      Assert.AreSame (typeof (TestQueryable<Student>), fromClause.GetQuerySourceType());
    }

    [Test]
    public void Accept ()
    {
      var visitorMock = MockRepository.GenerateMock<IQueryVisitor> ();
      _additionalFromClause.Accept (visitorMock);
      visitorMock.AssertWasCalled (mock => mock.VisitAdditionalFromClause (_additionalFromClause));
    }

    [Test]
    public void QueryModelAtInitialization ()
    {
      Assert.IsNull (_additionalFromClause.QueryModel);
    }

    [Test]
    public void SetQueryModel ()
    {
      QueryModel model = ExpressionHelper.CreateQueryModel ();
      _additionalFromClause.SetQueryModel (model);
      Assert.IsNotNull (_additionalFromClause.QueryModel);
    }

    [Test]
    [ExpectedException (typeof (ArgumentNullException))]
    public void SetQueryModelWithNull_Exception ()
    {
      _additionalFromClause.SetQueryModel (null);
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "QueryModel is already set")]
    public void SetQueryModelTwice_Exception ()
    {
      QueryModel model = ExpressionHelper.CreateQueryModel ();
      _additionalFromClause.SetQueryModel (model);
      _additionalFromClause.SetQueryModel (model);
    }

    [Test]
    public void Clone ()
    {
      var newPreviousClause = ExpressionHelper.CreateMainFromClause ();
      _cloneContext.ClonedClauseMapping.AddMapping (_additionalFromClause.PreviousClause, newPreviousClause);
      var clone = _additionalFromClause.Clone (_cloneContext);

      Assert.That (clone, Is.Not.Null);
      Assert.That (clone, Is.Not.SameAs (_additionalFromClause));
      Assert.That (clone.Identifier, Is.SameAs (_additionalFromClause.Identifier));
      Assert.That (clone.FromExpression, Is.SameAs (_additionalFromClause.FromExpression));
      Assert.That (clone.PreviousClause, Is.SameAs (newPreviousClause));
      Assert.That (clone.QueryModel, Is.Null);
    }

    [Test]
    public void Clone_AdjustsExpressions ()
    {
      var mainFromClause = ExpressionHelper.CreateMainFromClause();
      var fromExpression = new QuerySourceReferenceExpression (mainFromClause);
      var additionalFromClause = new AdditionalFromClause (
          mainFromClause, 
          ExpressionHelper.CreateParameterExpression(), 
          Expression.Lambda (fromExpression));

      var newMainFromClause = ExpressionHelper.CreateMainFromClause ();
      _cloneContext.ClonedClauseMapping.AddMapping (mainFromClause, newMainFromClause);

      var clone = additionalFromClause.Clone (_cloneContext);

      Assert.That (((QuerySourceReferenceExpression) clone.FromExpression.Body).ReferencedClause, Is.SameAs (newMainFromClause));
    }

    [Test]
    public void Clone_ViaInterface_PassesMapping ()
    {
      _cloneContext.ClonedClauseMapping.AddMapping (_additionalFromClause.PreviousClause, ExpressionHelper.CreateClause ());
      var clone = ((IBodyClause) _additionalFromClause).Clone (_cloneContext);
      Assert.That (_cloneContext.ClonedClauseMapping.GetClause (_additionalFromClause), Is.SameAs (clone));
    }

    [Test]
    public void Clone_JoinClauses ()
    {
      var originalJoinClause1 = ExpressionHelper.CreateJoinClause (_additionalFromClause, _additionalFromClause);
      _additionalFromClause.AddJoinClause (originalJoinClause1);

      var originalJoinClause2 = ExpressionHelper.CreateJoinClause (originalJoinClause1, _additionalFromClause);
      _additionalFromClause.AddJoinClause (originalJoinClause2);

      var newPreviousClause = ExpressionHelper.CreateClause();
      _cloneContext.ClonedClauseMapping.AddMapping (_additionalFromClause.PreviousClause, newPreviousClause);
      var clone = _additionalFromClause.Clone (_cloneContext);
      Assert.That (clone.JoinClauses.Count, Is.EqualTo (2));

      Assert.That (clone.JoinClauses[0], Is.Not.SameAs (originalJoinClause1));
      Assert.That (clone.JoinClauses[0].EqualityExpression, Is.SameAs (originalJoinClause1.EqualityExpression));
      Assert.That (clone.JoinClauses[0].InExpression, Is.SameAs (originalJoinClause1.InExpression));
      Assert.That (clone.JoinClauses[0].FromClause, Is.SameAs (clone));
      Assert.That (clone.JoinClauses[0].PreviousClause, Is.SameAs (clone));

      Assert.That (clone.JoinClauses[1], Is.Not.SameAs (originalJoinClause2));
      Assert.That (clone.JoinClauses[1].EqualityExpression, Is.SameAs (originalJoinClause2.EqualityExpression));
      Assert.That (clone.JoinClauses[1].InExpression, Is.SameAs (originalJoinClause2.InExpression));
      Assert.That (clone.JoinClauses[1].FromClause, Is.SameAs (clone));
      Assert.That (clone.JoinClauses[1].PreviousClause, Is.SameAs (clone.JoinClauses[0]));
    }

    [Test]
    public void Clone_JoinClauses_PassesMapping ()
    {
      var oldFromClause = ExpressionHelper.CreateMainFromClause ();
      var originalJoinClause = new JoinClause (
          _additionalFromClause,
          _additionalFromClause,
          ExpressionHelper.CreateParameterExpression (),
          new QuerySourceReferenceExpression (oldFromClause),
          ExpressionHelper.CreateExpression (),
          ExpressionHelper.CreateExpression ());
      _additionalFromClause.AddJoinClause (originalJoinClause);

      var newFromClause = ExpressionHelper.CreateMainFromClause ();
      _cloneContext.ClonedClauseMapping.AddMapping (oldFromClause, newFromClause);
      _cloneContext.ClonedClauseMapping.AddMapping (_additionalFromClause.PreviousClause, ExpressionHelper.CreateClause ());

      var clone = _additionalFromClause.Clone (_cloneContext);
      Assert.That (((QuerySourceReferenceExpression) clone.JoinClauses[0].InExpression).ReferencedClause, Is.SameAs (newFromClause));
    }

    [Test]
    public void Clone_AddsClauseToMapping ()
    {
      _cloneContext.ClonedClauseMapping.AddMapping (_additionalFromClause.PreviousClause, ExpressionHelper.CreateClause ());
      var clone = _additionalFromClause.Clone (_cloneContext);
      Assert.That (_cloneContext.ClonedClauseMapping.GetClause (_additionalFromClause), Is.SameAs (clone));
    }
  }
}

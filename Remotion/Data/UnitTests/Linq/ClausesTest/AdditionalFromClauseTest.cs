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
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq;
using Rhino.Mocks;
using Remotion.Data.Linq.Clauses;

namespace Remotion.Data.UnitTests.Linq.ClausesTest
{
  [TestFixture]
  public class AdditionalFromClauseTest
  {
    [Test]
    public void Initialize_WithIDAndExpression ()
    {
      ParameterExpression id = ExpressionHelper.CreateParameterExpression ();
      LambdaExpression fromExpression = ExpressionHelper.CreateLambdaExpression ();
      LambdaExpression projectionExpression = ExpressionHelper.CreateLambdaExpression ();

      IClause clause = ExpressionHelper.CreateClause();
      
      var fromClause = new AdditionalFromClause (clause,id, fromExpression, projectionExpression);

      Assert.AreSame (id, fromClause.Identifier);
      Assert.AreSame (fromExpression, fromClause.FromExpression);
      Assert.AreSame (projectionExpression, fromClause.ProjectionExpression);
      Assert.AreSame (clause, fromClause.PreviousClause);

      Assert.That (fromClause.JoinClauses, Is.Empty);
      Assert.AreEqual (0, fromClause.JoinClauses.Count);
    }

    [Test]
    public void ImplementInterface_IFromLetWhereClause ()
    {
      AdditionalFromClause fromClause = ExpressionHelper.CreateAdditionalFromClause ();
      Assert.IsInstanceOfType (typeof (IBodyClause), fromClause);
    }

    [Test]
    public void GetQueriedEntityType ()
    {
      IQueryable<Student> querySource = ExpressionHelper.CreateQuerySource();
      LambdaExpression fromExpression = Expression.Lambda (Expression.Constant (querySource), Expression.Parameter (typeof (Student), "student"));
      var fromClause =
          new AdditionalFromClause (ExpressionHelper.CreateClause(), ExpressionHelper.CreateParameterExpression(), 
          fromExpression, ExpressionHelper.CreateLambdaExpression());
      Assert.AreSame (typeof (TestQueryable<Student>), fromClause.GetQuerySourceType());
    }

    [Test]
    public void Accept ()
    {
      AdditionalFromClause fromClause = ExpressionHelper.CreateAdditionalFromClause ();
      var visitorMock = MockRepository.GenerateMock<IQueryVisitor> ();
      fromClause.Accept (visitorMock);
      visitorMock.AssertWasCalled (mock => mock.VisitAdditionalFromClause (fromClause));
    }

    [Test]
    public void QueryModelAtInitialization ()
    {
      AdditionalFromClause additionalFromClause = ExpressionHelper.CreateAdditionalFromClause ();
      Assert.IsNull (additionalFromClause.QueryModel);
    }

    [Test]
    public void SetQueryModel ()
    {
      AdditionalFromClause additionalFromClause = ExpressionHelper.CreateAdditionalFromClause ();
      QueryModel model = ExpressionHelper.CreateQueryModel ();
      additionalFromClause.SetQueryModel (model);
      Assert.IsNotNull (additionalFromClause.QueryModel);
    }

    [Test]
    [ExpectedException (typeof (ArgumentNullException))]
    public void SetQueryModelWithNull_Exception ()
    {
      AdditionalFromClause additionalFromClause = ExpressionHelper.CreateAdditionalFromClause ();
      additionalFromClause.SetQueryModel (null);
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "QueryModel is already set")]
    public void SetQueryModelTwice_Exception ()
    {
      AdditionalFromClause additionalFromClause = ExpressionHelper.CreateAdditionalFromClause ();     
      QueryModel model = ExpressionHelper.CreateQueryModel ();
      additionalFromClause.SetQueryModel (model);
      additionalFromClause.SetQueryModel (model);
    }

    [Test]
    public void Clone ()
    {
      var originalClause = ExpressionHelper.CreateAdditionalFromClause ();
      var newPreviousClause = ExpressionHelper.CreateMainFromClause ();
      var clone = originalClause.Clone (newPreviousClause);

      Assert.That (clone, Is.Not.Null);
      Assert.That (clone, Is.Not.SameAs (originalClause));
      Assert.That (clone.Identifier, Is.SameAs (originalClause.Identifier));
      Assert.That (clone.FromExpression, Is.SameAs (originalClause.FromExpression));
      Assert.That (clone.ProjectionExpression, Is.SameAs (originalClause.ProjectionExpression));
      Assert.That (clone.PreviousClause, Is.SameAs (newPreviousClause));
      Assert.That (clone.QueryModel, Is.Null);
    }

    [Test]
    public void Clone_JoinClauses ()
    {
      AdditionalFromClause originalClause = ExpressionHelper.CreateAdditionalFromClause ();
      var originalJoinClause1 = ExpressionHelper.CreateJoinClause ();
      originalClause.AddJoinClause (originalJoinClause1);

      var originalJoinClause2 = ExpressionHelper.CreateJoinClause ();
      originalClause.AddJoinClause (originalJoinClause2);

      var newPreviousClause = ExpressionHelper.CreateClause();
      var clone = originalClause.Clone (newPreviousClause);
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
  }
}

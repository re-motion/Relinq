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
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq;
using Rhino.Mocks;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.UnitTests.Linq.ParsingTest;
using Remotion.Data.UnitTests.Linq.TestQueryGenerators;

namespace Remotion.Data.UnitTests.Linq.ClausesTest
{
  [TestFixture]
  public class WhereClauseTest
  {
    [Test] 
    public void InitializeWithboolExpression()
    {
      LambdaExpression boolExpression = ExpressionHelper.CreateLambdaExpression ();
      IClause clause = ExpressionHelper.CreateClause();
      
      WhereClause whereClause = new WhereClause(clause,boolExpression);
      Assert.AreSame (clause, whereClause.PreviousClause);
      Assert.AreSame (boolExpression, whereClause.BoolExpression);
    }

    [Test]
    public void ImplementInterface()
    {
      WhereClause whereClause = ExpressionHelper.CreateWhereClause();
      Assert.IsInstanceOfType (typeof (IBodyClause), whereClause);
    }
    
    [Test]
    public void WhereClause_ImplementIQueryElement()
    {
      WhereClause whereClause = ExpressionHelper.CreateWhereClause();
      Assert.IsInstanceOfType (typeof (IQueryElement), whereClause);
    }

    [Test]
    public void GetSimplifiedBoolExpression ()
    {
      MethodCallExpression whereMethodCallExpression = WhereTestQueryGenerator.CreateWhereQueryWithEvaluatableSubExpression_WhereExpression (
          ExpressionHelper.CreateQuerySource());
      LambdaExpression boolExpression = (LambdaExpression) ((UnaryExpression) whereMethodCallExpression.Arguments[1]).Operand;
      IClause clause = ExpressionHelper.CreateClause();
      WhereClause whereClause = new WhereClause (clause, boolExpression);
      LambdaExpression simplifiedExpression = whereClause.GetSimplifiedBoolExpression ();
      LambdaExpression simplifiedExpression2 = whereClause.GetSimplifiedBoolExpression ();
      Assert.AreSame (simplifiedExpression2, simplifiedExpression);

      ParameterExpression parameter = Expression.Parameter (typeof (Student), "s");
      Expression expected = Expression.Lambda (Expression.Equal (Expression.MakeMemberAccess (parameter, typeof (Student).GetProperty ("Last")),
          Expression.Constant ("Garcia")), parameter);

      ExpressionTreeComparer.CheckAreEqualTrees (simplifiedExpression, expected);
    }

    [Test]
    public void Accept()
    {
      WhereClause whereClause = ExpressionHelper.CreateWhereClause();

      MockRepository repository = new MockRepository ();
      IQueryVisitor visitorMock = repository.StrictMock<IQueryVisitor> ();

      visitorMock.VisitWhereClause (whereClause);

      repository.ReplayAll();

      whereClause.Accept (visitorMock);

      repository.VerifyAll();
    }

    [Test]
    public void QueryModelAtInitialization ()
    {
      WhereClause whereClause = ExpressionHelper.CreateWhereClause ();
      Assert.IsNull (whereClause.QueryModel);
    }

    [Test]
    public void SetQueryModel ()
    {
      WhereClause whereClause = ExpressionHelper.CreateWhereClause ();
      QueryModel model = ExpressionHelper.CreateQueryModel ();
      whereClause.SetQueryModel (model);
      Assert.IsNotNull (whereClause.QueryModel);
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
      Assert.That (clone.BoolExpression, Is.SameAs (originalClause.BoolExpression));
      Assert.That (clone.PreviousClause, Is.SameAs (newPreviousClause));
      Assert.That (clone.QueryModel, Is.Null);
    }
  }
}

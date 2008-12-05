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
using Remotion.Data.Linq;
using Rhino.Mocks;
using Remotion.Data.Linq.Clauses;

namespace Remotion.Data.UnitTests.Linq.ClausesTest
{
  [TestFixture]
  public class JoinClauseTest
  {
    [Test]
    public void Intialize_WithIDAndInExprAndOnExprAndEqExpr()
    {
      ParameterExpression identifier = ExpressionHelper.CreateParameterExpression ();
      Expression inExpression = ExpressionHelper.CreateExpression ();
      Expression onExpression = ExpressionHelper.CreateExpression ();
      Expression equalityExpression = ExpressionHelper.CreateExpression ();

      IClause clause = ExpressionHelper.CreateClause();

      JoinClause joinClause = new JoinClause (clause, identifier, inExpression, onExpression, equalityExpression);

      Assert.AreSame (clause, joinClause.PreviousClause);
      Assert.AreSame (identifier, joinClause.Identifier);
      Assert.AreSame (inExpression, joinClause.InExpression);
      Assert.AreSame (onExpression, joinClause.OnExpression);
      Assert.AreSame (equalityExpression, joinClause.EqualityExpression);
      
      Assert.IsNull (joinClause.IntoIdentifier);
    }

    [Test]
    public void Intialize_WithIDAndInExprAndOnExprAndEqExprAndIntoID ()
    {
      ParameterExpression identifier = ExpressionHelper.CreateParameterExpression ();
      Expression inExpression = ExpressionHelper.CreateExpression ();
      Expression onExpression = ExpressionHelper.CreateExpression ();
      Expression equalityExpression = ExpressionHelper.CreateExpression ();
      ParameterExpression intoIdentifier = ExpressionHelper.CreateParameterExpression ();

      IClause clause = ExpressionHelper.CreateClause ();
      JoinClause joinClause = new JoinClause (clause,identifier, inExpression, onExpression, equalityExpression, intoIdentifier);

      Assert.AreSame (clause, joinClause.PreviousClause);
      Assert.AreSame (identifier, joinClause.Identifier);
      Assert.AreSame (inExpression, joinClause.InExpression);
      Assert.AreSame (onExpression, joinClause.OnExpression);
      Assert.AreSame (equalityExpression, joinClause.EqualityExpression);
      Assert.AreSame (equalityExpression, joinClause.EqualityExpression);
      Assert.AreSame (intoIdentifier, joinClause.IntoIdentifier);

    }

    [Test]
    public void JoinClause_ImplementsIQueryElement()
    {
      JoinClause joinClause = ExpressionHelper.CreateJoinClause();
      Assert.IsInstanceOfType (typeof (IQueryElement), joinClause);
    }

    [Test]
    public void Accept ()
    {
      JoinClause joinClause = ExpressionHelper.CreateJoinClause ();

      MockRepository repository = new MockRepository ();
      IQueryVisitor visitorMock = repository.StrictMock<IQueryVisitor> ();

      visitorMock.VisitJoinClause (joinClause);

      repository.ReplayAll ();

      joinClause.Accept (visitorMock);

      repository.VerifyAll ();

    }
  }
}

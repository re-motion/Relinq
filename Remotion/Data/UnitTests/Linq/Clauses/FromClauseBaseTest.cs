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
using Remotion.Data.Linq.Clauses;
using Rhino.Mocks;

namespace Remotion.Data.UnitTests.Linq.Clauses
{
  [TestFixture]
  public class FromClauseBaseTest
  {
    [Test]
    public void AddJoinClause()
    {
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause();

      JoinClause joinClause1 = ExpressionHelper.CreateJoinClause();
      JoinClause joinClause2 = ExpressionHelper.CreateJoinClause();

      fromClause.JoinClauses.Add (joinClause1);
      fromClause.JoinClauses.Add (joinClause2);

      Assert.That (fromClause.JoinClauses, Is.EqualTo (new object[] { joinClause1, joinClause2 }));
      Assert.AreEqual (2, fromClause.JoinClauses.Count);
    }

    [Test]
    [ExpectedException (typeof (ArgumentNullException))]
    public void AddJoinClause_Null_ThrowsArgumentNullException ()
    {
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause ();
      fromClause.JoinClauses.Add (null);
    }

    [Test]
    [ExpectedException (typeof (ArgumentNullException))]
    public void ChangeJoinClause_WithNull_ThrowsArgumentNullException ()
    {
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause ();
      JoinClause joinClause = ExpressionHelper.CreateJoinClause ();
      fromClause.JoinClauses.Add (joinClause);
      fromClause.JoinClauses[0] = null;
    }

    [Test]
    public void TransformExpressions ()
    {
      var oldExpression = ExpressionHelper.CreateExpression ();
      var newExpression = ExpressionHelper.CreateExpression ();
      var clause = new MainFromClause ("x", typeof (Student), oldExpression);

      clause.TransformExpressions (ex =>
      {
        Assert.That (ex, Is.SameAs (oldExpression));
        return newExpression;
      });

      Assert.That (clause.FromExpression, Is.SameAs (newExpression));
    }

    [Test]
    public void TransformExpressions_PassedToJoinClauses ()
    {
      Func<Expression, Expression> transformer = ex => ex;
      var expression = ExpressionHelper.CreateExpression ();
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause ();
      var joinClauseMock = MockRepository.GenerateMock<JoinClause> ("item", typeof(string), expression, expression, expression);
      fromClause.JoinClauses.Add (joinClauseMock);

      joinClauseMock.Expect (mock => mock.TransformExpressions (transformer));

      joinClauseMock.Replay ();

      fromClause.TransformExpressions (transformer);

      joinClauseMock.VerifyAllExpectations ();
    }

    [Test]
    public new void ToString ()
    {
      var fromClause = new MainFromClause ("x", typeof (Student), Expression.Constant (0));
      Assert.That (fromClause.ToString (), Is.EqualTo ("from Student x in 0"));
    }

    [Test]
    public void ToString_WithJoins ()
    {
      var fromClause = new MainFromClause ("x", typeof (Student), Expression.Constant (0));
      fromClause.JoinClauses.Add (new JoinClause ("y", typeof (Student), Expression.Constant (1), Expression.Constant (2), Expression.Constant (3)));
      fromClause.JoinClauses.Add (new JoinClause ("z", typeof (Student), Expression.Constant (4), Expression.Constant (5), Expression.Constant (6)));

      Assert.That (fromClause.ToString (), Is.EqualTo ("from Student x in 0 join Student y in 1 on 2 equals 3 join Student z in 4 on 5 equals 6"));
    }
  }
}

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
using Remotion.Data.Linq.DataObjectModel;

namespace Remotion.Data.UnitTests.Linq.ClausesTest
{
  [TestFixture]
  public class MainFromClauseTest
  {
    [Test]
    public void Initialize_WithIDAndConstant ()
    {
      ParameterExpression id = ExpressionHelper.CreateParameterExpression ();
      IQueryable querySource = ExpressionHelper.CreateQuerySource ();

      ConstantExpression constantExpression = Expression.Constant (querySource);
      MainFromClause fromClause = new MainFromClause (id, constantExpression);

      Assert.AreSame (id, fromClause.Identifier);
      Assert.AreSame (constantExpression, fromClause.QuerySource);

      Assert.That (fromClause.JoinClauses, Is.Empty);
      Assert.AreEqual (0, fromClause.JoinClauses.Count);

      Assert.IsNull (fromClause.PreviousClause);
    }

    [Test]
    public void Initialize_WithExpression ()
    {
      ParameterExpression id = ExpressionHelper.CreateParameterExpression ();
      IQueryable querySource = ExpressionHelper.CreateQuerySource ();
      var anonymous = new {source = querySource};
      MemberExpression sourceExpression = Expression.MakeMemberAccess (Expression.Constant (anonymous), anonymous.GetType().GetProperty ("source"));

      MainFromClause fromClause = new MainFromClause (id, sourceExpression);

      Assert.AreSame (sourceExpression, fromClause.QuerySource);
    }

    [Test]
    public void Accept ()
    {
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause ();

      MockRepository repository = new MockRepository ();
      IQueryVisitor visitorMock = repository.StrictMock<IQueryVisitor> ();

      visitorMock.VisitMainFromClause (fromClause);

      repository.ReplayAll ();

      fromClause.Accept (visitorMock);

      repository.VerifyAll ();

    }

    [Test]
    public void GetQueriedEntityType ()
    {
      IQueryable<Student> querySource = ExpressionHelper.CreateQuerySource();
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause(ExpressionHelper.CreateParameterExpression(), querySource);
      Assert.AreSame (typeof (TestQueryable<Student>), fromClause.GetQuerySourceType());
    }

    [Test]
    public void Clone ()
    {
      MainFromClause originalClause = ExpressionHelper.CreateMainFromClause ();
      var clone = originalClause.Clone ();

      Assert.That (clone, Is.Not.Null);
      Assert.That (clone, Is.Not.SameAs (originalClause));
      Assert.That (clone.Identifier, Is.SameAs (originalClause.Identifier));
      Assert.That (clone.QuerySource, Is.SameAs (originalClause.QuerySource));
    }

    [Test]
    public void Clone_JoinClauses ()
    {
      MainFromClause originalClause = ExpressionHelper.CreateMainFromClause ();
      var originalJoinClause1 = ExpressionHelper.CreateJoinClause ();
      originalClause.Add (originalJoinClause1);

      var originalJoinClause2 = ExpressionHelper.CreateJoinClause ();
      originalClause.Add (originalJoinClause2);

      var clone = originalClause.Clone ();
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

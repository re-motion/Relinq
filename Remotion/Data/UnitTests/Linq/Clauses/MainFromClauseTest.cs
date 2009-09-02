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
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.UnitTests.Linq.TestDomain;
using Rhino.Mocks;

namespace Remotion.Data.UnitTests.Linq.Clauses
{
  [TestFixture]
  public class MainFromClauseTest
  {
    private MainFromClause _mainFromClause;
    private CloneContext _cloneContext;

    [SetUp]
    public void SetUp ()
    {
      _mainFromClause = ExpressionHelper.CreateMainFromClause();
      _cloneContext = new CloneContext (new QuerySourceMapping());
    }

    [Test]
    public void Initialize ()
    {
      IQueryable querySource = ExpressionHelper.CreateStudentQueryable ();

      ConstantExpression constantExpression = Expression.Constant (querySource);
      var fromClause = new MainFromClause ("s", typeof (Student), constantExpression);

      Assert.That (fromClause.ItemName, Is.EqualTo ("s"));
      Assert.That (fromClause.ItemType, Is.SameAs (typeof (Student)));
      Assert.That (fromClause.FromExpression, Is.SameAs (constantExpression));
    }

    [Test]
    public void Initialize_WithNonConstantExpression ()
    {
      IQueryable querySource = ExpressionHelper.CreateStudentQueryable ();
      var anonymous = new {source = querySource};
      MemberExpression sourceExpression = Expression.MakeMemberAccess (Expression.Constant (anonymous), anonymous.GetType().GetProperty ("source"));

      var fromClause = new MainFromClause ("s", typeof (Student), sourceExpression);
      Assert.That (fromClause.FromExpression, Is.SameAs (sourceExpression));
    }

    [Test]
    public void Accept ()
    {
      var repository = new MockRepository ();
      var visitorMock = repository.StrictMock<IQueryModelVisitor> ();
      var queryModel = ExpressionHelper.CreateQueryModel_Student ();

      visitorMock.VisitMainFromClause (_mainFromClause, queryModel);

      repository.ReplayAll ();

      _mainFromClause.Accept (visitorMock, queryModel);

      repository.VerifyAll ();
    }

    [Test]
    public void Clone ()
    {
      var clone = _mainFromClause.Clone (_cloneContext);

      Assert.That (clone, Is.Not.Null);
      Assert.That (clone, Is.Not.SameAs (_mainFromClause));
      Assert.That (clone.ItemName, Is.EqualTo (_mainFromClause.ItemName));
      Assert.That (clone.ItemType, Is.SameAs (_mainFromClause.ItemType));
      Assert.That (clone.FromExpression, Is.SameAs (_mainFromClause.FromExpression));
    }

    [Test]
    public void Clone_AddsClauseToMapping ()
    {
      var clone = _mainFromClause.Clone (_cloneContext);
      Assert.That (((QuerySourceReferenceExpression) _cloneContext.QuerySourceMapping.GetExpression (_mainFromClause)).ReferencedQuerySource, Is.SameAs (clone));
    }
  }
}

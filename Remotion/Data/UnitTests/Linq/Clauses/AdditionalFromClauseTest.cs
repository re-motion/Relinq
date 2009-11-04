// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// as published by the Free Software Foundation; either version 2.1 of the 
// License, or (at your option) any later version.
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
  public class AdditionalFromClauseTest
  {
    private AdditionalFromClause _additionalFromClause;
    private CloneContext _cloneContext;

    [SetUp]
    public void SetUp ()
    {
      _additionalFromClause = ExpressionHelper.CreateAdditionalFromClause();
      _cloneContext = new CloneContext (new QuerySourceMapping ());      
    }

    [Test]
    public void Initialize ()
    {
      var fromExpression = ExpressionHelper.CreateExpression ();
      var fromClause = new AdditionalFromClause ("s", typeof (Student), fromExpression);

      Assert.That (fromClause.ItemName, Is.EqualTo ("s"));
      Assert.That (fromClause.ItemType, Is.SameAs (typeof (Student)));
      Assert.That (fromClause.FromExpression, Is.SameAs (fromExpression));
    }

    [Test]
    public void ImplementInterface_IFromLetWhereClause ()
    {
      Assert.That (_additionalFromClause, Is.InstanceOfType (typeof (IBodyClause)));
    }

    [Test]
    public void Accept ()
    {
      var queryModel = ExpressionHelper.CreateQueryModel_Student ();
      var visitorMock = MockRepository.GenerateMock<IQueryModelVisitor> ();
      _additionalFromClause.Accept (visitorMock, queryModel, 1);
      visitorMock.AssertWasCalled (mock => mock.VisitAdditionalFromClause (_additionalFromClause, queryModel, 1));
    }
    
    [Test]
    public void Clone ()
    {
      var clone = _additionalFromClause.Clone (_cloneContext);

      Assert.That (clone, Is.Not.Null);
      Assert.That (clone, Is.Not.SameAs (_additionalFromClause));
      Assert.That (clone.ItemName, Is.EqualTo (_additionalFromClause.ItemName));
      Assert.That (clone.ItemType, Is.SameAs (_additionalFromClause.ItemType));
      Assert.That (clone.FromExpression, Is.SameAs (_additionalFromClause.FromExpression));
    }

    [Test]
    public void Clone_AddsClauseToMapping ()
    {
      var clone = _additionalFromClause.Clone (_cloneContext);
      Assert.That (((QuerySourceReferenceExpression) _cloneContext.QuerySourceMapping.GetExpression (_additionalFromClause)).ReferencedQuerySource,
          Is.SameAs (clone));
    }
  }
}

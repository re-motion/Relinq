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
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.DataObjectModel;
using Rhino.Mocks;

namespace Remotion.Data.UnitTests.Linq.ClausesTest
{
  [TestFixture]
  public class MemberFromClauseTest
  {
    [Test]
    public void FromExpressionContainsMemberExpression ()
    {
      MemberFromClause fromClause = ExpressionHelper.CreateMemberFromClause();
      Assert.That (fromClause.MemberExpression, Is.SameAs (fromClause.FromExpression.Body));
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = "From expression must contain a MemberExpression.")]
    public void FromExpressionContainNoMemberExpression ()
    {
      var previousClause = ExpressionHelper.CreateClause ();
      var identifier = ExpressionHelper.CreateParameterExpression ();
      var bodyExpression = Expression.Constant ("test");
      var fromExpression = Expression.Lambda (bodyExpression);
      var projectionExpression = ExpressionHelper.CreateLambdaExpression ();
      new MemberFromClause (previousClause, identifier, fromExpression, projectionExpression);
    }

    [Test]
    public void Accept ()
    {
      MemberFromClause fromClause = ExpressionHelper.CreateMemberFromClause ();
      var visitorMock = MockRepository.GenerateMock<IQueryVisitor> ();
      fromClause.Accept (visitorMock);
      visitorMock.AssertWasCalled (mock => mock.VisitMemberFromClause (fromClause));
    }

    [Test]
    public void GetFromSource ()
    {
      MemberFromClause fromClause = ExpressionHelper.CreateMemberFromClause ();
      var columnSource = fromClause.GetFromSource (StubDatabaseInfo.Instance);

      Assert.That (columnSource is Table);
      var tableSource = (Table) columnSource;
      Assert.That (tableSource.Name, Is.EqualTo ("studentTable"));
      Assert.That (tableSource.Alias, Is.EqualTo (fromClause.Identifier.Name));
    }
  }
}

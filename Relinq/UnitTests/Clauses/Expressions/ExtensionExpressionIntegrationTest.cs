// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (c) rubicon IT GmbH, www.rubicon.eu
// 
// re-linq is free software; you can redistribute it and/or modify it under 
// the terms of the GNU Lesser General Public License as published by the 
// Free Software Foundation; either version 2.1 of the License, 
// or (at your option) any later version.
// 
// re-linq is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-linq; if not, see http://www.gnu.org/licenses.
// 
using System;
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.UnitTests.Linq.Core.Clauses.Expressions.TestDomain;
using Rhino.Mocks;

namespace Remotion.Linq.UnitTests.Linq.Core.Clauses.Expressions
{
  [TestFixture]
  public class ExtensionExpressionIntegrationTest
  {
    [Test]
    public void ExpressionOverridingAcceptMethod ()
    {
      var visitorMock = MockRepository.GenerateMock<SpecificVisitor> ();
      
      var expression = new TestableExtensionExpressionWithSpecificVisitor (typeof (int));
      expression.Accept (visitorMock);

      visitorMock.AssertWasCalled (mock => mock.VisitTestableExtensionExpression (expression));
    }

    [Test]
    public void CanReduce_True ()
    {
      var expression = new ReducibleExtensionExpression (typeof (int));
      Assert.That (expression.CanReduce, Is.True);
    }

    [Test]
    public void Reduce ()
    {
      var expression = new ReducibleExtensionExpression (typeof (int));

      var result = expression.Reduce ();

      var expectedExpression = Expression.Constant (0);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }
  }
}
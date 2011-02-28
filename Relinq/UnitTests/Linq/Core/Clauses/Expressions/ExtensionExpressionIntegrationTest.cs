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
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Linq.UnitTests.Linq.Core.Clauses.Expressions.TestDomain;
using Remotion.Linq.UnitTests.Linq.Core.Parsing;
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
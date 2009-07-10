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
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq;
using Remotion.Data.Linq.Clauses.ExecutionStrategies;
using Remotion.Data.Linq.Parsing.Structure;
using Remotion.Data.UnitTests.Linq.Clauses.ResultOperators;
using Remotion.Utilities;
using Rhino.Mocks;

namespace Remotion.Data.UnitTests.Linq.Clauses
{
  [TestFixture]
  public class ResultOperatorBaseTest
  {
    TestResultOperator _resultOperator;

    [SetUp]
    public void SetUp ()
    {
      _resultOperator = new TestResultOperator (CollectionExecutionStrategy.Instance);
    }

    [Test]
    public void Accept ()
    {
      var queryModel = ExpressionHelper.CreateQueryModel ();
      var visitorMock = MockRepository.GenerateMock<IQueryModelVisitor> ();
      _resultOperator.Accept (visitorMock, queryModel, 1);

      visitorMock.AssertWasCalled (mock => mock.VisitResultOperator (_resultOperator, queryModel, 1));
    }

    [Test]
    public void InvokeGenericOnEnumerable ()
    {
      var input = new[] { 1, 2, 3, 4, 3, 2, 1 };
      var result = _resultOperator.InvokeGenericOnEnumerable (input, _resultOperator.DistinctExecuteMethod);

      Assert.That (((IEnumerable<int>) result).ToArray(), Is.EquivalentTo (new[] { 1, 2, 3, 4 }));
    }

    [Test]
    [ExpectedException (typeof (NotImplementedException), ExpectedMessage = "Test")]
    public void InvokeGenericOnEnumerable_Throws ()
    {
      var input = new[] { 1, 2, 3, 4, 3, 2, 1 };
      var result = _resultOperator.InvokeGenericOnEnumerable (input, _resultOperator.ThrowingExecuteMethod);

      Assert.That (((IEnumerable<int>) result).ToArray (), Is.EquivalentTo (new[] { 1, 2, 3, 4 }));
    }

    [Test]
    [ExpectedException (typeof (ArgumentTypeException), ExpectedMessage = "Argument input has type System.Int32 when type "
        + "System.Collections.Generic.IEnumerable`1[T] was expected.\r\nParameter name: input")]
    public void InvokeGenericOnEnumerable_InvalidInputType ()
    {
      _resultOperator.InvokeGenericOnEnumerable (14, e => e.Count ());
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = "Method to invoke ('NonGenericExecuteMethod') must be a public generic method "
        + "with exactly one generic argument.\r\nParameter name: genericMethodCaller")]
    public void InvokeGenericOnEnumerable_NonGenericMethod ()
    {
      _resultOperator.InvokeGenericOnEnumerable (new[] { 1, 2, 3 }, _resultOperator.NonGenericExecuteMethod);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = "Method to invoke ('NonPublicExecuteMethod') must be a public generic method "
        + "with exactly one generic argument.\r\nParameter name: genericMethodCaller")]
    public void InvokeGenericOnEnumerable_NonPublicTestMethod ()
    {
      _resultOperator.InvokeGenericOnEnumerable (new[] { 1, 2, 3 }, _resultOperator.NonPublicExecuteMethod);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = "Cannot call method 'ExecuteMethodWithNonMatchingArgumentType' on input of type "
        + "'System.Int32[]': Object of type 'System.Int32[]' cannot be converted to type 'System.Collections.Generic.IEnumerable`1[System.Object]'."
        + "\r\nParameter name: method")]
    public void InvokeGenericOnEnumerable_NonMatchingArgument ()
    {
      _resultOperator.InvokeGenericOnEnumerable (new[] { 1, 2, 3 }, _resultOperator.ExecuteMethodWithNonMatchingArgumentType<object>);
    }
  }
}
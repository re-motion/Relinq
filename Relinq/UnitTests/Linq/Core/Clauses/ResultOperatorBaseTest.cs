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
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Linq.UnitTests.Linq.Core.Clauses.ResultOperators;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.Utilities;
using Rhino.Mocks;

namespace Remotion.Linq.UnitTests.Linq.Core.Clauses
{
  [TestFixture]
  public class ResultOperatorBaseTest
  {
    private TestResultOperator _resultOperator;
    private StreamedSequence _executeInMemoryInput;

    [SetUp]
    public void SetUp ()
    {
      _resultOperator = new TestResultOperator ();
      _executeInMemoryInput = new StreamedSequence (new[] { 1, 2, 3, 4, 3, 2, 1 }, new StreamedSequenceInfo (typeof (int[]), Expression.Constant (0)));
    }

    [Test]
    public void Accept ()
    {
      var queryModel = ExpressionHelper.CreateQueryModel_Cook ();
      var visitorMock = MockRepository.GenerateMock<IQueryModelVisitor> ();
      _resultOperator.Accept (visitorMock, queryModel, 1);

      visitorMock.AssertWasCalled (mock => mock.VisitResultOperator (_resultOperator, queryModel, 1));
    }

    [Test]
    public void InvokeGenericExecuteMethod ()
    {
      var result = _resultOperator.InvokeGenericExecuteMethod<StreamedSequence, StreamedSequence> (
          _executeInMemoryInput, 
          _resultOperator.DistinctExecuteMethod<object>);

      Assert.That (result.GetTypedSequence<int>().ToArray(), Is.EquivalentTo (new[] { 1, 2, 3, 4 }));
    }

    [Test]
    [ExpectedException (typeof (NotImplementedException), ExpectedMessage = "Test")]
    public void InvokeGenericExecuteMethod_Throws ()
    {
      _resultOperator.InvokeGenericExecuteMethod<StreamedSequence, StreamedSequence> (
          _executeInMemoryInput, 
          _resultOperator.ThrowingExecuteMethod<object>);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = "Method to invoke ('NonGenericExecuteMethod') must be a generic method "
        + "with exactly one generic argument.\r\nParameter name: genericExecuteCaller")]
    public void InvokeGenericExecuteMethod_NonGenericMethod ()
    {
      _resultOperator.InvokeGenericExecuteMethod<StreamedSequence, StreamedSequence> (
          _executeInMemoryInput,
          _resultOperator.NonGenericExecuteMethod);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = "Method to invoke ('InvalidExecuteInMemory_TooManyGenericParameters') must be a generic method "
        + "with exactly one generic argument.\r\nParameter name: genericExecuteCaller")]
    public void InvokeGenericExecuteMethod_WrongNumberOfGenericArguments ()
    {
      _resultOperator.InvokeGenericExecuteMethod<StreamedSequence, StreamedValue> (
          _executeInMemoryInput,
          _resultOperator.InvalidExecuteInMemory_TooManyGenericParameters<object, object>);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = "Method to invoke ('NonPublicExecuteMethod') must be a public method.\r\n"
        + "Parameter name: method")]
    public void InvokeGenericExecuteMethod_NonPublicMethod ()
    {
      _resultOperator.InvokeGenericExecuteMethod<StreamedSequence, StreamedSequence> (
          _executeInMemoryInput,
          _resultOperator.NonPublicExecuteMethod<object>);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = "Cannot call method 'ExecuteMethodWithNonMatchingArgumentType' on input of type "
        + "'Remotion.Linq.Clauses.StreamedData.StreamedSequence': Object of type 'Remotion.Linq.Clauses.StreamedData."
        + "StreamedSequence' cannot be converted to type 'Remotion.Linq.Clauses.StreamedData.StreamedValue'."
        + "\r\nParameter name: method")]
    public void InvokeGenericExecuteMethod_NonMatchingArgument ()
    {
      _resultOperator.InvokeGenericExecuteMethod<StreamedValue, StreamedValue> (
          _executeInMemoryInput,
          _resultOperator.ExecuteMethodWithNonMatchingArgumentType<object>);
    }

    [Test]
    public void CheckSequenceItemType_SameType ()
    {
      var sequenceInfo = new StreamedSequenceInfo (typeof (object[]), Expression.Constant ("test"));
      
      _resultOperator.CheckSequenceItemType (sequenceInfo, typeof (object));
    }

    [Test]
    [ExpectedException (typeof (ArgumentTypeException), ExpectedMessage = 
        "The input sequence must have items of type 'System.String', but it has items of type 'System.Object'.\r\nParameter name: inputInfo")]
    public void CheckSequenceItemType_WrongType ()
    {
      var sequenceInfo = new StreamedSequenceInfo (typeof (object[]), Expression.Constant ("test"));

      _resultOperator.CheckSequenceItemType (sequenceInfo, typeof (string));
    }
  }
}

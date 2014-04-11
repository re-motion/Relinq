// Copyright (c) rubicon IT GmbH, www.rubicon.eu
//
// See the NOTICE file distributed with this work for additional information
// regarding copyright ownership.  rubicon licenses this file to you under 
// the Apache License, Version 2.0 (the "License"); you may not use this 
// file except in compliance with the License.  You may obtain a copy of the 
// License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, WITHOUT 
// WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.  See the 
// License for the specific language governing permissions and limitations
// under the License.
// 
using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.UnitTests.Clauses.ResultOperators;
using Remotion.Linq.UnitTests.TestDomain;
using Rhino.Mocks;

namespace Remotion.Linq.UnitTests.Clauses
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
      var queryModel = ExpressionHelper.CreateQueryModel<Cook> ();
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
      var sequenceInfo = new StreamedSequenceInfo (typeof (int[]), Expression.Constant (0));
      var expectedItemType = typeof (int);

      Assert.That (() => _resultOperator.CheckSequenceItemType (sequenceInfo, expectedItemType), Throws.Nothing);
    }

    [Test]
    public void CheckSequenceItemType_ExpectedAssignableFromSequenceItem ()
    {
      var sequenceInfo = new StreamedSequenceInfo (typeof (string[]), Expression.Constant ("t"));
      var expectedItemType = typeof (object);

      Assert.That (() => _resultOperator.CheckSequenceItemType (sequenceInfo, expectedItemType), Throws.Nothing);
    }

    [Test]
    public void CheckSequenceItemType_SequenceItemAssignableFromExpected ()
    {
      var sequenceInfo = new StreamedSequenceInfo (typeof (object[]), Expression.Constant (null, typeof (object)));
      var expectedItemType = typeof (string);

      Assert.That (
          () => _resultOperator.CheckSequenceItemType (sequenceInfo, expectedItemType), 
          Throws.ArgumentException.With.Message.EqualTo (
              "The input sequence must have items of type 'System.String', but it has items of type 'System.Object'.\r\nParameter name: inputInfo"));
    }

    [Test]
    public void CheckSequenceItemType_DifferentTypes ()
    {
      var sequenceInfo = new StreamedSequenceInfo (typeof (string[]), Expression.Constant ("t"));
      var expectedItemType = typeof (int);

      Assert.That (
          () => _resultOperator.CheckSequenceItemType (sequenceInfo, expectedItemType),
          Throws.ArgumentException.With.Message.EqualTo (
              "The input sequence must have items of type 'System.Int32', but it has items of type 'System.String'.\r\nParameter name: inputInfo"));
    }
  }
}

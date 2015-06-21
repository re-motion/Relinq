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
using Remotion.Linq.Utilities;
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
    public void InvokeExecuteMethod ()
    {
      var result = (StreamedSequence) _resultOperator.InvokeExecuteMethod (
          ReflectionUtility.GetMethod (() => ((TestResultOperator) null).DistinctExecuteMethod<int> (null)),
          _executeInMemoryInput);

      Assert.That (result.GetTypedSequence<int>().ToArray(), Is.EquivalentTo (new[] { 1, 2, 3, 4 }));
    }

    [Test]
    public void InvokeGenericExecuteMethod_Throws ()
    {
      var methodInfo =
          ReflectionUtility.GetMethod (() => ((TestResultOperator) null).ThrowingExecuteMethod<object> (null));
      Assert.That (
          () => _resultOperator.InvokeExecuteMethod (methodInfo, _executeInMemoryInput),
          Throws.Exception.TypeOf<NotImplementedException>().With.Message.EqualTo ("Test"));
    }

    [Test]
    public void InvokeGenericExecuteMethod_NonPublicMethod ()
    {
      var methodInfo =
          ReflectionUtility.GetMethod (() => ((TestResultOperator) null).NonPublicExecuteMethod<object> (null));
      Assert.That (
          () => _resultOperator.InvokeExecuteMethod (methodInfo, _executeInMemoryInput),
          Throws.ArgumentException.With.Message.EqualTo (
              "Method to invoke ('NonPublicExecuteMethod') must be a public method.\r\nParameter name: method"));
    }

    [Test]
    public void InvokeGenericExecuteMethod_NonMatchingArgument ()
    {
      var methodInfo =
          ReflectionUtility.GetMethod (() => ((TestResultOperator) null).ExecuteMethodWithNonMatchingArgumentType<object> (null));
      Assert.That (
          () => _resultOperator.InvokeExecuteMethod (methodInfo, _executeInMemoryInput),
          Throws.ArgumentException.With.Message.EqualTo (
              "Cannot call method 'ExecuteMethodWithNonMatchingArgumentType' on input of type "
        + "'Remotion.Linq.Clauses.StreamedData.StreamedSequence': Object of type 'Remotion.Linq.Clauses.StreamedData."
        + "StreamedSequence' cannot be converted to type 'Remotion.Linq.Clauses.StreamedData.StreamedValue'."
        + "\r\nParameter name: method"));
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

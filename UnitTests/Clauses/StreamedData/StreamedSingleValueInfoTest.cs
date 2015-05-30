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
using NUnit.Framework;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.UnitTests.Clauses.ResultOperators;
using Remotion.Linq.UnitTests.TestDomain;
using Rhino.Mocks;

namespace Remotion.Linq.UnitTests.Clauses.StreamedData
{
  [TestFixture]
  public class StreamedSingleValueInfoTest
  {
    private StreamedSingleValueInfo _streamedSingleValueInfoWithDefault;
    private StreamedSingleValueInfo _streamedSingleValueInfoNoDefault;

    [SetUp]
    public void SetUp ()
    {
      _streamedSingleValueInfoWithDefault = new StreamedSingleValueInfo (typeof (Cook), true);
      _streamedSingleValueInfoNoDefault = new StreamedSingleValueInfo (typeof (Cook), false);
    }

    [Test]
    public void DataType ()
    {
      Assert.That (_streamedSingleValueInfoNoDefault.DataType, Is.SameAs (typeof (Cook)));
    }

    [Test]
    public void AdjustDataType_CompatibleType ()
    {
      var result = _streamedSingleValueInfoWithDefault.AdjustDataType (typeof (object));

      Assert.That (result, Is.Not.SameAs (_streamedSingleValueInfoWithDefault));
      Assert.That (result, Is.TypeOf (typeof (StreamedSingleValueInfo)));
      Assert.That (result.DataType, Is.SameAs (typeof (object)));
      Assert.That (((StreamedSingleValueInfo) result).ReturnDefaultWhenEmpty, Is.True);

      Assert.That (((StreamedSingleValueInfo) _streamedSingleValueInfoNoDefault.AdjustDataType (typeof (object))).ReturnDefaultWhenEmpty, Is.False);
    }

    [Test]
    public void AdjustDataType_IncompatibleType ()
    {
      Assert.That (
          () => _streamedSingleValueInfoNoDefault.AdjustDataType (typeof (string)),
          Throws.ArgumentException.With.Message.EqualTo (
              "'System.String' cannot be used as the new data type for a value of type 'Remotion.Linq.UnitTests.TestDomain.Cook'.\r\n"
              + "Parameter name: dataType"));
    }

    [Test]
    public void MakeClosedGenericExecuteMethod ()
    {
      var executeMethod = typeof (CountResultOperator).GetMethod ("ExecuteInMemory", new[] { typeof (StreamedSequence) });
      var result = _streamedSingleValueInfoNoDefault.MakeClosedGenericExecuteMethod (executeMethod);

      Assert.That (result.GetGenericArguments (), Is.EqualTo (new[] { typeof (Cook) }));
    }

    [Test]
    public void MakeClosedGenericExecuteMethod_NonGenericMethod ()
    {
      var executeMethod = typeof (CountResultOperator).GetMethod ("ExecuteInMemory", new[] { typeof (IStreamedData) });
      Assert.That (
          () => _streamedSingleValueInfoNoDefault.MakeClosedGenericExecuteMethod (executeMethod),
          Throws.ArgumentException.With.Message.EqualTo (
              "GenericMethodDefinition must be a generic method definition.\r\n"
              + "Parameter name: genericMethodDefinition"));
    }

    [Test]
    public void MakeClosedGenericExecuteMethod_NonGenericMethodDefinition ()
    {
      var executeMethod = typeof (CountResultOperator)
          .GetMethod ("ExecuteInMemory", new[] { typeof (StreamedSequence) })
          .MakeGenericMethod (typeof (int));
      Assert.That (
          () => _streamedSingleValueInfoNoDefault.MakeClosedGenericExecuteMethod (executeMethod),
          Throws.ArgumentException.With.Message.EqualTo (
              "GenericMethodDefinition must be a generic method definition.\r\n"
              + "Parameter name: genericMethodDefinition"));
    }

    [Test]
    public void MakeClosedGenericExecuteMethod_WrongNumberOfGenericParameters ()
    {
      var executeMethod = typeof (TestResultOperator).GetMethod ("InvalidExecuteInMemory_TooManyGenericParameters");
      Assert.That (
          () => _streamedSingleValueInfoNoDefault.MakeClosedGenericExecuteMethod (executeMethod),
          Throws.ArgumentException.With.Message.EqualTo (
              "GenericMethodDefinition must have exactly one generic parameter.\r\n"
              + "Parameter name: genericMethodDefinition"));
    }

    [Test]
    public void ExecuteQueryModel_WithDefaultWhenEmpty ()
    {
      var queryModel = ExpressionHelper.CreateQueryModel<Cook> ();
      var student1 = new Cook();
      
      var executorMock = MockRepository.GenerateMock<IQueryExecutor> ();
      executorMock.Expect (mock => mock.ExecuteSingle<Cook> (queryModel, true)).Return (student1);
      
      var streamedData = _streamedSingleValueInfoWithDefault.ExecuteQueryModel (queryModel, executorMock);

      executorMock.VerifyAllExpectations ();

      Assert.That (streamedData, Is.InstanceOf (typeof (StreamedValue)));
      Assert.That (streamedData.DataInfo, Is.SameAs (_streamedSingleValueInfoWithDefault));
      Assert.That (streamedData.Value, Is.EqualTo (student1));
    }

    [Test]
    public void ExecuteQueryModel_NoDefaultWhenEmpty ()
    {
      var queryModel = ExpressionHelper.CreateQueryModel<Cook> ();
      var student1 = new Cook ();

      var executorMock = MockRepository.GenerateMock<IQueryExecutor> ();
      executorMock.Expect (mock => mock.ExecuteSingle<Cook> (queryModel, false)).Return (student1);

      var streamedData = _streamedSingleValueInfoNoDefault.ExecuteQueryModel (queryModel, executorMock);

      executorMock.VerifyAllExpectations ();

      Assert.That (streamedData, Is.InstanceOf (typeof (StreamedValue)));
      Assert.That (streamedData.DataInfo, Is.SameAs (_streamedSingleValueInfoNoDefault));
      Assert.That (streamedData.Value, Is.EqualTo (student1));
    }

    [Test]
    public void ExecuteQueryModel_WithValueType ()
    {
      var queryModel = ExpressionHelper.CreateQueryModel_Int ();

      var executorMock = MockRepository.GenerateMock<IQueryExecutor> ();
      executorMock.Expect (mock => mock.ExecuteSingle<int> (queryModel, true)).Return (5);

      var streamedSingleValueInfo = new StreamedSingleValueInfo (typeof (int), true);
      var streamedData = streamedSingleValueInfo.ExecuteQueryModel (queryModel, executorMock);

      executorMock.VerifyAllExpectations ();

      Assert.That (streamedData, Is.InstanceOf (typeof (StreamedValue)));
      Assert.That (streamedData.DataInfo, Is.SameAs (streamedSingleValueInfo));
      Assert.That (streamedData.Value, Is.EqualTo (5));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "Test")]
    public void ExecuteQueryModel_WithException ()
    {
      var queryModel = ExpressionHelper.CreateQueryModel<Cook> ();

      var executorMock = MockRepository.GenerateMock<IQueryExecutor> ();
      executorMock.Expect (mock => mock.ExecuteSingle<Cook> (queryModel, true)).Throw (new InvalidOperationException ("Test"));

      _streamedSingleValueInfoWithDefault.ExecuteQueryModel (queryModel, executorMock);
    }

    [Test]
    public void Equals ()
    {
      // ReSharper disable EqualExpressionComparison
      Assert.That (new StreamedSingleValueInfo (typeof (int), false).Equals (null), Is.False);
      Assert.That (new StreamedSingleValueInfo (typeof (int), false).Equals (new StreamedSingleValueInfo (typeof (int), false)), Is.True);
      Assert.That (new StreamedSingleValueInfo (typeof (int), false).Equals (new StreamedSingleValueInfo (typeof (int), true)), Is.False);
      Assert.That (new StreamedSingleValueInfo (typeof (int),false).Equals (new StreamedSingleValueInfo (typeof (bool), false)), Is.False);
      // ReSharper restore EqualExpressionComparison
    }

    [Test]
    public void GetHashCodeTest ()
    {
      Assert.That (
          new StreamedSingleValueInfo (typeof (int), false).GetHashCode (), 
          Is.EqualTo (new StreamedSingleValueInfo (typeof (int), false).GetHashCode ()));
    }
  }
}

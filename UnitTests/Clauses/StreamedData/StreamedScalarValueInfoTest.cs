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
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.UnitTests.TestDomain;
using Rhino.Mocks;

namespace Remotion.Linq.UnitTests.Clauses.StreamedData
{
  [TestFixture]
  public class StreamedScalarValueInfoTest
  {
    private StreamedScalarValueInfo _streamedScalarValueInfo;

    [SetUp]
    public void SetUp ()
    {
      _streamedScalarValueInfo = new StreamedScalarValueInfo (typeof (int));
    }

    [Test]
    public void DataType ()
    {
      Assert.That (_streamedScalarValueInfo.DataType, Is.SameAs (typeof (int)));
    }

    [Test]
    public void AdjustDataType_CompatibleType ()
    {
      var result = _streamedScalarValueInfo.AdjustDataType (typeof (object));

      Assert.That (result, Is.Not.SameAs (_streamedScalarValueInfo));
      Assert.That (result, Is.TypeOf (typeof (StreamedScalarValueInfo)));
      Assert.That (result.DataType, Is.SameAs (typeof (object)));
    }

    [Test]
    public void AdjustDataType_IncompatibleType ()
    {
      Assert.That (
          () => _streamedScalarValueInfo.AdjustDataType (typeof (string)),
          Throws.ArgumentException.With.Message.EqualTo (
              "'System.String' cannot be used as the new data type for a value of type 'System.Int32'.\r\nParameter name: dataType"));
    }

    [Test]
    public void ExecuteQueryModel ()
    {
      var queryModel = ExpressionHelper.CreateQueryModel<Cook> ();
      var executorMock = MockRepository.GenerateMock<IQueryExecutor> ();
      executorMock.Expect (mock => mock.ExecuteScalar<int> (queryModel)).Return(1);

      var streamedData = _streamedScalarValueInfo.ExecuteQueryModel (queryModel, executorMock);

      executorMock.VerifyAllExpectations ();

      Assert.That (streamedData, Is.InstanceOf(typeof(StreamedValue)));
      Assert.That (streamedData.DataInfo, Is.SameAs (_streamedScalarValueInfo));
      Assert.That (streamedData.Value, Is.EqualTo (1));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "Test")]
    public void ExecuteQueryModel_WithException ()
    {
      var queryModel = ExpressionHelper.CreateQueryModel<Cook> ();

      var executorMock = MockRepository.GenerateMock<IQueryExecutor> ();
      executorMock.Expect (mock => mock.ExecuteScalar<int> (queryModel)).Throw (new InvalidOperationException ("Test"));

      _streamedScalarValueInfo.ExecuteQueryModel (queryModel, executorMock);
    }

    [Test]
    public void Equals ()
    {
      Assert.That (new StreamedScalarValueInfo (typeof (int)).Equals (null), Is.False);
      Assert.That (new StreamedScalarValueInfo (typeof (int)).Equals (new StreamedScalarValueInfo (typeof (int))), Is.True);
      Assert.That (new StreamedScalarValueInfo (typeof (int)).Equals (new StreamedScalarValueInfo (typeof (bool))), Is.False);
    }

    [Test]
    public void GetHashCodeTest ()
    {
      Assert.That (new StreamedScalarValueInfo (typeof (int)).GetHashCode (), Is.EqualTo (new StreamedScalarValueInfo (typeof (int)).GetHashCode ()));
      Assert.That (new StreamedScalarValueInfo (typeof (int)).GetHashCode (), Is.Not.EqualTo (new StreamedScalarValueInfo (typeof (string)).GetHashCode ()));
    }
  }
}

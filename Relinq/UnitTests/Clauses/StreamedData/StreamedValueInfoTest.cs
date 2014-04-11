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
using Remotion.Linq.Development.UnitTesting.Clauses.StreamedData;
using Remotion.Linq.UnitTests.Clauses.ResultOperators;

namespace Remotion.Linq.UnitTests.Clauses.StreamedData
{
  [TestFixture]
  public class StreamedValueInfoTest
  {
    private StreamedValueInfo _infoWithIntValue;

    [SetUp]
    public void SetUp ()
    {
      _infoWithIntValue = new TestStreamedValueInfo (typeof (int));
    }

    [Test]
    public void DataType ()
    {
      Assert.That (_infoWithIntValue.DataType, Is.SameAs (typeof (int)));
    }

    [Test]
    public void AdjustDataType_CompatibleType ()
    {
      var result = _infoWithIntValue.AdjustDataType (typeof (object));

      Assert.That (result, Is.Not.SameAs (_infoWithIntValue));
      Assert.That (result, Is.TypeOf (typeof (TestStreamedValueInfo)));
      Assert.That (result.DataType, Is.SameAs (typeof (object)));
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage =
        "'System.String' cannot be used as the new data type for a value of type 'System.Int32'.\r\nParameter name: dataType")]
    public void AdjustDataType_IncompatibleType ()
    {
      _infoWithIntValue.AdjustDataType (typeof (string));
    }

    [Test]
    public void MakeClosedGenericExecuteMethod ()
    {
      var executeMethod = typeof (CountResultOperator).GetMethod ("ExecuteInMemory", new[] { typeof (StreamedSequence) });
      var result = _infoWithIntValue.MakeClosedGenericExecuteMethod (executeMethod);

      Assert.That (result.GetGenericArguments (), Is.EqualTo (new[] { typeof (int) }));
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), 
        ExpectedMessage = "GenericMethodDefinition must be a generic method definition.\r\nParameter name: genericMethodDefinition")]
    public void MakeClosedGenericExecuteMethod_NonGenericMethod ()
    {
      var executeMethod = typeof (CountResultOperator).GetMethod ("ExecuteInMemory", new[] { typeof (IStreamedData) });
      _infoWithIntValue.MakeClosedGenericExecuteMethod (executeMethod);
      Assert.Fail ();
    }

    [Test]
    [ExpectedException (typeof (ArgumentException),
        ExpectedMessage = "GenericMethodDefinition must be a generic method definition.\r\nParameter name: genericMethodDefinition")]
    public void MakeClosedGenericExecuteMethod_NonGenericMethodDefinition ()
    {
      var executeMethod = typeof (CountResultOperator)
          .GetMethod ("ExecuteInMemory", new[] { typeof (StreamedSequence) })
          .MakeGenericMethod (typeof (int));
      _infoWithIntValue.MakeClosedGenericExecuteMethod (executeMethod);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException),
        ExpectedMessage = "GenericMethodDefinition must have exactly one generic parameter.\r\nParameter name: genericMethodDefinition")]
    public void MakeClosedGenericExecuteMethod_WrongNumberOfGenericParameters ()
    {
      var executeMethod = typeof (TestResultOperator).GetMethod ("InvalidExecuteInMemory_TooManyGenericParameters");
      _infoWithIntValue.MakeClosedGenericExecuteMethod (executeMethod);
    }

    [Test]
    public void Equals ()
    {
      // ReSharper disable EqualExpressionComparison
      Assert.That (new TestStreamedValueInfo (typeof (int)).Equals ((object) null), Is.False);
      Assert.That (new TestStreamedValueInfo (typeof (int)).Equals ((object) new TestStreamedValueInfo (typeof (int))), Is.True);
      Assert.That (new TestStreamedValueInfo (typeof (int)).Equals ((object) new TestStreamedValueInfo (typeof (bool))), Is.False);
      Assert.That (new TestStreamedValueInfo (typeof (int)).Equals ((object) new StreamedScalarValueInfo (typeof (int))), Is.False);
      // ReSharper restore EqualExpressionComparison
    }

    [Test]
    public void GetHashCodeTest ()
    {
      Assert.That (new TestStreamedValueInfo (typeof (int)).GetHashCode (), Is.EqualTo (new TestStreamedValueInfo (typeof (int)).GetHashCode ()));
    }
  }
}

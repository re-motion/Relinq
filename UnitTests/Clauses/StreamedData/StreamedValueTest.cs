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

namespace Remotion.Linq.UnitTests.Clauses.StreamedData
{
  [TestFixture]
  public class StreamedValueTest
  {
    private StreamedValue _dataWithIntValue;
    private StreamedValue _dataWithNullValue;

    [SetUp]
    public void SetUp ()
    {
      _dataWithIntValue = new StreamedValue (0, new StreamedScalarValueInfo (typeof(int)));
      _dataWithNullValue = new StreamedValue (null, new StreamedSingleValueInfo (typeof(object), false));
    }

    [Test]
    public void Initialization_NullValuePossible ()
    {
      Assert.That (_dataWithNullValue.Value, Is.Null);
      Assert.That (_dataWithNullValue.GetTypedValue<object>(), Is.Null);
    }

    [Test]
    public void DataInfo ()
    {
      Assert.That (_dataWithIntValue.DataInfo.DataType, Is.SameAs (typeof (int)));
    }

    [Test]
    public void DataInfo_Null ()
    {
      Assert.That (_dataWithNullValue.DataInfo.DataType, Is.SameAs (typeof (object)));
    }

    [Test]
    public void GetCurrentSingleValue ()
    {
      Assert.That (_dataWithIntValue.GetTypedValue<int> (), Is.EqualTo (0));
    }

    [Test]
    public void GetCurrentSingleValue_InvalidType ()
    {
      Assert.That (
          () => _dataWithIntValue.GetTypedValue<string> (),
          Throws.InvalidOperationException
              .With.Message.EqualTo ("Cannot retrieve the current value as type 'System.String' because it is of type 'System.Int32'."));
    }

    [Test]
    public void Initialization_CurrentValue_WrongItemExpression ()
    {
      Assert.That (
          () => new StreamedValue (0, new StreamedScalarValueInfo (typeof (string))),
          Throws.ArgumentException
              .With.Message.EqualTo ("Parameter 'value' has type 'System.Int32' when type 'System.String' was expected.\r\nParameter name: value"));
    }
  }
}

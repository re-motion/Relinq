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
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Clauses.StreamedData;

namespace Remotion.Data.Linq.UnitTests.Clauses.StreamedData
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
    [ExpectedException (typeof (InvalidOperationException), 
        ExpectedMessage = "Cannot retrieve the current value as type 'System.String' because it is of type 'System.Int32'.")]
    public void GetCurrentSingleValue_InvalidType ()
    {
      _dataWithIntValue.GetTypedValue<string> ();
    }
  }
}

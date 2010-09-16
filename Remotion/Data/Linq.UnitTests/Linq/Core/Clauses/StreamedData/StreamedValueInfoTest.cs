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
using Remotion.Data.Linq.Clauses.ResultOperators;
using Remotion.Data.Linq.Clauses.StreamedData;
using Remotion.Data.Linq.UnitTests.Linq.Core.Clauses.ResultOperators;

namespace Remotion.Data.Linq.UnitTests.Linq.Core.Clauses.StreamedData
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
  }
}

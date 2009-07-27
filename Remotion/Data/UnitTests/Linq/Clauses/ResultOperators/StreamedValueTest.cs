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
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Clauses.ResultOperators;

namespace Remotion.Data.UnitTests.Linq.Clauses.ResultOperators
{
  [TestFixture]
  public class StreamedValueTest
  {
    private StreamedValue _dataWithIntValue;
    private StreamedValue _dataWithNullValue;

    [SetUp]
    public void SetUp ()
    {
      _dataWithIntValue = new StreamedValue (0);
      _dataWithNullValue = new StreamedValue (null);
    }

    [Test]
    public void Initialization_NullValuePossible ()
    {
      Assert.That (_dataWithNullValue.CurrentValue, Is.Null);
      Assert.That (_dataWithNullValue.GetCurrentSingleValue<object>(), Is.Null);
    }

    [Test]
    public void DataType ()
    {
      Assert.That (_dataWithIntValue.DataType, Is.SameAs (typeof (int)));
    }

    [Test]
    public void DataType_Null ()
    {
      Assert.That (_dataWithNullValue.DataType, Is.SameAs (typeof (object)));
    }

    [Test]
    public void GetCurrentSingleValue ()
    {
      Assert.That (_dataWithIntValue.GetCurrentSingleValue<int> (), Is.EqualTo (0));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), 
        ExpectedMessage = "Cannot retrieve the current value as type 'System.String' because it is of type 'System.Int32'.")]
    public void GetCurrentSingleValue_InvalidType ()
    {
      _dataWithIntValue.GetCurrentSingleValue<string> ();
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), 
        ExpectedMessage = "Cannot retrieve the current value as a sequence because it is a value.")]
    public void GetCurrentSequence_NoSequence ()
    {
      ((IStreamedData) _dataWithIntValue).GetCurrentSequenceInfo<int> ();
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException),
        ExpectedMessage = "Cannot retrieve the current value as a sequence because it is a value.")]
    public void GetCurrentSequence_Untyped_NoSequence ()
    {
      ((IStreamedData) _dataWithIntValue).GetCurrentSequenceInfo ();
    }

    [Test]
    public void MakeClosedGenericExecuteMethod ()
    {
      var executeMethod = typeof (CountResultOperator).GetMethod ("ExecuteInMemory", new[] { typeof (StreamedSequence) });
      var result = _dataWithIntValue.MakeClosedGenericExecuteMethod (executeMethod);

      Assert.That (result.GetGenericArguments (), Is.EqualTo (new[] { typeof (int) }));
    }

    [Test]
    public void MakeClosedGenericExecuteMethod_Null ()
    {
      var executeMethod = typeof (CountResultOperator).GetMethod ("ExecuteInMemory", new[] { typeof (StreamedSequence) });
      var result = _dataWithNullValue.MakeClosedGenericExecuteMethod (executeMethod);

      Assert.That (result.GetGenericArguments (), Is.EqualTo (new[] { typeof (object) }));
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), 
        ExpectedMessage = "GenericMethodDefinition must be a generic method definition.\r\nParameter name: genericMethodDefinition")]
    public void MakeClosedGenericExecuteMethod_NonGenericMethod ()
    {
      var executeMethod = typeof (CountResultOperator).GetMethod ("ExecuteInMemory", new[] { typeof (IStreamedData) });
      _dataWithIntValue.MakeClosedGenericExecuteMethod (executeMethod);
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
      _dataWithIntValue.MakeClosedGenericExecuteMethod (executeMethod);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException),
        ExpectedMessage = "GenericMethodDefinition must have exactly one generic parameter.\r\nParameter name: genericMethodDefinition")]
    public void MakeClosedGenericExecuteMethod_WrongNumberOfGenericParameters ()
    {
      var executeMethod = typeof (TestResultOperator).GetMethod ("InvalidExecuteInMemory_TooManyGenericParameters");
      _dataWithIntValue.MakeClosedGenericExecuteMethod (executeMethod);
    }
  }
}
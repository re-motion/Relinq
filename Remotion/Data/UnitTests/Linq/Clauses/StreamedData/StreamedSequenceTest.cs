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
using System.Linq.Expressions;
using Remotion.Data.Linq.Clauses.StreamedData;
using Remotion.Data.UnitTests.Linq.Clauses.ResultOperators;
using Remotion.Utilities;

namespace Remotion.Data.UnitTests.Linq.Clauses.StreamedData
{
  [TestFixture]
  public class StreamedSequenceTest
  {
    private ConstantExpression _intExpression;
    private int[] _intSequence;
    private StreamedSequence _dataWithIntSequence;

    [SetUp]
    public void SetUp ()
    {
      _intExpression = Expression.Constant (0);
      _intSequence = new[] { 0, 0, 0 };

      _dataWithIntSequence = new StreamedSequence (_intSequence, _intExpression);
    }

    [Test]
    [ExpectedException (typeof (ArgumentNullException))]
    public void Initialization_CurrentSequence_WithoutItemExpression ()
    {
      new StreamedSequence (_intSequence, null);
    }

    [Test]
    [ExpectedException (typeof (ArgumentTypeException))]
    public void Initialization_CurrentSequence_WrongItemExpression ()
    {
      new StreamedSequence (new[] { "1", "2", "3" }, _intExpression);
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), 
        ExpectedMessage = "Cannot retrieve the current single value because the current value is a sequence of type 'System.Int32[]'.")]
    public void GetCurrentSingleValue_InvalidType ()
    {
      ((IStreamedData) _dataWithIntSequence).GetCurrentSingleValue<string> ();
    }

    [Test]
    public void DataType ()
    {
      Assert.That (_dataWithIntSequence.DataType, Is.SameAs (typeof (int[])));
    }

    [Test]
    public void GetCurrentSequence ()
    {
      var sequenceData = _dataWithIntSequence.GetCurrentSequenceInfo<int> ();
      Assert.That (sequenceData.Sequence, Is.SameAs (_intSequence));
      Assert.That (sequenceData.ItemExpression, Is.SameAs (_intExpression));
    }

    [Test]
    public void GetCurrentSequence_Untyped ()
    {
      var sequenceData = _dataWithIntSequence.GetCurrentSequenceInfo ();
      Assert.That (sequenceData.Sequence, Is.SameAs (_intSequence));
      Assert.That (sequenceData.ItemExpression, Is.SameAs (_intExpression));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "Cannot retrieve the current value as a sequence with item type "
                                                                              + "'System.String' because its items are of type 'System.Int32'.")]
    public void GetCurrentSequence_InvalidItemType ()
    {
      _dataWithIntSequence.GetCurrentSequenceInfo<string> ();
    }

    [Test]
    public void MakeClosedGenericExecuteMethod ()
    {
      var executeMethod = typeof (CountResultOperator).GetMethod ("ExecuteInMemory", new[] { typeof (StreamedSequence) });
      var result = _dataWithIntSequence.MakeClosedGenericExecuteMethod (executeMethod);

      Assert.That (result.GetGenericArguments (), Is.EqualTo (new[] { typeof (int) }));
    }

    [Test]
    [ExpectedException (typeof (ArgumentException),
        ExpectedMessage = "GenericMethodDefinition must be a generic method definition.\r\nParameter name: genericMethodDefinition")]
    public void MakeClosedGenericExecuteMethod_NonGenericMethod ()
    {
      var executeMethod = typeof (CountResultOperator).GetMethod ("ExecuteInMemory", new[] { typeof (IStreamedData) });
      _dataWithIntSequence.MakeClosedGenericExecuteMethod (executeMethod);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException),
        ExpectedMessage = "GenericMethodDefinition must be a generic method definition.\r\nParameter name: genericMethodDefinition")]
    public void MakeClosedGenericExecuteMethod_NonGenericMethodDefinition ()
    {
      var executeMethod = typeof (CountResultOperator)
          .GetMethod ("ExecuteInMemory", new[] { typeof (StreamedSequence) })
          .MakeGenericMethod (typeof (int));
      _dataWithIntSequence.MakeClosedGenericExecuteMethod (executeMethod);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException),
        ExpectedMessage = "GenericMethodDefinition must have exactly one generic parameter.\r\nParameter name: genericMethodDefinition")]
    public void MakeClosedGenericExecuteMethod_WrongNumberOfGenericParameters ()
    {
      var executeMethod = typeof (TestResultOperator).GetMethod ("InvalidExecuteInMemory_TooManyGenericParameters");
      _dataWithIntSequence.MakeClosedGenericExecuteMethod (executeMethod);
    }
  }
}
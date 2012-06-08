// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (c) rubicon IT GmbH, www.rubicon.eu
// 
// re-linq is free software; you can redistribute it and/or modify it under 
// the terms of the GNU Lesser General Public License as published by the 
// Free Software Foundation; either version 2.1 of the License, 
// or (at your option) any later version.
// 
// re-linq is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-linq; if not, see http://www.gnu.org/licenses.
// 
using System;
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.Utilities;

namespace Remotion.Linq.UnitTests.Linq.Core.Clauses.StreamedData
{
  [TestFixture]
  public class StreamedSequenceTest
  {
    private ConstantExpression _stringExpression;
    private string[] _stringSequence;
    private StreamedSequence _dataWithCovariantSequence;

    [SetUp]
    public void SetUp ()
    {
      _stringExpression = Expression.Constant (0);
      _stringSequence = new[] { "a", "b", "c" };

      _dataWithCovariantSequence = new StreamedSequence (_stringSequence, new StreamedSequenceInfo (typeof (object[]), _stringExpression));
    }

    [Test]
    [ExpectedException (typeof (ArgumentNullException))]
    public void Initialization_CurrentSequence_WithoutItemExpression ()
    {
      new StreamedSequence (_stringSequence, null);
    }

    [Test]
    [ExpectedException (typeof (ArgumentTypeException), ExpectedMessage = 
        "Argument sequence has type System.String[] when type System.Int32[] was expected.\r\nParameter name: sequence")]
    public void Initialization_CurrentSequence_WrongItemExpression ()
    {
      new StreamedSequence (new[] { "1", "2", "3" }, new StreamedSequenceInfo (typeof (int[]), _stringExpression));
    }

    [Test]
    public void DataInfo ()
    {
      Assert.That (_dataWithCovariantSequence.DataInfo.DataType, Is.SameAs (typeof (object[])));
      Assert.That (_dataWithCovariantSequence.DataInfo.ResultItemType, Is.SameAs (typeof (object)));
      Assert.That (_dataWithCovariantSequence.DataInfo.ItemExpression, Is.SameAs (_stringExpression));
    }

    [Test]
    public void GetTypedSequence ()
    {
      var sequenceData = _dataWithCovariantSequence.GetTypedSequence<string> ();
      Assert.That (sequenceData, Is.SameAs (_stringSequence));

      var covariantSequenceData = _dataWithCovariantSequence.GetTypedSequence<object> ();
      Assert.That (covariantSequenceData, Is.SameAs (_stringSequence));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = 
        "Cannot retrieve the current value as a sequence with item type 'System.Int32' because its items are of type 'System.Object'.")]
    public void GetTypedSequence_InvalidItemType ()
    {
      _dataWithCovariantSequence.GetTypedSequence<int>();
    }
  }
}

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
using System.Collections;
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Clauses.StreamedData;
using Remotion.Utilities;

namespace Remotion.Data.Linq.UnitTests.Clauses.StreamedData
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

      _dataWithIntSequence = new StreamedSequence (_intSequence, new StreamedSequenceInfo (typeof (int[]), _intExpression));
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
      new StreamedSequence (new[] { "1", "2", "3" }, new StreamedSequenceInfo (typeof (string[]), _intExpression));
    }

    [Test]
    public void DataInfo ()
    {
      Assert.That (_dataWithIntSequence.DataInfo.DataType, Is.SameAs (typeof (int[])));
      Assert.That (_dataWithIntSequence.DataInfo.ItemExpression, Is.SameAs (_intExpression));
    }

    [Test]
    public void GetTypedSequence ()
    {
      var sequenceData = _dataWithIntSequence.GetTypedSequence<int> ();
      Assert.That (sequenceData, Is.SameAs (_intSequence));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "Cannot retrieve the current value as a sequence with item type "
                                                                              + "'System.String' because its items are of type 'System.Int32'.")]
    public void GetTypedSequence_InvalidItemType ()
    {
      _dataWithIntSequence.GetTypedSequence<string> ();
    }
  }
}

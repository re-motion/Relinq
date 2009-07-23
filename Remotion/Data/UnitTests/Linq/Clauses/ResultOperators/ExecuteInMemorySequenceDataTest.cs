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
using System.Collections.Generic;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Collections;
using Remotion.Data.Linq.Clauses.ResultOperators;
using System.Linq.Expressions;
using Remotion.Utilities;

namespace Remotion.Data.UnitTests.Linq.Clauses.ResultOperators
{
  [TestFixture]
  public class ExecuteInMemorySequenceDataTest
  {
    private ConstantExpression _intExpression;
    private int[] _intSequence;
    private ExecuteInMemorySequenceData _dataWithIntSequence;

    [SetUp]
    public void SetUp ()
    {
      _intExpression = Expression.Constant (0);
      _intSequence = new[] { 0, 0, 0 };

      _dataWithIntSequence = new ExecuteInMemorySequenceData (_intSequence, _intExpression);
    }

    [Test]
    [ExpectedException (typeof (ArgumentNullException))]
    public void Initialization_CurrentSequence_WithoutItemExpression ()
    {
      new ExecuteInMemorySequenceData (_intSequence, null);
    }

    [Test]
    [ExpectedException (typeof (ArgumentTypeException))]
    public void Initialization_CurrentSequence_WrongItemExpression ()
    {
      new ExecuteInMemorySequenceData (new[] { "1", "2", "3" }, _intExpression);
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), 
        ExpectedMessage = "Cannot retrieve the current single value because the current value is a sequence of type 'System.Int32[]'.")]
    public void GetCurrentSingleValue_InvalidType ()
    {
      _dataWithIntSequence.GetCurrentSingleValue<string> ();
    }

    [Test]
    public void GetCurrentSequence ()
    {
      Tuple<IEnumerable<int>, Expression> sequenceData = _dataWithIntSequence.GetCurrentSequence<int> ();
      Assert.That (sequenceData.A, Is.SameAs (_intSequence));
      Assert.That (sequenceData.B, Is.SameAs (_intExpression));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "Cannot retrieve the current value as a sequence with item type "
        + "'System.String' because its items are of type 'System.Int32'.")]
    public void GetCurrentSequence_InvalidItemType ()
    {
      _dataWithIntSequence.GetCurrentSequence<string> ();
    }
  }
}
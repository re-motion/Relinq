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
  public class ExecuteInMemoryValueDataTest
  {
    private ExecuteInMemoryValueData _dataWithIntValue;

    [SetUp]
    public void SetUp ()
    {
      _dataWithIntValue = new ExecuteInMemoryValueData (0);
    }

    [Test]
    public void Initialization_NullValuePossible ()
    {
      var dataWithNullValue = new ExecuteInMemoryValueData (null);
      Assert.That (dataWithNullValue.CurrentValue, Is.Null);
      Assert.That (dataWithNullValue.GetCurrentSingleValue<object>(), Is.Null);
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
      _dataWithIntValue.GetCurrentSequence<int> ();
    }
  }
}
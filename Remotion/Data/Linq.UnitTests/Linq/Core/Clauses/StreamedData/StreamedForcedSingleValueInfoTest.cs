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
using Remotion.Data.Linq.UnitTests.Linq.Core.TestDomain;

namespace Remotion.Data.Linq.UnitTests.Linq.Core.Clauses.StreamedData
{
  [TestFixture]
  public class StreamedForcedSingleValueInfoTest
  {
    private StreamedForcedSingleValueInfo _streamedForcedSingleValueInfoWithDefault;
    private StreamedForcedSingleValueInfo _streamedForcedSingleValueInfoNoDefault;

    [SetUp]
    public void SetUp ()
    {
      _streamedForcedSingleValueInfoWithDefault = new StreamedForcedSingleValueInfo (typeof (Cook), true);
      _streamedForcedSingleValueInfoNoDefault = new StreamedForcedSingleValueInfo (typeof (Restaurant), false);
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_streamedForcedSingleValueInfoWithDefault.DataType, Is.EqualTo(typeof (Cook)));
      Assert.That (_streamedForcedSingleValueInfoWithDefault.ReturnDefaultWhenEmpty, Is.True);
      Assert.That (_streamedForcedSingleValueInfoNoDefault.DataType, Is.EqualTo(typeof (Restaurant)));
      Assert.That (_streamedForcedSingleValueInfoNoDefault.ReturnDefaultWhenEmpty, Is.False);
    }

    [Test]
    public void AdjustDataInfo ()
    {
      Assert.That (_streamedForcedSingleValueInfoWithDefault.AdjustDataType (typeof (object)), Is.TypeOf (typeof (StreamedForcedSingleValueInfo)));
    }
  }
}
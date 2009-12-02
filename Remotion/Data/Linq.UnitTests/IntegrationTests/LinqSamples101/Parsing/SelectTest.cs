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
using System.Linq;
using NUnit.Framework;
using Remotion.Data.UnitTests.Linq.IntegrationTests.LinqSamples101.TestDomain;

namespace Remotion.Data.UnitTests.Linq.IntegrationTests.LinqSamples101.Parsing
{
  /// <summary>
  /// http://msdn.microsoft.com/en-us/bb737936.aspx
  /// </summary>
  [TestFixture]
  public class SelectTest : TestBase
  {
    [Test]
    public void Test_Simple ()
    {
      CheckParsedQuery (
          () => from c in QuerySource.Customers select c.ContactName,
          "from Customer c in Customers select [c].ContactName");
    }
  }
}

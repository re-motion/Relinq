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
using System.Reflection;
using NUnit.Framework;
using Remotion.Data.Linq.IntegrationTests.TestDomain.Northwind;

namespace Remotion.Data.Linq.UnitTests
{
  [TestFixture]
  public class ResolutionTests
  {
    private Northwind _db;

    [SetUp]
    public void SetUp()
    {
      _db = new Northwind ("Data Source=localhost;Initial Catalog=Northwind; Integrated Security=SSPI;");
    }

    [Test]
    public void ResoltutionTest01()
    {
      string resolvedName=_db.Mapping.GetTable (typeof (Customer)).TableName;
      Assert.AreEqual ("dbo.Customers", resolvedName);

      PropertyInfo name = typeof (Customer).GetProperty ("CompanyName");
      string dataMemberName=_db.Mapping.GetTable (typeof (Customer)).RowType.GetDataMember (name).Name;
      Assert.AreEqual (dataMemberName, "CompanyName");
    }

    [TearDown]
    public void TearDown()
    {
      _db.Dispose();
      _db = null;
    }
  }
}
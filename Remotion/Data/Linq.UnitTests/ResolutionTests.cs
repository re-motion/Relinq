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
      string resolvedName=_db.Mapping.GetTable (typeof (Customer)).TableName; //This will return the mapped TableName of the Class Customer
      Assert.AreEqual ("dbo.Customers", resolvedName);

      PropertyInfo nameInfo = typeof (Customer).GetProperty ("CompanyName"); //Reflection Method to get a Property of a Class
      string resolvedRowName = _db.Mapping.GetTable (typeof (Customer)).RowType.GetDataMember (nameInfo).Name; //This will return the mapped RowName
      Assert.AreEqual ("CompanyName", resolvedRowName);
    }

    [Test]
    public void ReturnTypeFromTableTest ()
    {
      //var x = _db.Mapping.GetMetaType (_db.GetType ().GetProperty ("Employees").PropertyType);

      var tableCol = _db.Mapping.GetTables();
      foreach (var table in tableCol)
      {
        //TODO: tableName startswith dbo. or not?
        //if (x.TableName.Equals("dbo.Customers"))
        if (table.RowType.Name.Equals ("Customers"))
        {
          Assert.IsTrue (table.GetType() == typeof (Customer));
          break;
        }
      }
    }

    [TearDown]
    public void TearDown()
    {
      _db.Dispose();
      _db = null;
    }
  }
}
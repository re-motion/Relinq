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
using System.Reflection;
using NUnit.Framework;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing.FieldResolving;

namespace Remotion.Data.UnitTests.Linq.DataObjectModel
{
  [TestFixture]
  public class FieldDescriptorTest
  {
    [Test]
    [ExpectedException (typeof (ArgumentNullException),ExpectedMessage = "Either member or column must have a value.\r\n"+
      "Parameter name: member && column")]
    public void MemberAndColumnNull()
    {
      new FieldDescriptor (null, ExpressionHelper.GetPathForNewTable(), null);
    }

    [Test]
    public void MemberNull()
    {
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause();
      Column column = new Column();
      FieldSourcePath path = ExpressionHelper.GetPathForNewTable();
      FieldDescriptor descriptor = new FieldDescriptor (null, path, column);
      Assert.IsNull (descriptor.Member);
      Assert.AreEqual (column, descriptor.Column);
      Assert.AreEqual (path, descriptor.SourcePath);
    }

    [Test]
    public void ColumnNull ()
    {
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause ();
      FieldSourcePath path = ExpressionHelper.GetPathForNewTable();
      MemberInfo member = typeof (Student).GetProperty ("First");
      FieldDescriptor descriptor = new FieldDescriptor (member,path, null);
      Assert.IsNull (descriptor.Column);
      Assert.AreEqual (member, descriptor.Member);
      Assert.AreEqual (path, descriptor.SourcePath);
    }

    [Test]
    public void MemberAndColumnNotNull ()
    {
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause ();
      FieldSourcePath path = ExpressionHelper.GetPathForNewTable ();
      MemberInfo member = typeof (Student).GetProperty ("First");
      Column column = new Column ();
      FieldDescriptor descriptor = new FieldDescriptor (member, path, column);
      Assert.AreEqual (column, descriptor.Column);
      Assert.AreEqual (member, descriptor.Member);
      Assert.AreEqual (path, descriptor.SourcePath);
    }
    
    [Test]
    public void GetMandatoryColumn()
    {
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause ();
      FieldSourcePath path = ExpressionHelper.GetPathForNewTable ();
      MemberInfo member = typeof (Student).GetProperty ("First");
      Column column = new Column ();
      FieldDescriptor descriptor = new FieldDescriptor (member, path, column);
      Assert.AreEqual (column, descriptor.GetMandatoryColumn());
    }
    
    [Test]
    [ExpectedException (typeof (FieldAccessResolveException), ExpectedMessage = "The member 'Remotion.Data.UnitTests.Linq.Student.First' "
      + "does not identify a queryable column.")]
    public void GetMandatoryColumnWithException ()
    {
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause ();
      FieldSourcePath path = ExpressionHelper.GetPathForNewTable ();
      MemberInfo member = typeof (Student).GetProperty ("First");
      new FieldDescriptor (member, path, null).GetMandatoryColumn ();
    }
    
  }
}

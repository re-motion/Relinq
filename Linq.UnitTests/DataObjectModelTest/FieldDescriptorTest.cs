using System;
using System.Reflection;
using NUnit.Framework;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Data.Linq.Parsing.FieldResolving;

namespace Rubicon.Data.Linq.UnitTests.DataObjectModelTest
{
  [TestFixture]
  public class FieldDescriptorTest
  {
    [Test]
    [ExpectedException (typeof (ArgumentNullException),ExpectedMessage = "Either member or column must have a value.\r\n"+
      "Parameter name: member && column")]
    public void MemberAndColumnNull()
    {
      new FieldDescriptor (null, ExpressionHelper.CreateMainFromClause(), ExpressionHelper.GetPathForNewTable(), null);
    }

    [Test]
    public void MemberNull()
    {
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause();
      Column column = new Column();
      FieldSourcePath path = ExpressionHelper.GetPathForNewTable();
      FieldDescriptor descriptor = new FieldDescriptor (null, fromClause, path, column);
      Assert.IsNull (descriptor.Member);
      Assert.AreSame (fromClause, descriptor.FromClause);
      Assert.AreEqual (column, descriptor.Column);
      Assert.AreEqual (path, descriptor.SourcePath);
    }

    [Test]
    public void ColumnNull ()
    {
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause ();
      FieldSourcePath path = ExpressionHelper.GetPathForNewTable();
      MemberInfo member = typeof (Student).GetProperty ("First");
      FieldDescriptor descriptor = new FieldDescriptor (member, fromClause, path, null);
      Assert.IsNull (descriptor.Column);
      Assert.AreSame (fromClause, descriptor.FromClause);
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
      FieldDescriptor descriptor = new FieldDescriptor (member, fromClause, path, column);
      Assert.AreEqual (column, descriptor.Column);
      Assert.AreSame (fromClause, descriptor.FromClause);
      Assert.AreEqual (member, descriptor.Member);
      Assert.AreEqual (path, descriptor.SourcePath);
    }

    [Test]
    public void GetMandatoryIColumn ()
    {
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause ();
      FieldSourcePath path = ExpressionHelper.GetPathForNewTable ();
      MemberInfo member = typeof (Student).GetProperty ("First");
      IColumn column = new VirtualColumn ();
      FieldDescriptor descriptor = new FieldDescriptor (member, fromClause, path, column);
      Assert.AreEqual (column, descriptor.GetMandatoryIColumn ());
    }

    [Test]
    public void GetMandatoryColumn()
    {
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause ();
      FieldSourcePath path = ExpressionHelper.GetPathForNewTable ();
      MemberInfo member = typeof (Student).GetProperty ("First");
      Column column = new Column ();
      FieldDescriptor descriptor = new FieldDescriptor (member, fromClause, path, column);
      Assert.AreEqual (column, descriptor.GetMandatoryColumn());
    }

    [Test]
    [ExpectedException (typeof (FieldAccessResolveException), ExpectedMessage = "The member 'Rubicon.Data.Linq.UnitTests.Student.First' "
      + "does not identify a queryable column.")]
    public void GetMandatoryIColumnWithException ()
    {
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause ();
      FieldSourcePath path = ExpressionHelper.GetPathForNewTable ();
      MemberInfo member = typeof (Student).GetProperty ("First");
      new FieldDescriptor (member, fromClause, path, null).GetMandatoryIColumn ();
    }

    [Test]
    [ExpectedException (typeof (FieldAccessResolveException), ExpectedMessage = "The member 'Rubicon.Data.Linq.UnitTests.Student.First' "
      + "does not identify a queryable column.")]
    public void GetMandatoryColumnWithException ()
    {
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause ();
      FieldSourcePath path = ExpressionHelper.GetPathForNewTable ();
      MemberInfo member = typeof (Student).GetProperty ("First");
      new FieldDescriptor (member, fromClause, path, null).GetMandatoryColumn ();
    }

    [Test]
    [ExpectedException (typeof (FieldAccessResolveException), ExpectedMessage = "The member 'Rubicon.Data.Linq.UnitTests.Student.First' "
      + "is a virtual column, which cannot be used in this context.")]
    public void GetMandatoryColumnWithVirtualColumn ()
    {
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause ();
      FieldSourcePath path = ExpressionHelper.GetPathForNewTable ();
      MemberInfo member = typeof (Student).GetProperty ("First");
      IColumn column = new VirtualColumn ();
      new FieldDescriptor (member, fromClause, path, column).GetMandatoryColumn ();
    }


  }
}
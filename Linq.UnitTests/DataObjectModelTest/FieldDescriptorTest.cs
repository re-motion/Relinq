using System;
using System.Reflection;
using NUnit.Framework;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Data.Linq.DataObjectModel;

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
      new FieldDescriptor (null, ExpressionHelper.CreateMainFromClause(), new Table(), null);
    }

    [Test]
    public void MemberNull()
    {
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause();
      Table table = new Table();
      Column column = new Column();
      FieldDescriptor descriptor = new FieldDescriptor (null, fromClause, table, column);
      Assert.IsNull (descriptor.Member);
      Assert.AreSame (fromClause, descriptor.FromClause);
      Assert.AreEqual (column, descriptor.Column);
      Assert.AreEqual (table, descriptor.Source);
    }

    [Test]
    public void ColumnNull ()
    {
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause ();
      Table table = new Table ();
      MemberInfo member = typeof (Student).GetProperty ("First");
      FieldDescriptor descriptor = new FieldDescriptor (member, fromClause, table, null);
      Assert.IsNull (descriptor.Column);
      Assert.AreSame (fromClause, descriptor.FromClause);
      Assert.AreEqual (member, descriptor.Member);
      Assert.AreEqual (table, descriptor.Source);
    }

    [Test]
    public void MemberAndColumnNotNull ()
    {
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause ();
      Table table = new Table ();
      MemberInfo member = typeof (Student).GetProperty ("First");
      Column column = new Column ();
      FieldDescriptor descriptor = new FieldDescriptor (member, fromClause, table, column);
      Assert.AreEqual (column, descriptor.Column);
      Assert.AreSame (fromClause, descriptor.FromClause);
      Assert.AreEqual (member, descriptor.Member);
      Assert.AreEqual (table, descriptor.Source);
    }

    [Test]
    public void GetMandatoryColumn()
    {
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause ();
      Table table = new Table ();
      MemberInfo member = typeof (Student).GetProperty ("First");
      Column column = new Column ();
      FieldDescriptor descriptor = new FieldDescriptor (member, fromClause, table, column);
      Assert.AreEqual (column, descriptor.GetMandatoryColumn());
    }


    [Test]
    [ExpectedException (typeof (FieldAccessResolveException), ExpectedMessage = "The member 'Rubicon.Data.Linq.UnitTests.Student.First' "
      + "does not identify a queryable column.")]
    public void GetMandatoryColumnWithException ()
    {
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause ();
      Table table = new Table ("x", "y");
      MemberInfo member = typeof (Student).GetProperty ("First");
      new FieldDescriptor (member, fromClause, table, null).GetMandatoryColumn();
    }


  }
}
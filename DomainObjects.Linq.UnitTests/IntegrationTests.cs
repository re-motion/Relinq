using System;
using System.Linq;
using NUnit.Framework;
using Rubicon.Data.DomainObjects.UnitTests;
using Rubicon.Data.DomainObjects.UnitTests.TestDomain;
using NUnit.Framework.SyntaxHelpers;

namespace Rubicon.Data.DomainObjects.Linq.UnitTests
{
  [TestFixture]
  public class IntegrationTests : ClientTransactionBaseTest
  {
    [Test]
    public void QueryWithWhereConditions()
    {
      var computers =
          from c in DataContext.Entity<Computer> (new TestQueryListener ())
          where c.SerialNumber == "93756-ndf-23" || c.SerialNumber == "98678-abc-43"
          select c;
      Computer[] computerArray = computers.ToArray();
      Assert.That (computerArray,
          Is.EquivalentTo (new object[] { Computer.GetObject (DomainObjectIDs.Computer2), Computer.GetObject (DomainObjectIDs.Computer5) }));
    }

    [Test]
    public void QueryWithWhereConditionsAndNull ()
    {
      var computers =
          from c in DataContext.Entity<Computer> (new TestQueryListener ())
          where c.Employee != null
          select c;
      Computer[] computerArray = computers.ToArray ();
      Assert.That (computerArray,
          Is.EquivalentTo (new object[] { Computer.GetObject (DomainObjectIDs.Computer1), Computer.GetObject (DomainObjectIDs.Computer2),
          Computer.GetObject (DomainObjectIDs.Computer3)}));
    }

    [Test]
    public void QueryWithWhereConditionAndStartsWith ()
    {
      var computers = 
        from c in DataContext.Entity<Computer> (new TestQueryListener())
        where c.SerialNumber.StartsWith("9")
        select c;
      Computer[] computerArray = computers.ToArray ();
      Assert.That (computerArray,
          Is.EquivalentTo (new object[] {Computer.GetObject (DomainObjectIDs.Computer5), Computer.GetObject (DomainObjectIDs.Computer2)}));
    }

    [Test]
    public void QueryWithWhereConditionAndEndsWith ()
    {
      var computers =
        from c in DataContext.Entity<Computer> (new TestQueryListener ())
        where c.SerialNumber.EndsWith ("7")
        select c;
      Computer[] computerArray = computers.ToArray ();
      Assert.That (computerArray,
          Is.EquivalentTo (new object[] { Computer.GetObject (DomainObjectIDs.Computer3) }));
    }

    [Test]
    [Ignore ("TODO: This will only work with joins.")]
    public void QueryWithVirtualKeySide ()
    {
      var employees =
          from e in DataContext.Entity<Employee> (new TestQueryListener ())
          where e.Computer != null
          select e;
      Employee[] employeeArray = employees.ToArray ();
      Assert.That (employeeArray,
          Is.EquivalentTo (new object[] { Employee.GetObject (DomainObjectIDs.Employee3), Computer.GetObject (DomainObjectIDs.Employee4),
          Computer.GetObject (DomainObjectIDs.Employee5)}));
    }

    [Test]
    [Ignore ("TODO: Extend IDatabaseInfo to recognize that a constant value is a DomainObject and use its ID instead of the object itself.")]
    public void QueryWithWhereConditionsAndVariableAccess ()
    {
      Employee employee = Employee.GetObject (DomainObjectIDs.Employee3);
      var computers =
          from c in DataContext.Entity<Computer> (new TestQueryListener ())
          where c.Employee == employee
          select c;
      Computer[] computerArray = computers.ToArray ();
      Assert.That (computerArray,
          Is.EquivalentTo (new object[] { Computer.GetObject (DomainObjectIDs.Computer1)}));
    }
  }
}
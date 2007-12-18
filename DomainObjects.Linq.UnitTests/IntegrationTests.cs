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
    [Ignore ("TODO: Apply partial tree evaluator")]
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
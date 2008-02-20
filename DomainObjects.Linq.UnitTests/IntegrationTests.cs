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
      
      CheckQueryResult (computers, DomainObjectIDs.Computer2, DomainObjectIDs.Computer5);
    }

    [Test]
    [Ignore ("TODO: Make entity members work correctly in where clauses.")]
    public void QueryWithWhereConditionsAndNull ()
    {
      var computers =
          from c in DataContext.Entity<Computer> (new TestQueryListener ())
          where c.Employee != null
          select c;

      CheckQueryResult (computers, DomainObjectIDs.Computer1, DomainObjectIDs.Computer2, DomainObjectIDs.Computer3);
    }

    [Test]
    public void QueryWithWhereConditionAndStartsWith ()
    {
      var computers = 
        from c in DataContext.Entity<Computer> (new TestQueryListener())
        where c.SerialNumber.StartsWith("9")
        select c;

      CheckQueryResult (computers, DomainObjectIDs.Computer2, DomainObjectIDs.Computer5);
    }

    [Test]
    public void QueryWithWhereConditionAndEndsWith ()
    {
      var computers =
        from c in DataContext.Entity<Computer> (new TestQueryListener ())
        where c.SerialNumber.EndsWith ("7")
        select c;

      CheckQueryResult (computers, DomainObjectIDs.Computer3);
    }

    [Test]
    [Ignore ("TODO: This will only work with joins.")]
    public void QueryWithVirtualKeySide ()
    {
      var employees =
          from e in DataContext.Entity<Employee> (new TestQueryListener ())
          where e.Computer != null
          select e;

      CheckQueryResult (employees, DomainObjectIDs.Employee3, DomainObjectIDs.Employee4, DomainObjectIDs.Employee5);
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

      CheckQueryResult (computers, DomainObjectIDs.Computer1);
    }

    [Test]
    public void QueryWithOrderByAndImplicitJoin()
    {
      var orders =
          from o in DataContext.Entity<Order> (new TestQueryListener ())
          where o.OrderNumber <= 4
          orderby o.Customer.Name
          select o;

      Order[] expected =
          GetExpectedObjects<Order> (DomainObjectIDs.OrderWithoutOrderItem, DomainObjectIDs.Order1, DomainObjectIDs.Order2, DomainObjectIDs.Order3);
      Assert.That (orders.ToArray(), Is.EqualTo (expected));
    }

    [Test]
    [Ignore ("TODO: Find solution for querying the virtual side of relations.")]
    public void QueryWithSelectAndImplicitJoin_VirtualSide ()
    {
      var ceos =
          from o in DataContext.Entity<Order> (new TestQueryListener ())
          select o.Customer.Ceo;

      CheckQueryResult (ceos); // TODO
    }

    [Test]
    public void QueryWithSelectAndImplicitJoin ()
    {
      var ceos =
          from o in DataContext.Entity<Order> (new TestQueryListener ())
          where o.Customer.Ceo.Name == "Hugo Boss"
          select o.Customer.Ceo;

      CheckQueryResult (ceos, DomainObjectIDs.Ceo5); // TODO
    }

    [Test]
    [Ignore ("TODO: Flatten joins when adding them in SqlGeneratorVisitor to avoid adding join parts twice")]
    public void QueryWithSelectAndImplicitJoin_UsingJoinPartTwice ()
    {
      var ceos =
          from o in DataContext.Entity<Order> (new TestQueryListener())
          where o.Customer.Name == "Kunde 3"
          select o.Customer.Ceo;

      CheckQueryResult (ceos); // TODO
    }

    [Test]
    [Ignore ("TODO: Support distinct")]
    public void QueryWithDistinct ()
    {
      var ceos =
          (from o in DataContext.Entity<Order> (new TestQueryListener ())
          select o.Customer.Ceo).Distinct();

      CheckQueryResult (ceos); // TODO
    }

    [Test]
    public void QueryWithWhereAndImplicitJoin ()
    {
      var orders =
          from o in DataContext.Entity<Order> (new TestQueryListener ())
          where o.Customer.Type == Customer.CustomerType.Gold
          select o;

      CheckQueryResult (orders, DomainObjectIDs.InvalidOrder, DomainObjectIDs.Order3, DomainObjectIDs.Order2, DomainObjectIDs.Order4);
    }

    private void CheckQueryResult<T> (IQueryable<T> query, params ObjectID[] expectedObjectIDs)
    where T : DomainObject
    {
      T[] results = query.ToArray ();
      T[] expected = GetExpectedObjects<T> (expectedObjectIDs);
      Assert.That (results, Is.EquivalentTo (expected));
    }

    private T[] GetExpectedObjects<T> (params ObjectID[] expectedObjectIDs)
    where T : DomainObject
    {
      return (from id in expectedObjectIDs select (T) DomainObject.GetObject (id)).ToArray();
    }
  }
}
using System.Linq;
using NUnit.Framework;
using Remotion.Data.DomainObjects.UnitTests;
using Remotion.Data.DomainObjects.UnitTests.TestDomain;
using NUnit.Framework.SyntaxHelpers;

namespace Remotion.Data.DomainObjects.Linq.UnitTests
{
  [TestFixture]
  public class IntegrationTests : ClientTransactionBaseTest
  {
    [Test]
    public void SimpleQuery ()
    {
      var computers =
          from c in DataContext.Entity<Computer> (new TestQueryListener())
          select c;
      CheckQueryResult (computers, DomainObjectIDs.Computer1, DomainObjectIDs.Computer2, DomainObjectIDs.Computer3, DomainObjectIDs.Computer4,
          DomainObjectIDs.Computer5);
    }

    [Test]
    public void SimpleQuery_WithRelatedEntity ()
    {
      var query =
          from o in DataContext.Entity<OrderTicket> (new TestQueryListener())
          select o.Order;
      CheckQueryResult (query, DomainObjectIDs.Order1, DomainObjectIDs.Order2, DomainObjectIDs.Order3, DomainObjectIDs.Order4,
          DomainObjectIDs.OrderWithoutOrderItem);
    }


    [Test]
    public void QueryWithWhereConditions ()
    {
      var computers =
          from c in DataContext.Entity<Computer> (new TestQueryListener())
          where c.SerialNumber == "93756-ndf-23" || c.SerialNumber == "98678-abc-43"
          select c;

      CheckQueryResult (computers, DomainObjectIDs.Computer2, DomainObjectIDs.Computer5);
    }

    [Test]
    public void QueryWithWhereConditionsAndNull ()
    {
      var computers =
          from c in DataContext.Entity<Computer> (new TestQueryListener())
          where c.Employee != null
          select c;

      CheckQueryResult (computers, DomainObjectIDs.Computer1, DomainObjectIDs.Computer2, DomainObjectIDs.Computer3);
    }

    [Test]
    public void QueryWithWhereConditionAndStartsWith ()
    {
      var computers =
          from c in DataContext.Entity<Computer> (new TestQueryListener())
          where c.SerialNumber.StartsWith ("9")
          select c;

      CheckQueryResult (computers, DomainObjectIDs.Computer2, DomainObjectIDs.Computer5);
    }

    [Test]
    public void QueryWithWhereConditionAndEndsWith ()
    {
      var computers =
          from c in DataContext.Entity<Computer> (new TestQueryListener())
          where c.SerialNumber.EndsWith ("7")
          select c;

      CheckQueryResult (computers, DomainObjectIDs.Computer3);
    }

    [Test]
    public void QueryWithWhere_OuterObject ()
    {
      Employee employee = Employee.GetObject (DomainObjectIDs.Employee1);
      var employees =
          from e in DataContext.Entity<Employee> (new TestQueryListener())
          where e == employee
          select e;

      CheckQueryResult (employees, DomainObjectIDs.Employee1);
    }

    [Test]
    public void QueryWithWhereConditionAndGreaterThan ()
    {
      var orders =
          from o in DataContext.Entity<Order> (new TestQueryListener())
          where o.OrderNumber <= 3
          select o;

      CheckQueryResult (orders, DomainObjectIDs.OrderWithoutOrderItem, DomainObjectIDs.Order2, DomainObjectIDs.Order1);
    }


    [Test]
    public void QueryWithVirtualKeySide_EqualsNull ()
    {
      var employees =
          from e in DataContext.Entity<Employee> (new TestQueryListener())
          where e.Computer == null
          select e;

      CheckQueryResult (employees, DomainObjectIDs.Employee1, DomainObjectIDs.Employee2, DomainObjectIDs.Employee6, DomainObjectIDs.Employee7);
    }

    [Test]
    public void QueryWithVirtualKeySide_NotEqualsNull ()
    {
      var employees =
          from e in DataContext.Entity<Employee> (new TestQueryListener())
          where e.Computer != null
          select e;

      CheckQueryResult (employees, DomainObjectIDs.Employee3, DomainObjectIDs.Employee4, DomainObjectIDs.Employee5);
    }

    [Test]
    public void QueryWithVirtualKeySide_EqualsOuterObject ()
    {
      Computer computer = Computer.GetObject (DomainObjectIDs.Computer1);
      var employees =
          from e in DataContext.Entity<Employee> (new TestQueryListener())
          where e.Computer == computer
          select e;

      CheckQueryResult (employees, DomainObjectIDs.Employee3);
    }

    [Test]
    public void QueryWithVirtualKeySide_NotEqualsOuterObject ()
    {
      Computer computer = Computer.GetObject (DomainObjectIDs.Computer1);
      var employees =
          from e in DataContext.Entity<Employee> (new TestQueryListener())
          where e.Computer != computer
          select e;

      CheckQueryResult (employees, DomainObjectIDs.Employee1, DomainObjectIDs.Employee2, DomainObjectIDs.Employee4, DomainObjectIDs.Employee5,
          DomainObjectIDs.Employee6, DomainObjectIDs.Employee7);
    }

    [Test]
    public void QueryWithOuterEntityInCondition ()
    {
      Employee employee = Employee.GetObject (DomainObjectIDs.Employee3);
      var computers =
          from c in DataContext.Entity<Computer> (new TestQueryListener())
          where c.Employee == employee
          select c;

      CheckQueryResult (computers, DomainObjectIDs.Computer1);
    }

    [Test]
    public void QueryWithIDInCondition ()
    {
      Employee employee = Employee.GetObject (DomainObjectIDs.Employee3);
      var computers =
          from c in DataContext.Entity<Computer> (new TestQueryListener())
          where c.Employee.ID == employee.ID
          select c;

      CheckQueryResult (computers, DomainObjectIDs.Computer1);
    }

    [Test]
    public void QueryWithSimpleOrderBy ()
    {
      var query =
          from o in DataContext.Entity<Order> (new TestQueryListener())
          orderby o.OrderNumber
          select o;
      CheckQueryResult (query, DomainObjectIDs.Order1, DomainObjectIDs.OrderWithoutOrderItem, DomainObjectIDs.Order2, DomainObjectIDs.Order3,
          DomainObjectIDs.Order4, DomainObjectIDs.InvalidOrder);
    }

    [Test]
    public void QueryWithOrderByAndImplicitJoin ()
    {
      var orders =
          from o in DataContext.Entity<Order> (new TestQueryListener())
          where o.OrderNumber <= 4
          orderby o.Customer.Name
          select o;

      Order[] expected =
          GetExpectedObjects<Order> (DomainObjectIDs.OrderWithoutOrderItem, DomainObjectIDs.Order1, DomainObjectIDs.Order2, DomainObjectIDs.Order3);
      Assert.That (orders.ToArray(), Is.EqualTo (expected));
    }

    [Test]
    public void QueryWithSelectAndImplicitJoin_VirtualSide ()
    {
      var ceos =
          (from o in DataContext.Entity<Order> (new TestQueryListener())
          where o.Customer.Ceo != null
          select o.Customer.Ceo).Distinct();

      CheckQueryResult (ceos, DomainObjectIDs.Ceo12, DomainObjectIDs.Ceo5, DomainObjectIDs.Ceo3);
    }

    [Test]
    public void QueryWithSelectAndImplicitJoin ()
    {
      var ceos =
          from o in DataContext.Entity<Order> (new TestQueryListener())
          where o.Customer.Ceo.Name == "Hugo Boss"
          select o.Customer.Ceo;

      CheckQueryResult (ceos, DomainObjectIDs.Ceo5);
    }

    [Test]
    public void QueryWithSelectAndImplicitJoin_UsingJoinPartTwice ()
    {
      var ceos =
          from o in DataContext.Entity<Order> (new TestQueryListener())
          where o.Customer.Name == "Kunde 3"
          select o.Customer.Ceo;

      CheckQueryResult (ceos, DomainObjectIDs.Ceo5);
    }

    [Test]
    public void QueryWithDistinct ()
    {
      var ceos =
          (from o in DataContext.Entity<Order> (new TestQueryListener())
          where o.Customer.Ceo != null
          select o.Customer.Ceo).Distinct();

      CheckQueryResult (ceos, DomainObjectIDs.Ceo12, DomainObjectIDs.Ceo5, DomainObjectIDs.Ceo3);
    }

    [Test]
    public void QueryWithWhereAndImplicitJoin ()
    {
      var orders =
          from o in DataContext.Entity<Order> (new TestQueryListener())
          where o.Customer.Type == Customer.CustomerType.Gold
          select o;

      CheckQueryResult (orders, DomainObjectIDs.InvalidOrder, DomainObjectIDs.Order3, DomainObjectIDs.Order2, DomainObjectIDs.Order4);
    }

    [Test]
    public void QueryWithSubQueryAndWhereInAdditionalFrom()
    {
      var orders =
          from o in DataContext.Entity<Order> (new TestQueryListener())
          from o2 in
            (from oi in DataContext.Entity<OrderItem> (new TestQueryListener ()) where oi.Order == o select oi)
          select o2;

      CheckQueryResult (orders, DomainObjectIDs.OrderItem5, DomainObjectIDs.OrderItem4, DomainObjectIDs.OrderItem2, DomainObjectIDs.OrderItem1, 
        DomainObjectIDs.OrderItem3);
    }

    [Test]
    public void QueryWithSubQueryInWhere ()
    {
      var orders =
          from o in DataContext.Entity<Order> (new TestQueryListener())
          where (from c in DataContext.Entity<Customer>() select c).Contains (o.Customer)
          select o;

      CheckQueryResult (orders, DomainObjectIDs.Order1, DomainObjectIDs.Order2, DomainObjectIDs.Order3, DomainObjectIDs.Order4, 
        DomainObjectIDs.OrderWithoutOrderItem, DomainObjectIDs.InvalidOrder);
    }

    [Test]
    public void QueryWithSubQueryAndJoinInWhere ()
    {
      var orders =
          from o in DataContext.Entity<Order> (new TestQueryListener ())
          where (from c in DataContext.Entity<OrderTicket> () select c.Order).Contains (o)
          select o;

      CheckQueryResult (orders, DomainObjectIDs.Order1, DomainObjectIDs.Order2, DomainObjectIDs.Order3, DomainObjectIDs.Order4, 
          DomainObjectIDs.OrderWithoutOrderItem);
    }

    [Test]
    public void QueryWithLet ()
    {
      var orders = from o in DataContext.Entity<Order> (new TestQueryListener ())
                   let x = o
                   select x;

      CheckQueryResult (orders, DomainObjectIDs.Order1, DomainObjectIDs.Order2, DomainObjectIDs.Order3, DomainObjectIDs.Order4,DomainObjectIDs.Order4,
          DomainObjectIDs.InvalidOrder,DomainObjectIDs.OrderWithoutOrderItem);
    }

    [Test]
    public void QueryWithLet_LetWithColumn ()
    {
      var orders = from o in DataContext.Entity<Order> (new TestQueryListener ())
                   let y = o.OrderNumber
                   where y > 1
                   select o;

      CheckQueryResult (orders,
        DomainObjectIDs.InvalidOrder, DomainObjectIDs.Order3, DomainObjectIDs.OrderWithoutOrderItem, DomainObjectIDs.Order2, 
        DomainObjectIDs.Order4);
    }

    [Test]
    public void QueryWithLet_LetWithColumn2()
    {
      var orders = from o in DataContext.Entity<Order>(new TestQueryListener())
                   let x = o.Customer.Name
                   where x == "Kunde 1"
                   select o;
      CheckQueryResult (orders, DomainObjectIDs.OrderWithoutOrderItem, DomainObjectIDs.Order1);
    }

    [Test]
    public void QueryWithSeveralJoinsAndCrossApply ()
    {
      var ceos = from o in DataContext.Entity<Order>(new TestQueryListener())
                 let x = o.Customer.Ceo
                 where x.Name == "Hugo Boss"
                 select x;

      CheckQueryResult(ceos,DomainObjectIDs.Ceo5);
    }

    [Test]
    public void QueryWithLet_SeveralCrossApplies()
    {
      var orders = from o in DataContext.Entity<Order>(new TestQueryListener())
                   let x = o
                   let y = o.Customer
                   select x;

      CheckQueryResult (orders, DomainObjectIDs.Order1, DomainObjectIDs.Order2, DomainObjectIDs.Order3, DomainObjectIDs.Order4, DomainObjectIDs.Order4,
          DomainObjectIDs.InvalidOrder, DomainObjectIDs.OrderWithoutOrderItem);
    }
    
    private void CheckQueryResult<T> (IQueryable<T> query, params ObjectID[] expectedObjectIDs)
        where T: TestDomainBase
    {
      T[] results = query.ToArray();
      T[] expected = GetExpectedObjects<T> (expectedObjectIDs);
      Assert.That (results, Is.EquivalentTo (expected));
    }

    private T[] GetExpectedObjects<T> (params ObjectID[] expectedObjectIDs)
        where T: TestDomainBase
    {
      return (from id in expectedObjectIDs select (id == null ? null : (T) TestDomainBase.GetObject (id))).ToArray();
    }
  }
}
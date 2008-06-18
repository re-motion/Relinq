using System;
using System.Collections;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Collections;
using Remotion.Data.DomainObjects.Mapping;
using Remotion.Data.DomainObjects.Queries;
using Remotion.Data.DomainObjects.UnitTests;
using Remotion.Data.DomainObjects.UnitTests.TestDomain;
using Remotion.Data.Linq;
using System.Linq;
using Remotion.Data.Linq.Parsing.Structure;
using Remotion.Data.Linq.SqlGeneration;
using Remotion.Data.Linq.SqlGeneration.SqlServer;
using Remotion.Mixins;

namespace Remotion.Data.DomainObjects.Linq.UnitTests
{
  [TestFixture]
  public class QueryExecutorTest : ClientTransactionBaseTest
  {
    private TestQueryListener _listener;
    private SqlGeneratorBase _sqlGenerator;

    public override void SetUp ()
    {
      base.SetUp ();
      _listener = new TestQueryListener ();
      _sqlGenerator = new SqlServerGenerator (DatabaseInfo.Instance);
    }

    [Test]
    public void QueryExecutor_Listener ()
    {
      QueryExecutor<Computer> executor = new QueryExecutor<Computer> (_listener, _sqlGenerator);
      Assert.AreSame (_listener, executor.Listener);
    }

    [Test]
    public void ExecuteSingle()
    {
      SetDatabaseModifyable();

      Computer.GetObject (DomainObjectIDs.Computer1).Delete ();
      Computer.GetObject (DomainObjectIDs.Computer2).Delete ();
      Computer.GetObject (DomainObjectIDs.Computer3).Delete ();
      Computer.GetObject (DomainObjectIDs.Computer4).Delete ();

      ClientTransaction.Current.Commit();

      QueryExecutor<Computer> executor = new QueryExecutor<Computer> (_listener, _sqlGenerator);
      QueryModel model = GetParsedSimpleQuery ();

      object instance = executor.ExecuteSingle (model);
      Assert.IsNotNull (instance);
      Assert.AreSame (Computer.GetObject (DomainObjectIDs.Computer5), instance);
    }

    [Test]
    [ExpectedException (ExpectedMessage = "ExecuteSingle must return a single object, but the query returned 5 objects.")]
    public void ExecuteSingle_TooManyObjects()
    {
      QueryExecutor<Computer> executor = new QueryExecutor<Computer> (_listener, _sqlGenerator);
      executor.ExecuteSingle (GetParsedSimpleQuery());
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "No ClientTransaction has been associated with the current thread.")]
    public void QueryExecutor_ExecuteSingle_NoCurrentTransaction ()
    {
      QueryExecutor<Computer> executor = new QueryExecutor<Computer> (null, _sqlGenerator);
      QueryModel model = GetParsedSimpleQuery ();

      using (ClientTransactionScope.EnterNullScope ())
      {
        executor.ExecuteSingle (model);
      }
    }

    [Test]
    public void ExecuteCollection ()
    {
      QueryExecutor<Computer> executor = new QueryExecutor<Computer> (_listener, _sqlGenerator);
      QueryModel model = GetParsedSimpleQuery();

      IEnumerable computers = executor.ExecuteCollection (model);

      ArrayList computerList = new ArrayList();
      foreach (Computer computer in computers)
        computerList.Add (computer);

      Computer[] expected = new Computer[]
          {
              Computer.GetObject (DomainObjectIDs.Computer1),
              Computer.GetObject (DomainObjectIDs.Computer2),
              Computer.GetObject (DomainObjectIDs.Computer3),
              Computer.GetObject (DomainObjectIDs.Computer4),
              Computer.GetObject (DomainObjectIDs.Computer5),
          };
      Assert.That (computerList, Is.EquivalentTo (expected));
    }

    [Test]
    [ExpectedException (typeof (MappingException), ExpectedMessage = "Mapping does not contain class 'System.String'.")]
    public void ExecuteCollection_WrongType ()
    {
      QueryExecutor<string> executor = new QueryExecutor<string> (_listener, _sqlGenerator);
      executor.ExecuteCollection (GetParsedSimpleQuery());
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "No ClientTransaction has been associated with the current thread.")]
    public void QueryExecutor_ExecuteCollection_NoCurrentTransaction ()
    {
      QueryExecutor<Computer> executor = new QueryExecutor<Computer> (null, _sqlGenerator);
      QueryModel model = GetParsedSimpleQuery ();

      using (ClientTransactionScope.EnterNullScope ())
      {
        executor.ExecuteCollection (model);
      }
    }

    [Test]
    public void ExecuteSingle_WithParameters ()
    {
      QueryExecutor<Order> executor = new QueryExecutor<Order> (_listener, _sqlGenerator);
      QueryModel model = GetParsedSimpleWhereQuery ();

      Order order = (Order) executor.ExecuteSingle (model);

      Order expected = Order.GetObject (DomainObjectIDs.Order1);
      Assert.AreSame (expected, order);
    }

    [Test]
    public void ExecuteCollection_WithParameters()
    {
      QueryExecutor<Order> executor = new QueryExecutor<Order> (_listener, _sqlGenerator);
      QueryModel model = GetParsedSimpleWhereQuery ();

      IEnumerable orders = executor.ExecuteCollection (model);

      ArrayList orderList = new ArrayList ();
      foreach (Order order in orders) 
        orderList.Add (order);

      Order[] expected = new Order[]
          {
              Order.GetObject (DomainObjectIDs.Order1),

          };
      Assert.That (orderList, Is.EquivalentTo (expected));
    }

    public class TestMixin : Mixin<object, TestMixin.IBaseCallRequirements>
    {
      public interface IBaseCallRequirements
      {
        ClassDefinition GetClassDefinition ();
        Query CreateQuery (ClassDefinition classDefinition, string statement, CommandParameter[] commandParameters);
        Tuple<string, CommandParameter[]> CreateStatement (QueryModel queryModel);
      }

      public bool GetClassDefinitionCalled = false;
      public bool CreateQueryCalled = false;
      public bool GetStatementCalled = false;

      [OverrideTarget]
      public ClassDefinition GetClassDefinition ()
      {
        GetClassDefinitionCalled = true;
        return Base.GetClassDefinition();
      }

      [OverrideTarget]
      public Query CreateQuery (ClassDefinition classDefinition, string statement, CommandParameter[] commandParameters)
      {
        CreateQueryCalled = true;
        return Base.CreateQuery (classDefinition, statement, commandParameters);
      }

      [OverrideTarget]
      public Tuple<string, CommandParameter[]> CreateStatement (QueryModel queryModel)
      {
        GetStatementCalled = true;
        return Base.CreateStatement (queryModel);
      }
    }
    
    [Test]
    public void QueryExecutor_CanBeMixed ()
    {
      using (MixinConfiguration.BuildNew ().ForClass (typeof (QueryExecutor<>)).AddMixin<TestMixin> ().EnterScope ())
      {
        DomainObjectQueryable<Order> queryable = new DomainObjectQueryable<Order> (null, new SqlServerGenerator (DatabaseInfo.Instance));
        Assert.That (Mixin.Get<QueryExecutorTest.TestMixin> (((QueryProvider)queryable.Provider).Executor), Is.Not.Null);
      }
    }

    [Test]
    public void GetClassDefinition_CanBeMixed ()
    {
      using (MixinConfiguration.BuildNew ().ForClass (typeof (QueryExecutor<>)).AddMixin<TestMixin> ().EnterScope ())
      {
        DomainObjectQueryable<Order> queryable = new DomainObjectQueryable<Order> (null, new SqlServerGenerator (DatabaseInfo.Instance));
        QueryExecutor<Order> executor = (QueryExecutor<Order>) ((QueryProvider) queryable.Provider).Executor;

        executor.GetClassDefinition();
        Assert.That (Mixin.Get<TestMixin> (executor).GetClassDefinitionCalled, Is.True);
      }
    }

    [Test]
    public void GetStatement_CanBeMixed ()
    {
      using (MixinConfiguration.BuildNew ().ForClass (typeof (QueryExecutor<>)).AddMixin<TestMixin> ().EnterScope ())
      {
        DomainObjectQueryable<Computer> queryable = new DomainObjectQueryable<Computer> (null, new SqlServerGenerator (DatabaseInfo.Instance));
        QueryExecutor<Computer> executor = (QueryExecutor<Computer>) ((QueryProvider) queryable.Provider).Executor;

        executor.CreateStatement (GetParsedSimpleQuery ());
        Assert.That (Mixin.Get<TestMixin> (executor).GetStatementCalled, Is.True);
      }
    }

    [Test]
    public void CreateQuery_CanBeMixed ()
    {
      using (MixinConfiguration.BuildNew ().ForClass (typeof (QueryExecutor<>)).AddMixin<TestMixin> ().EnterScope ())
      {
        DomainObjectQueryable<Order> queryable = new DomainObjectQueryable<Order> (null, new SqlServerGenerator (DatabaseInfo.Instance));
        QueryExecutor<Order> executor = (QueryExecutor<Order>) ((QueryProvider) queryable.Provider).Executor;

        ClassDefinition classDefinition = executor.GetClassDefinition();
        Tuple<string, CommandParameter[]> statement = executor.CreateStatement(GetParsedSimpleQuery());

        executor.CreateQuery (classDefinition, statement.A, statement.B);
        Assert.That (Mixin.Get<TestMixin> (executor).CreateQueryCalled, Is.True);
      }
    }

    private QueryModel GetParsedSimpleQuery ()
    {
      IQueryable<Computer> query = from computer in DataContext.Entity<Computer> () select computer;
      return new QueryParser (query.Expression).GetParsedQuery ();
    }

    private QueryModel GetParsedSimpleWhereQuery ()
    {
      IQueryable<Order> query = from order in DataContext.Entity<Order> () where order.OrderNumber == 1 select order;
      return new QueryParser (query.Expression).GetParsedQuery ();
    }

  }
}
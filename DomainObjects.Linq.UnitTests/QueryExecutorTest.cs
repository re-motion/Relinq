using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Rubicon.Data.DomainObjects.Mapping;
using Rubicon.Data.DomainObjects.UnitTests;
using Rubicon.Data.DomainObjects.UnitTests.TestDomain;
using Rubicon.Data.Linq;
using System.Linq;
using Rubicon.Data.Linq.Parsing;

namespace Rubicon.Data.DomainObjects.Linq.UnitTests
{
  [TestFixture]
  public class QueryExecutorTest : ClientTransactionBaseTest
  {
    [Test]
    public void ExecuteSingle()
    {
      SetDatabaseModifyable();

      Computer.GetObject (DomainObjectIDs.Computer1).Delete ();
      Computer.GetObject (DomainObjectIDs.Computer2).Delete ();
      Computer.GetObject (DomainObjectIDs.Computer3).Delete ();
      Computer.GetObject (DomainObjectIDs.Computer4).Delete ();

      ClientTransaction.Current.Commit();
      
      QueryExecutor<Computer> executor = new QueryExecutor<Computer> ();
      QueryExpression expression = GetParsedQuery ();

      object instance = executor.ExecuteSingle (expression);
      Assert.IsNotNull (instance);
      Assert.AreSame (Computer.GetObject (DomainObjectIDs.Computer5), instance);
    }

    [Test]
    [ExpectedException (ExpectedMessage = "ExecuteSingle must return a single object, but the query returned 5 objects.")]
    public void ExecuteSingle_TooManyObjects()
    {
      QueryExecutor<Computer> executor = new QueryExecutor<Computer> ();
      executor.ExecuteSingle (GetParsedQuery());
    }

    [Test]
    public void ExecuteCollection ()
    {
      QueryExecutor<Computer> executor = new QueryExecutor<Computer> ();
      QueryExpression expression = GetParsedQuery();

      IEnumerable computers = executor.ExecuteCollection (expression);

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
      QueryExecutor<string> executor = new QueryExecutor<string> ();
      executor.ExecuteCollection (GetParsedQuery());
    }

    private QueryExpression GetParsedQuery ()
    {
      IQueryable<Computer> query = from computer in DataContext.Entity<Computer> () select computer;
      return new QueryParser (query.Expression).GetParsedQuery ();
    }

  }
}
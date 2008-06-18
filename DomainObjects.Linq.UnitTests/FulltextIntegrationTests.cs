using System.Linq;
using NUnit.Framework;
using Remotion.Data.DomainObjects.Mapping;
using Remotion.Data.DomainObjects.Queries;
using Remotion.Data.DomainObjects.Queries.Configuration;
using Remotion.Data.DomainObjects.UnitTests;
using Remotion.Data.DomainObjects.UnitTests.TestDomain;
using Remotion.Data.Linq.ExtensionMethods;

namespace Remotion.Data.DomainObjects.Linq.UnitTests
{
  [TestFixture]
  public class FulltextIntegrationTests : ClientTransactionBaseTest
  {
    public override void TestFixtureSetUp ()
    {
      base.TestFixtureSetUp ();

      SetDatabaseModifyable ();
      DatabaseAgent.ExecuteBatch ("DataDomainObjects_CreateFulltextIndices.sql", false);
      WaitForIndices ();
    }

    [Test]
    public void Fulltext_Spike ()
    {
      ClassDefinition orderClassDefinition = MappingConfiguration.Current.ClassDefinitions.GetMandatory (typeof (Order));
      QueryDefinition queryDefinition =
          new QueryDefinition ("bla", orderClassDefinition.StorageProviderID, "SELECT * FROM Ceo WHERE Contains ([Ceo].[Name], 'Fischer')", QueryType.Collection);
      Query query = new Query (queryDefinition);

      var orders = ClientTransactionMock.QueryManager.GetCollection<Ceo> (query).Cast<Ceo> ();
      IntegrationTests.CheckQueryResult (orders, DomainObjectIDs.Ceo4);
    }

    [Test]
    public void QueryWithContainsFullText ()
    {
      var ceos = from c in DataContext.Entity<Ceo> (new TestQueryListener ())
                      where c.Name.ContainsFulltext ("Fischer")
                      select c;
      IntegrationTests.CheckQueryResult (ceos, DomainObjectIDs.Ceo4);
    }

    private void WaitForIndices ()
    {
      var rowCount = DatabaseAgent.ExecuteScalarCommand ("SELECT COUNT(*) FROM Ceo WHERE Contains ([Ceo].[Name], 'Fischer')");
      if (!rowCount.Equals (1))
        WaitForIndices ();
    }

  }
}
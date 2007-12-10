using System.Collections;
using Rubicon.Data.DomainObjects.Mapping;
using Rubicon.Data.DomainObjects.Queries;
using Rubicon.Data.DomainObjects.Queries.Configuration;
using Rubicon.Data.Linq;
using System;

namespace Rubicon.Data.DomainObjects.Linq
{
  public class QueryExecutor<T> : IQueryExecutor
  {
    public object ExecuteSingle (QueryExpression queryExpression)
    {
      IEnumerable results = ExecuteCollection (queryExpression);
      ArrayList resultList = new ArrayList();
      foreach (object o in results)
        resultList.Add (o);
      if (resultList.Count == 1)
        return resultList[0];
      else
      {
        string message = string.Format ("ExecuteSingle must return a single object, but the query returned {0} objects.", resultList.Count);
        throw new InvalidOperationException (message);
      }
    }

    public IEnumerable ExecuteCollection (QueryExpression queryExpression)
    {
      ClassDefinition classDefinition = MappingConfiguration.Current.ClassDefinitions.GetMandatory (typeof (T));
      string statement = new SqlGenerator (queryExpression).GetCommandString (DatabaseInfo.Instance);
      QueryDefinition queryDefinition = new QueryDefinition ("<dynamic query>", classDefinition.StorageProviderID, statement,
          QueryType.Collection);
      Query query = new Query (queryDefinition);
      return ClientTransaction.Current.QueryManager.GetCollection (query);
    }
  }
}
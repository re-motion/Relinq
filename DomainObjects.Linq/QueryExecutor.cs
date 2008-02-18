using System.Collections;
using Rubicon.Collections;
using Rubicon.Data.DomainObjects.Mapping;
using Rubicon.Data.DomainObjects.Queries;
using Rubicon.Data.DomainObjects.Queries.Configuration;
using Rubicon.Data.Linq;
using System;
using Rubicon.Data.Linq.SqlGeneration;
using Rubicon.Data.Linq.SqlGeneration.SqlServer;

namespace Rubicon.Data.DomainObjects.Linq
{
  public class QueryExecutor<T> : IQueryExecutor
  {
    public QueryExecutor (IQueryListener listener)
    {
      Listener = listener;
    }

    public IQueryListener Listener { get; private set; }

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
      if (ClientTransaction.Current == null)
        throw new InvalidOperationException ("No ClientTransaction has been associated with the current thread.");

      ClassDefinition classDefinition = MappingConfiguration.Current.ClassDefinitions.GetMandatory (typeof (T));
      SqlServerGenerator sqlGenerator = new SqlServerGenerator (queryExpression, DatabaseInfo.Instance);
      
      Tuple<string, CommandParameter[]> result = sqlGenerator.BuildCommandString();
      string statement = result.A;
      CommandParameter[] commandParameters = result.B;

      QueryParameterCollection queryParameters = new QueryParameterCollection();
      foreach (CommandParameter commandParameter in commandParameters)
        queryParameters.Add (commandParameter.Name, commandParameter.Value, QueryParameterType.Value);

      QueryDefinition queryDefinition = new QueryDefinition ("<dynamic query>", classDefinition.StorageProviderID, statement, QueryType.Collection);
      Query query = new Query (queryDefinition,queryParameters);

      if (Listener != null)
        Listener.QueryConstructed (query);

      return ClientTransaction.Current.QueryManager.GetCollection (query);
    }
  }
}
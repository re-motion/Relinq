using System.Collections;
using Remotion.Data.DomainObjects.Mapping;
using Remotion.Data.Linq;
using Remotion.Data.Linq.SqlGeneration.SqlServer;
using Remotion.Collections;
using Remotion.Data.DomainObjects.Queries;
using Remotion.Data.DomainObjects.Queries.Configuration;
using System;
using Remotion.Data.Linq.SqlGeneration;

namespace Remotion.Data.DomainObjects.Linq
{
  public class QueryExecutor<T> : IQueryExecutor
  {
    public QueryExecutor (IQueryListener listener, SqlGeneratorBase sqlGenerator)
    {
      Listener = listener;
      SqlGenerator = sqlGenerator;
    }

    public IQueryListener Listener { get; private set; }
    public SqlGeneratorBase SqlGenerator { get; private set; }

    public object ExecuteSingle (QueryModel queryModel)
    {
      IEnumerable results = ExecuteCollection (queryModel);
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

    public IEnumerable ExecuteCollection (QueryModel queryModel)
    {
      if (ClientTransaction.Current == null)
        throw new InvalidOperationException ("No ClientTransaction has been associated with the current thread.");

      ClassDefinition classDefinition = GetClassDefinition();
      //SqlServerGenerator sqlGenerator = new SqlServerGenerator (DatabaseInfo.Instance);

      Tuple<string, CommandParameter[]> result = GetStatement(queryModel);
      
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

    public virtual ClassDefinition GetClassDefinition()
    {
      return MappingConfiguration.Current.ClassDefinitions.GetMandatory (typeof (T));
    }

    public virtual Tuple<string, CommandParameter[]> GetStatement (QueryModel queryModel)
    {
      return SqlGenerator.BuildCommandString (queryModel);
    }
  }
}
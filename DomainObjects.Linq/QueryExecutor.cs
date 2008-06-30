using System.Collections;
using Remotion.Data.DomainObjects.Mapping;
using Remotion.Data.Linq;
using Remotion.Collections;
using Remotion.Data.DomainObjects.Queries;
using Remotion.Data.DomainObjects.Queries.Configuration;
using System;
using Remotion.Data.Linq.SqlGeneration;

namespace Remotion.Data.DomainObjects.Linq
{
  public class QueryExecutor<T> : IQueryExecutor
  {
    public QueryExecutor (ISqlGeneratorBase sqlGenerator)
    {
      SqlGenerator = sqlGenerator;
    }

    public ISqlGeneratorBase SqlGenerator { get; private set; }

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
      
      CommandData commandData = CreateStatement(queryModel);
      Query query = CreateQuery(classDefinition, commandData.Statement, commandData.Parameters);
      
      return ClientTransaction.Current.QueryManager.GetCollection (query);
    }

    public virtual Query CreateQuery(ClassDefinition classDefinition, string statement, CommandParameter[] commandParameters)
    {
      QueryParameterCollection queryParameters = new QueryParameterCollection();
      foreach (CommandParameter commandParameter in commandParameters)
        queryParameters.Add (commandParameter.Name, commandParameter.Value, QueryParameterType.Value);

      QueryDefinition queryDefinition = new QueryDefinition ("<dynamic query>", classDefinition.StorageProviderID, statement, QueryType.Collection);
      return new Query (queryDefinition,queryParameters);
    }

    public virtual ClassDefinition GetClassDefinition ()
    {
      return MappingConfiguration.Current.ClassDefinitions.GetMandatory (typeof (T));
    }

    public virtual CommandData CreateStatement (QueryModel queryModel)
    {
      return SqlGenerator.BuildCommand (queryModel);
    }
  }
}
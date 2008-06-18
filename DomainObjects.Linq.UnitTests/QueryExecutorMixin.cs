using System;
using Remotion.Data.DomainObjects.Mapping;
using Remotion.Data.DomainObjects.Queries;
using Remotion.Data.Linq.SqlGeneration;
using Remotion.Mixins;
using Remotion.Utilities;

namespace Remotion.Data.DomainObjects.Linq.UnitTests
{
  [Extends(typeof (QueryExecutor<>))]
  public class QueryExecutorMixin : Mixin<object, QueryExecutorMixin.IBaseCallRequirements>
  {
    public interface IBaseCallRequirements
    {
      Query CreateQuery (ClassDefinition classDefinition, string statement, CommandParameter[] commandParameters);
    }

    [OverrideTarget]
    public Query CreateQuery (ClassDefinition classDefinition, string statement, CommandParameter[] commandParameters)
    {
      Query query = Base.CreateQuery (classDefinition, statement, commandParameters);
      QueryConstructed(query);
      return query;
    }

    private static void QueryConstructed (Query query)
    {
      ArgumentUtility.CheckNotNull ("query", query);

      Console.WriteLine (query.Statement);
      foreach (QueryParameter parameter in query.Parameters)
        Console.WriteLine ("{0} = {1}", parameter.Name, parameter.Value);
    }
  }
}
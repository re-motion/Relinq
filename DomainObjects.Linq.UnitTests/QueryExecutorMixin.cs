using System;
using Remotion.Data.DomainObjects.Mapping;
using Remotion.Data.DomainObjects.Queries;
using Remotion.Data.Linq.SqlGeneration;
using Remotion.Mixins;

namespace Remotion.Data.DomainObjects.Linq.UnitTests
{
  public class QueryExecutorMixin : Mixin<object, QueryExecutorMixin.IBaseCallRequirements>
  {
    public interface IBaseCallRequirements
    {
      Query CreateQuery (ClassDefinition classDefinition, string statement, CommandParameter[] commandParameters);
    }

    [OverrideTarget]
    public void CreateQuery (ClassDefinition classDefinition, string statement, CommandParameter[] commandParameters)
    {
      QueryConstructed(Base.CreateQuery (classDefinition, statement, commandParameters));
    }

    public void QueryConstructed (Query query)
    {
      Console.WriteLine (query.Statement);
      foreach (QueryParameter parameter in query.Parameters)
        Console.WriteLine ("{0} = {1}", parameter.Name, parameter.Value);
    }
  }
}
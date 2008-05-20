using System;
using Remotion.Data.DomainObjects.Queries;

namespace Remotion.Data.DomainObjects.Linq.UnitTests
{
  public class TestQueryListener : IQueryListener
  {
    public void QueryConstructed (Query query)
    {
      Console.WriteLine (query.Statement);
      foreach (QueryParameter parameter in query.Parameters)
        Console.WriteLine ("{0} = {1}", parameter.Name, parameter.Value);
    }
  }
}
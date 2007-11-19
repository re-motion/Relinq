using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Rhino.Mocks;
using Rubicon.Data.DomainObjects.Linq.QueryProviderImplementation;
using Rubicon.Data.DomainObjects.Linq.UnitTests.Parsing;

namespace Rubicon.Data.DomainObjects.Linq.UnitTests.ParsingTest
{
  public static class TestQueryGenerator
  {
    public static IQueryable<Student> CreateSimpleQuery(IQueryable<Student> source)
    {
      return from s in source select s;
    }
  }
}
using System.Linq;
using System.Linq.Expressions;

namespace Remotion.Data.UnitTests.Linq.TestQueryGenerators
{
  public class MethodCallTestQueryGenerator
  {
    public static int CreateQueryWithCount (IQueryable<Student> source)
    {
      return (from s in source select s).Count();
    }
  }
}
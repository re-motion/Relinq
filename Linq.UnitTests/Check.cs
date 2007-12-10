using System.Linq;

namespace Rubicon.Data.DomainObjects.Linq.QueryProviderImplementation
{
  public class Check
  {
    public static void Main()
    {
      StandardQueryable<int> queryable = new StandardQueryable<int> (null, null);
      queryable.GetEnumerator ();
    }
  }
}
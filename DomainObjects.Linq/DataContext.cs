namespace Rubicon.Data.DomainObjects.Linq
{
  public static class DataContext
  {
    public static DomainObjectQueryable<T> Entity<T>() 
      where T:DomainObject
    {
      return new DomainObjectQueryable<T>();
    }
  }
}
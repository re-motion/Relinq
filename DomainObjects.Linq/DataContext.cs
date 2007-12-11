namespace Rubicon.Data.DomainObjects.Linq
{
  public static class DataContext
  {
    public static DomainObjectQueryable<T> Entity<T>() 
      where T:DomainObject
    {
      return Entity<T> (null);
    }

    public static DomainObjectQueryable<T> Entity<T> (IQueryListener listener)
      where T : DomainObject
    {
      return new DomainObjectQueryable<T> (listener);
    }
  }
}
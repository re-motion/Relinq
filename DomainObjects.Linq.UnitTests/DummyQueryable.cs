using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Remotion.Data.DomainObjects.Linq.UnitTests
{
  public class DummyQueryable<T> : IQueryable<T>
  {
    IEnumerator<T> IEnumerable<T>.GetEnumerator ()
    {
      throw new NotImplementedException();
    }

    public IEnumerator GetEnumerator ()
    {
      return ((IEnumerable<T>) this).GetEnumerator();
    }

    public Expression Expression
    {
      get { throw new NotImplementedException(); }
    }

    public Type ElementType
    {
      get { throw new NotImplementedException(); }
    }

    public IQueryProvider Provider
    {
      get { throw new NotImplementedException(); }
    }
  }
}
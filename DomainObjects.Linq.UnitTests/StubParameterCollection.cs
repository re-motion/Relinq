using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace Rubicon.Data.DomainObjects.Linq.UnitTests
{
  public class StubParameterCollection : DbParameterCollection
  {
    private List<object> _parameters = new List<object> ();

    public override int Add (object value)
    {
      throw new NotImplementedException ();
    }

    public override void AddRange (Array values)
    {
      throw new NotImplementedException();
    }

    public override bool Contains (object value)
    {
      throw new NotImplementedException();
    }

    public override bool Contains (string value)
    {
      throw new NotImplementedException();
    }

    public override void CopyTo (Array array, int index)
    {
      throw new NotImplementedException();
    }

    public override void Clear ()
    {
      throw new NotImplementedException();
    }

    public override IEnumerator GetEnumerator ()
    {
      throw new NotImplementedException();
    }

    protected override DbParameter GetParameter (int index)
    {
      throw new NotImplementedException();
    }

    protected override DbParameter GetParameter (string parameterName)
    {
      throw new NotImplementedException();
    }

    public override int IndexOf (object value)
    {
      throw new NotImplementedException();
    }

    public override int IndexOf (string parameterName)
    {
      throw new NotImplementedException();
    }

    public override void Insert (int index, object value)
    {
      throw new NotImplementedException();
    }

    public override void Remove (object value)
    {
      throw new NotImplementedException();
    }

    public override void RemoveAt (int index)
    {
      throw new NotImplementedException();
    }

    public override void RemoveAt (string parameterName)
    {
      throw new NotImplementedException();
    }

    protected override void SetParameter (int index, DbParameter value)
    {
      throw new NotImplementedException();
    }

    protected override void SetParameter (string parameterName, DbParameter value)
    {
      throw new NotImplementedException();
    }

    public override int Count
    {
      get { return 0; }
    }

    public override bool IsFixedSize
    {
      get { throw new NotImplementedException(); }
    }

    public override bool IsReadOnly
    {
      get { throw new NotImplementedException(); }
    }

    public override bool IsSynchronized
    {
      get { throw new NotImplementedException(); }
    }

    public override object SyncRoot
    {
      get { throw new NotImplementedException(); }
    }
  }
}
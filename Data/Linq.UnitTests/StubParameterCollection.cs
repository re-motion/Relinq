/* Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh
 *
 * This program is free software: you can redistribute it and/or modify it under 
 * the terms of the re:motion license agreement in license.txt. If you did not 
 * receive it, please visit http://www.re-motion.org/licensing.
 * 
 * Unless otherwise provided, this software is distributed on an "AS IS" basis, 
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Rhino.Mocks;

namespace Remotion.Data.Linq.UnitTests
{
  public class StubParameterCollection : DbParameterCollection
  {
    private List<object> _parameters = new List<object> ();

    public override int Add (object value)
    {
      _parameters.Add (value);
      return _parameters.Count - 1;
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
      MockRepository repository = new MockRepository();
      DbParameter parameterStub = repository.Stub<DbParameter>();
      parameterStub.Value = _parameters[index];
      return parameterStub;
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
      get { return _parameters.Count; }
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

// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (c) rubicon IT GmbH, www.rubicon.eu
// 
// re-linq is free software; you can redistribute it and/or modify it under 
// the terms of the GNU Lesser General Public License as published by the 
// Free Software Foundation; either version 2.1 of the License, 
// or (at your option) any later version.
// 
// re-linq is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-linq; if not, see http://www.gnu.org/licenses.
// 

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Remotion.Linq.UnitTests.TestDomain
{
// ReSharper disable UnusedTypeParameter
  public class QueryableWithTooManyArguments<T1, T2> : IQueryable<T1>
// ReSharper restore UnusedTypeParameter
  {
    public IEnumerator<T1> GetEnumerator ()
    {
      throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator ()
    {
      return GetEnumerator();
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

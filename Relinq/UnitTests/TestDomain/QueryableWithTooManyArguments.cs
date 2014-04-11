// Copyright (c) rubicon IT GmbH, www.rubicon.eu
//
// See the NOTICE file distributed with this work for additional information
// regarding copyright ownership.  rubicon licenses this file to you under 
// the Apache License, Version 2.0 (the "License"); you may not use this 
// file except in compliance with the License.  You may obtain a copy of the 
// License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, WITHOUT 
// WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.  See the 
// License for the specific language governing permissions and limitations
// under the License.
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

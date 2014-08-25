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
using System.Collections.Generic;

namespace Remotion.Linq.Development.UnitTesting
{
  public class StubQueryExecutor : IQueryExecutor
  {
    public T ExecuteScalar<T> (QueryModel queryModel)
    {
      throw new NotImplementedException ("ExecuteScalar<" + typeof (T).Name + "> (" + queryModel + ")");
    }

    public T ExecuteSingle<T> (QueryModel queryModel, bool returnDefaultWhenEmpty)
    {
      throw new NotImplementedException ("ExecuteSingle<" + typeof (T).Name + "> (" + queryModel + ", " + returnDefaultWhenEmpty + ")");
    }

    public IEnumerable<T> ExecuteCollection<T> (QueryModel queryModel)
    {
      throw new NotImplementedException ("ExecuteCollection<" + typeof (T).Name + "> (" + queryModel + ")");
    }
  }
}
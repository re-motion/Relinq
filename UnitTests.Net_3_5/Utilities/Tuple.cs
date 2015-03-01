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

// ReSharper disable once CheckNamespace
namespace System
{
  public static class Tuple
  {
    public static Tuple<T1, T2> Create<T1, T2> (T1 item1, T2 item2)
    {
      return new Tuple<T1, T2> (item1, item2);
    }

    public static Tuple<T1, T2, T3> Create<T1, T2, T3> (T1 item1, T2 item2, T3 item3)
    {
      return new Tuple<T1, T2, T3> (item1, item2, item3);
    }

    public static Tuple<T1, T2, T3, T4> Create<T1, T2, T3, T4> (T1 item1, T2 item2, T3 item3, T4 item4)
    {
      return new Tuple<T1, T2, T3, T4> (item1, item2, item3, item4);
    }
  }
}
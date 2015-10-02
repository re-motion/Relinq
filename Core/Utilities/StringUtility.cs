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
#if NET_3_5
using System.Linq;
#endif

namespace Remotion.Linq.Utilities
{
  internal static class StringUtility
  {
    public static string Join<T> (string separator, IEnumerable<T> values)
    {
#if !NET_3_5
      return string.Join (separator, values);
#else
      if (typeof (T) == typeof (string))
        return string.Join (separator, values.Cast<string>().ToArray());
      else
        return string.Join (separator, values.Select (v=>v.ToString()).ToArray());
#endif
    }
  }
}
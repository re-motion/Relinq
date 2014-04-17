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

namespace Remotion.Linq.UnitTests.Parsing.Structure.TestDomain
{
  public class GenericClass<T>
  {
    public bool NonGenericMethod (T item)
    {
      return false;
    }

    public bool GenericMethod<T2> (T item1, T2 item2)
    {
      return false;
    }

    public bool GenericMethodHavingOverloadWithSameParameterCount<T2> (T item1, T2 item2, int value)
    {
      return false;
    }

    public bool GenericMethodHavingOverloadWithSameParameterCount<T2> (T item1, T2 item2, string value)
    {
      return false;
    }
  }

  public class GenericClass<T1, T2>
  {
    public bool NonGenericMethodOverloadedWithGenericParameterFromType (T1 item1, int item2)
    {
      return false;
    }

    public bool NonGenericMethodOverloadedWithGenericParameterFromType (T2 item1, int item2)
    {
      return false;
    }

    public bool NonGenericMethodOverloadedWithGenericParameterFromTypeAtDifferentPosition (T1 item1, int item2)
    {
      return false;
    }

    public bool NonGenericMethodOverloadedWithGenericParameterFromTypeAtDifferentPosition (int item2, T1 item1)
    {
      return false;
    }

    public bool GenericMethodOverloadedWithGenericParameterFromTypeAtSamePosition<T3> (T1 item1, T3 item2)
    {
      return false;
    }

    public bool GenericMethodOverloadedWithGenericParameterFromTypeAtSamePosition<T3> (T2 item1, T3 item2)
    {
      return false;
    }

    public bool GenericMethodOverloadedWithGenericParameterFromTypeAtDifferentPosition<T3> (T1 item1, T3 item2)
    {
      return false;
    }

    public bool GenericMethodOverloadedWithGenericParameterFromTypeAtDifferntPosition<T3> (T3 item1, T1 item2)
    {
      return false;
    }
  }
}

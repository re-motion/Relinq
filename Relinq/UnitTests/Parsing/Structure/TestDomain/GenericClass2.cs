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
  public class GenericClass<T1, T2>
  {
    public bool NonGenericMethodOverloadedWithGenericParameterFromTypeAndDifferentParameterName (T1 t1, double p2)
    {
      return false;
    }

    public bool NonGenericMethodOverloadedWithGenericParameterFromTypeAndDifferentParameterName (T2 t2, double p2)
    {
      return false;
    }

    public bool NonGenericMethodOverloadedWithGenericParameterFromTypeAtDifferentPosition (T1 p1, double p2)
    {
      return false;
    }

    public bool NonGenericMethodOverloadedWithGenericParameterFromTypeAtDifferentPosition (double p1, T1 p2)
    {
      return false;
    }
    
    public bool NonGenericMethodOverloadedWithGenericParameterFromTypeAndDifferentReturnTypes (T1 t, double p)
    {
      return false;
    }

    public DateTime NonGenericMethodOverloadedWithGenericParameterFromTypeAndDifferentReturnTypes (T2 t, double p)
    {
      return DateTime.Now;
    }

    public bool NonGenericMethodOverloadedWithGenericParameterFromTypeAndSameParameterName (T1 t, double p)
    {
      return false;
    }

    public bool NonGenericMethodOverloadedWithGenericParameterFromTypeAndSameParameterName (T2 t, double p)
    {
      return false;
    }

    public bool GenericMethodOverloadedWithGenericParameterFromTypeAtSamePosition<T3> (T1 t1, T3 t3)
    {
      return false;
    }

    public bool GenericMethodOverloadedWithGenericParameterFromTypeAtSamePosition<T3> (T2 t2, T3 t3)
    {
      return false;
    }

    public bool GenericMethodOverloadedWithGenericParameterFromTypeAtDifferentPosition<T3> (T1 t1, T3 t3)
    {
      return false;
    }

    public bool GenericMethodOverloadedWithGenericParameterFromTypeAtDifferntPosition<T3> (T3 t3, T1 t1)
    {
      return false;
    }
  }
}
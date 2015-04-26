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
using System.Reflection;
using Remotion.Linq.UnitTests.TestDomain;

namespace Remotion.Linq.UnitTests.Parsing.ExpressionVisitorTests
{
  internal class TypeForNewExpression
  {
    public int C;

    public TypeForNewExpression ()
    {
    }

    public TypeForNewExpression (int a)
    {
      C = a;
    }

    public TypeForNewExpression (Cook d)
    {
      D = d;
    }

    public TypeForNewExpression (int a, int b)
    {
      C = a;
      B = b;
    }

    public TypeForNewExpression (bool e)
    {
      E = e;
    }

    public int A
    {
      get { return C; }
      set { C = value; }
    }

    public int B { get; set; }

    public Cook D { get; set; }

    public bool E { get; set; }

    public int get_
    {
      get { return C; }
      set { C = value; }
    }

    public static int CompareString (string s1, string s2, bool textCompare)
    {
      return s1.CompareTo (s2);
    }

    public static ConstructorInfo GetConstructor (params Type[] parameterTypes)
    {
      var constructorInfo = typeof (TypeForNewExpression).GetConstructor (parameterTypes);
      if (constructorInfo == null)
        throw new ArgumentException ("Ctor not found.", "parameterTypes");
      return constructorInfo;
    }
  }
}

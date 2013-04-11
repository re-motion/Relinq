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
using System.Reflection;
using Remotion.Linq.UnitTests.Linq.Core.TestDomain;

namespace Remotion.Linq.UnitTests.Linq.Core.Parsing.ExpressionTreeVisitorTests
{
  internal class TypeForNewExpression
  {
    public int C;

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

    public int A
    {
      get { return C; }
      set { C = value; }
    }
    public int B { get; set; }

    public Cook D { get; set; }

    public static int CompareString (string s1, string s2, bool textCompare)
    {
      return s1.CompareTo (s2);
    }

    // TODO 4878: Use this instead of typeof (TypeForNewExpression).GetConstructor.
    public static ConstructorInfo GetConstructor (params Type[] parameterTypes)
    {
      var constructorInfo = typeof (TypeForNewExpression).GetConstructor (parameterTypes);
      if (constructorInfo == null)
        throw new ArgumentException ("Ctor not found.", "parameterTypes");
      return constructorInfo;
    }
  }
}

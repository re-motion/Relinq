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
using System.Linq.Expressions;
using System.Reflection;

namespace Remotion.Linq.Parsing.ExpressionVisitors.MemberBindings
{
  /// <summary>
  /// Represents a <see cref="MethodInfo"/> being bound to an associated <see cref="Expression"/> instance. 
  /// <seealso cref="System.Linq.Expressions.MemberBinding"/>
  /// This binding's 
  /// <see cref="MatchesReadAccess"/> method returns <see langword="true"/> for the same <see cref="MethodInfo"/> the expression is bound to or for a
  /// <see cref="PropertyInfo"/> whose getter method is the <see cref="MethodInfo"/> the expression is bound to.
  /// </summary>
  public class MethodInfoBinding : MemberBinding
  {
    public MethodInfoBinding (MethodInfo boundMember, Expression associatedExpression)
        : base (boundMember, associatedExpression)
    {
    }

    public override bool MatchesReadAccess (MemberInfo readMember)
    {
      if (readMember == BoundMember)
        return true;

      var property = readMember as PropertyInfo;
      if (property != null && property.CanRead && property.GetGetMethod (true) == BoundMember)
        return true;

      return false;
    }
  }
}

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
  /// Represents a <see cref="FieldInfo"/> being bound to an associated <see cref="Expression"/> instance. This binding's 
  /// <see cref="MatchesReadAccess"/> method returns <see langword="true"/> only for the same <see cref="FieldInfo"/> the expression is bound to.
  /// <seealso cref="System.Linq.Expressions.MemberBinding"/>
  /// </summary>
  public class FieldInfoBinding : MemberBinding
  {
    public FieldInfoBinding (FieldInfo boundMember, Expression associatedExpression)
        : base (boundMember, associatedExpression)
    {
    }

    public override bool MatchesReadAccess (MemberInfo member)
    {
      return member == BoundMember;
    }
  }
}

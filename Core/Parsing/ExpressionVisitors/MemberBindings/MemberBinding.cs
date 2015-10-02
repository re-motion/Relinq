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
using Remotion.Utilities;

namespace Remotion.Linq.Parsing.ExpressionVisitors.MemberBindings
{
  /// <summary>
  /// Represents a <see cref="MemberInfo"/> being bound to an associated <see cref="Expression"/> instance. This is used by the 
  /// <see cref="TransparentIdentifierRemovingExpressionVisitor"/> to represent assignments in constructor calls such as <c>new AnonymousType (a = 5)</c>, 
  /// where <c>a</c> is the member of <c>AnonymousType</c> and <c>5</c> is the associated expression.
  /// The <see cref="MatchesReadAccess"/> method can be used to check whether the member bound to an expression matches a given <see cref="MemberInfo"/>
  /// (considering read access). See the subclasses for details.
  /// </summary>
  public abstract class MemberBinding
  {
    public static MemberBinding Bind (MemberInfo boundMember, Expression associatedExpression)
    {
      ArgumentUtility.CheckNotNull ("boundMember", boundMember);
      ArgumentUtility.CheckNotNull ("associatedExpression", associatedExpression);

      var methodInfo = boundMember as MethodInfo;
      if (methodInfo != null)
        return new MethodInfoBinding (methodInfo, associatedExpression);

      var propertyInfo = boundMember as PropertyInfo;
      if (propertyInfo != null)
        return new PropertyInfoBinding (propertyInfo, associatedExpression);

      return new FieldInfoBinding ((FieldInfo) boundMember, associatedExpression);
    }

    private readonly MemberInfo _boundMember;
    private readonly Expression _associatedExpression;

    public MemberInfo BoundMember
    {
      get { return _boundMember; }
    }

    public Expression AssociatedExpression
    {
      get { return _associatedExpression; }
    }

    public MemberBinding (MemberInfo boundMember, Expression associatedExpression)
    {
      ArgumentUtility.CheckNotNull ("boundMember", boundMember);
      ArgumentUtility.CheckNotNull ("associatedExpression", associatedExpression);

      _boundMember = boundMember;
      _associatedExpression = associatedExpression;
    }

    public abstract bool MatchesReadAccess (MemberInfo member);
  }
}

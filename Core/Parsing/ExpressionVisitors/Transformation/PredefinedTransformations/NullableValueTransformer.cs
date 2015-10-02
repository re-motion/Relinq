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

namespace Remotion.Linq.Parsing.ExpressionVisitors.Transformation.PredefinedTransformations
{
  /// <summary>
  /// Replaces calls to <see cref="Nullable{T}.Value"/> and <see cref="Nullable{T}.HasValue"/> with casts and null checks. This allows LINQ providers
  /// to treat nullables like reference types.
  /// </summary>
  public class NullableValueTransformer : IExpressionTransformer<MemberExpression>
  {
    public ExpressionType[] SupportedExpressionTypes
    {
      get { return new[] { ExpressionType.MemberAccess }; }
    }

    public Expression Transform (MemberExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      if (expression.Member.Name == "Value" && IsDeclaredByNullableType(expression.Member))
        return Expression.Convert (expression.Expression, expression.Type);
      else if (expression.Member.Name == "HasValue" && IsDeclaredByNullableType (expression.Member))
        return Expression.NotEqual (expression.Expression, Expression.Constant (null, expression.Member.DeclaringType));
      else
      return expression;
    }

    private bool IsDeclaredByNullableType (MemberInfo memberInfo)
    {
      return memberInfo.DeclaringType.GetTypeInfo().IsGenericType && memberInfo.DeclaringType.GetGenericTypeDefinition() == typeof (Nullable<>);
    }
  }
}
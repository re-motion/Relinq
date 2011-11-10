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
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Linq.Utilities;

namespace Remotion.Linq.Parsing.ExpressionTreeVisitors.Transformation.PredefinedTransformations
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
      return memberInfo.DeclaringType.IsGenericType && memberInfo.DeclaringType.GetGenericTypeDefinition() == typeof (Nullable<>);
    }
  }
}
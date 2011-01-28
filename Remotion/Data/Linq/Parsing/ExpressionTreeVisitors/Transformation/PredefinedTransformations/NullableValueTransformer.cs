// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// as published by the Free Software Foundation; either version 2.1 of the 
// License, or (at your option) any later version.
// 
// re-motion is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-motion; if not, see http://www.gnu.org/licenses.
// 
using System;
using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.Parsing.ExpressionTreeVisitors.Transformation.PredefinedTransformations
{
  public class NullableValueTransformer : IExpressionTransformer<MemberExpression>
  {
    public ExpressionType[] SupportedExpressionTypes
    {
      get { return new[] { ExpressionType.MemberAccess }; }
    }

    public Expression Transform (MemberExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      if ((expression.Member.Name == "Value" || expression.Member.Name == "HasValue") 
        && (expression.Member.DeclaringType.IsGenericType && expression.Member.DeclaringType.GetGenericTypeDefinition() == typeof (Nullable<>)))
      {
        if (expression.Member.Name == "HasValue")
          return Expression.NotEqual (expression.Expression, Expression.Constant (null, expression.Member.DeclaringType));

        if (expression.Member.Name == "Value")
          return Expression.Convert (expression.Expression, expression.Type);

        throw new NotImplementedException();
      }

      return expression;
    }
  }
}
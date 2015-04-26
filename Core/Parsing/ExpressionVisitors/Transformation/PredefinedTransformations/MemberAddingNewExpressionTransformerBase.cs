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
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Utilities;

namespace Remotion.Linq.Parsing.ExpressionVisitors.Transformation.PredefinedTransformations
{
  /// <summary>
  /// Provides a base class for transformers detecting <see cref="NewExpression"/> nodes for tuple types and adding <see cref="MemberInfo"/> metadata 
  /// to those nodes. This allows LINQ providers to match member access and constructor arguments more easily.
  /// </summary>
  public abstract class MemberAddingNewExpressionTransformerBase : IExpressionTransformer<NewExpression>
  {
    protected abstract bool CanAddMembers (Type instantiatedType, ReadOnlyCollection<Expression> arguments);
    protected abstract MemberInfo[] GetMembers (ConstructorInfo constructorInfo, ReadOnlyCollection<Expression> arguments);

    public ExpressionType[] SupportedExpressionTypes
    {
      get { return new[] { ExpressionType.New }; }
    }

    public Expression Transform (NewExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

// ReSharper disable ConditionIsAlwaysTrueOrFalse
// ReSharper disable HeuristicUnreachableCode
      if (expression.Members == null && CanAddMembers (expression.Type, expression.Arguments))
      {
        var members = GetMembers (expression.Constructor, expression.Arguments);
        return Expression.New (
            expression.Constructor,
            RelinqExpressionVisitor.AdjustArgumentsForNewExpression (expression.Arguments, members),
            members);
      }
// ReSharper restore HeuristicUnreachableCode
// ReSharper restore ConditionIsAlwaysTrueOrFalse

      return expression;
    }

    protected MemberInfo GetMemberForNewExpression (Type instantiatedType, string propertyName)
    {
      // In .NET 4, Expression.New (...) will convert the get method into a property. That way, the generated NewExpression will look exactly like
      // an anonymous type expression.
      return instantiatedType.GetRuntimeProperty (propertyName).GetGetMethod (true);
    }
  }
}
// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
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
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Linq.Utilities;

namespace Remotion.Linq.Parsing.ExpressionTreeVisitors.Transformation.PredefinedTransformations
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
            ExpressionTreeVisitor.AdjustArgumentsForNewExpression (expression.Arguments, members),
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
      return instantiatedType.GetProperty (propertyName).GetGetMethod ();
    }
  }
}
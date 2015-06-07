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
using Remotion.Linq.Clauses.Expressions;
using Remotion.Utilities;

namespace Remotion.Linq.UnitTests
{
  public class ExpressionTreeNavigator
  {
    private readonly Expression _expression;

    public ExpressionTreeNavigator (Expression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      _expression = expression;
    }

    public ExpressionTreeNavigator Operand
    {
      get { return new ExpressionTreeNavigator (((UnaryExpression)Expression).Operand); }
    }

    public ExpressionTreeNavigator Body
    {
      get { return new ExpressionTreeNavigator (((LambdaExpression) Expression).Body); }
    }

    public ExpressionTreeNavigator Left
    {
      get { return new ExpressionTreeNavigator (((BinaryExpression) Expression).Left); }
    }

    public ExpressionTreeNavigator Right
    {
      get { return new ExpressionTreeNavigator (((BinaryExpression) Expression).Right); }
    }

    public object Value
    {
      get
      {
        var constantExpression = Expression as ConstantExpression;
        if (constantExpression != null)
          return constantExpression.Value;
        else
          return ((PartiallyEvaluatedExpression) Expression).EvaluatedExpression.Value;
      }
    }

    public ArgumentsNavigator Arguments
    {
      get { return new ArgumentsNavigator (this); }
    }

    public ParametersNavigator Parameters
    {
      get { return new ParametersNavigator (this); }
    }

    public ExpressionTreeNavigator MemberAccess_Expression
    {
      get { return new ExpressionTreeNavigator (((MemberExpression) Expression).Expression); }
    }

    public Expression Expression
    {
      get { return _expression; }
    }

    public ExpressionTreeNavigator Object
    {
      get { return new ExpressionTreeNavigator (((MethodCallExpression) Expression).Object); }
    }

    public string Name
    {
      get { return ((ParameterExpression)Expression).Name; }
    }

    public class ArgumentsNavigator
    {
      private readonly ExpressionTreeNavigator _navigator;

      public ArgumentsNavigator (ExpressionTreeNavigator navigator)
      {
        _navigator = navigator;
      }

      public ExpressionTreeNavigator this[int index]
      {
        get { return new ExpressionTreeNavigator (((MethodCallExpression) _navigator.Expression).Arguments[index]); }
      }
    }

    public class ParametersNavigator
    {
      private readonly ExpressionTreeNavigator _navigator;

      public ParametersNavigator (ExpressionTreeNavigator navigator)
      {
        _navigator = navigator;
      }

      public ExpressionTreeNavigator this[int index]
      {
        get { return new ExpressionTreeNavigator (((LambdaExpression) _navigator.Expression).Parameters[index]); }
      }
    }
  }
}
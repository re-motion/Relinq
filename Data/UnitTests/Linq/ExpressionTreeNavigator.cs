/* Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh
 *
 * This program is free software: you can redistribute it and/or modify it under 
 * the terms of the re:motion license agreement in license.txt. If you did not 
 * receive it, please visit http://www.re-motion.org/licensing.
 * 
 * Unless otherwise provided, this software is distributed on an "AS IS" basis, 
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 */

using System.Linq.Expressions;
using Remotion.Utilities;

namespace Remotion.Data.UnitTests.Linq
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
      get { return ((ConstantExpression) Expression).Value; }
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

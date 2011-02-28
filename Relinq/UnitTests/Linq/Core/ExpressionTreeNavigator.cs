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
using System.Linq.Expressions;
using Remotion.Linq.Utilities;

namespace Remotion.Linq.UnitTests.Linq.Core
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
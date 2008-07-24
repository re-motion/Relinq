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

namespace Remotion.Data.Linq.Parsing.Structure
{
  public abstract class BodyExpressionDataBase<TExpression> : BodyExpressionDataBase
      where TExpression : Expression
  {
    private TExpression _expression;
    
    public BodyExpressionDataBase(TExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      _expression = expression;
    }

    public TExpression TypedExpression
    {
      get { return _expression; }
    }

    public override Expression Expression
    {
      get { return _expression; }
      set { _expression = (TExpression) value; }
    }
  }

  public abstract class BodyExpressionDataBase
  {
    public abstract Expression Expression { get; set; }
  }
}

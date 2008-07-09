/* Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh
 *
 * This program is free software: you can redistribute it and/or modify it under 
 * the terms of the re:motion license agreement in license.txt. If you did not 
 * receive it, please visit http://www.re-motion.org/licensing.
 * 
 * Unless otherwise provided, this software is distributed on an "AS IS" basis, 
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 */

using System;
using System.Linq.Expressions;

namespace Remotion.Data.Linq.Parsing
{
  public class ParserException : Exception
  {
    public ParserException (string message)
        : this (message, null, null, null)
    {
    }

    public ParserException (string message, Exception inner)
        : base (message, inner)
    {
    }

    public ParserException (string message, object parsedExpression, Expression expressionTree, Exception inner)
        : base (message, inner)
    {
      ParsedExpression = parsedExpression;
      ExpressionTree = expressionTree;
    }

    public object ParsedExpression { get; private set; }
    public Expression ExpressionTree { get; private set; }
  }
}

/* Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh
 *
 * This program is free software: you can redistribute it and/or modify it under 
 * the terms of the re:motion license agreement in license.txt. If you did not 
 * receive it, please visit http://www.re-motion.org/licensing.
 * 
 * Unless otherwise provided, this software is distributed on an "AS IS" basis, 
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 */

using System.Collections.Generic;
using System.Linq.Expressions;
using Remotion.Data.Linq.Expressions;
using Remotion.Data.Linq.Parsing.TreeEvaluation;

namespace Remotion.Data.UnitTests.Linq.ParsingTest.TreeEvaluationTest
{
  public class TestablePartialEvaluationPreAnalyzer : PartialEvaluationPreAnalyzer
  {
    public bool VisitBinaryExpressionCalled { get; set; }
    public Expression StackTopInVisitBinary { get; set; }

    public new Stack<Expression> CurrentExpressions
    {
      get { return base.CurrentExpressions; }
    }

    public new void PrepareExpression (Expression expression)
    {
      base.PrepareExpression (expression);
    }

    public new void FinishExpression ()
    {
      base.FinishExpression ();
    }

    public new Expression VisitExpression (Expression expression)
    {
      return base.VisitExpression (expression);
    }

    public new Expression VisitLambdaExpression (LambdaExpression expression)
    {
      return base.VisitLambdaExpression (expression);
    }

    public new Expression VisitParameterExpression (ParameterExpression expression)
    {
      return base.VisitParameterExpression (expression);
    }

    public new Expression VisitSubQueryExpression (SubQueryExpression expression)
    {
      return base.VisitSubQueryExpression (expression);
    }

    protected override Expression VisitBinaryExpression (BinaryExpression expression)
    {
      VisitBinaryExpressionCalled = true;
      StackTopInVisitBinary = CurrentExpressions.Peek();
      return base.VisitBinaryExpression (expression);
    }
  }
}

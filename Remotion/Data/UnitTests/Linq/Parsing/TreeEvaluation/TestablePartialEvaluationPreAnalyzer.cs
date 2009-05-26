// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// version 3.0 as published by the Free Software Foundation.
// 
// re-motion is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-motion; if not, see http://www.gnu.org/licenses.
// 
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

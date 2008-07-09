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
using Remotion.Data.Linq.Visitor;

namespace Remotion.Data.Linq.Parsing.TreeEvaluation
{
  public class PartialEvaluationPreAnalyzer : ExpressionTreeVisitor
  {
    public PartialEvaluationPreAnalyzer ()
    {
      EvaluationData = new PartialEvaluationData ();
      CurrentExpressions = new Stack<Expression> ();
    }

    public PartialEvaluationData EvaluationData { get; private set; }
    protected Stack<Expression> CurrentExpressions { get; private set; }

    public void Analyze (Expression expression)
    {
      VisitExpression (expression);
    }

    protected override Expression VisitExpression (Expression expression)
    {
      if (expression is ConstantExpression || expression == null)
        return expression;

      PrepareExpression(expression);
      base.VisitExpression (expression);
      FinishExpression();
      
      return expression;
    }

    protected void PrepareExpression (Expression expression)
    {
      if (!EvaluationData.DeclaredParameters.ContainsKey (expression))
        EvaluationData.DeclaredParameters.Add (expression, new HashSet<ParameterExpression> ());
      if (!EvaluationData.UsedParameters.ContainsKey (expression))
        EvaluationData.UsedParameters.Add (expression, new HashSet<ParameterExpression> ());
      if (!EvaluationData.SubQueries.ContainsKey (expression))
        EvaluationData.SubQueries.Add (expression, new HashSet<SubQueryExpression> ());

      CurrentExpressions.Push (expression);
    }

    protected void FinishExpression ()
    {
      CurrentExpressions.Pop ();
    }

    protected override Expression VisitLambdaExpression (LambdaExpression expression)
    {
      foreach (ParameterExpression declaredParameter in expression.Parameters)
        AddEvaluationData (EvaluationData.DeclaredParameters, declaredParameter);

      VisitExpression (expression.Body);
      return expression;
    }

    protected override Expression VisitParameterExpression (ParameterExpression expression)
    {
      AddEvaluationData (EvaluationData.UsedParameters, expression);
      return expression;
    }

    protected override Expression VisitSubQueryExpression (SubQueryExpression expression)
    {
      AddEvaluationData (EvaluationData.SubQueries, expression);
      return expression;
    }

    private void AddEvaluationData<T> (Dictionary<Expression, HashSet<T>> dataStore, T dataValue)
    {
      foreach (Expression currentExpression in CurrentExpressions)
        dataStore[currentExpression].Add (dataValue);
    }

  }
}

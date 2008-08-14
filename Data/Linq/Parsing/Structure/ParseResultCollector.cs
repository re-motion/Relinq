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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Parsing.TreeEvaluation;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.Structure
{
  public class ParseResultCollector
  {
    private readonly List<BodyExpressionDataBase> _bodyExpressions = new List<BodyExpressionDataBase> ();
    private readonly List<LambdaExpression> _projectionExpressions = new List<LambdaExpression> ();
    private readonly List<MethodCallExpression> _resultModifierData = new List<MethodCallExpression>();
    
    public ParseResultCollector (Expression expressionTreeRoot)
    {
      ArgumentUtility.CheckNotNull ("expressionTreeRoot", expressionTreeRoot);
      ExpressionTreeRoot = expressionTreeRoot;
    }

    public Expression ExpressionTreeRoot { get; private set; }
    
    public ReadOnlyCollection<BodyExpressionDataBase> BodyExpressions
    {
      get { return _bodyExpressions.AsReadOnly(); }
    }

    public ReadOnlyCollection<LambdaExpression> ProjectionExpressions
    {
      get { return _projectionExpressions.AsReadOnly (); }
    }

    public ReadOnlyCollection<MethodCallExpression> ResultModifierData 
    {
      get { return _resultModifierData.AsReadOnly(); }
    }

    public List<MethodCallExpression> ResultModifiers { get; private set; }

    public void AddResultModifiers (MethodCallExpression expression)
    {
      ResultModifiers.Add (expression);
    }

    public void AddResultModifierData (MethodCallExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      _resultModifierData.Add (expression);
    }

    public void AddBodyExpression (BodyExpressionDataBase expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      _bodyExpressions.Add (expression);
    }

    public FromExpressionData ExtractMainFromExpression()
    {
      if (BodyExpressions.Count == 0)
        throw new InvalidOperationException ("There are no body expressions to be extracted.");

      FromExpressionData fromExpressionData = BodyExpressions[0] as FromExpressionData;
      if (fromExpressionData == null)
        throw new InvalidOperationException ("The first body expression is no FromExpressionData.");

      _bodyExpressions.RemoveAt (0);
      return fromExpressionData;
    }

    public void AddProjectionExpression (LambdaExpression expression)
    {
      _projectionExpressions.Add (expression);
    }

    public void Simplify (List<QueryModel> subQueryRegistry)
    {
      for (int i = 0; i < _projectionExpressions.Count; ++i)
        _projectionExpressions[i] = Simplify (_projectionExpressions[i], subQueryRegistry);

      for (int i = 0; i < _bodyExpressions.Count; ++i)
        _bodyExpressions[i].Expression = Simplify (_bodyExpressions[i].Expression, subQueryRegistry);
    }

    public static T Simplify<T> (T expression, List<QueryModel> subQueryRegistry)
        where T : Expression
    {
      if (expression == null)
        return expression;

      SubQueryFindingVisitor subQueryFindingVisitor = new SubQueryFindingVisitor (subQueryRegistry);
      T newExpression = (T) subQueryFindingVisitor.ReplaceSubQueries (expression);

      PartialTreeEvaluator partialEvaluator = new PartialTreeEvaluator (newExpression);
      newExpression = (T) partialEvaluator.GetEvaluatedTree();

      return newExpression;
    }
  }
}

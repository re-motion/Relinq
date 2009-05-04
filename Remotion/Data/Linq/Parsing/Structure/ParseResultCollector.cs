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
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using Remotion.Data.Linq.Parsing.TreeEvaluation;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.Structure
{
  /// <summary>
  /// Caches information of expression. The stored informaion is used to generate <see cref="QueryModel"/>.
  /// </summary>
  public class ParseResultCollector
  {
    private readonly List<BodyExpressionDataBase> _bodyExpressions = new List<BodyExpressionDataBase> ();
    private readonly List<LambdaExpression> _projectionExpressions = new List<LambdaExpression> ();
    private readonly List<MethodCallExpression> _resultModifierData = new List<MethodCallExpression>();

    /// <summary>
    /// Initialize a new instance of <see cref="ParseResultCollector"/>.
    /// </summary>
    /// <param name="expressionTreeRoot">expression tree of executed linq query</param>
    public ParseResultCollector (Expression expressionTreeRoot)
    {
      ArgumentUtility.CheckNotNull ("expressionTreeRoot", expressionTreeRoot);
      ExpressionTreeRoot = expressionTreeRoot;
    }

    public Expression ExpressionTreeRoot { get; private set; }

    /// <summary>
    /// Get all <see cref="BodyExpressionDataBase"/>. 
    /// </summary>
    public ReadOnlyCollection<BodyExpressionDataBase> BodyExpressions
    {
      get { return _bodyExpressions.AsReadOnly(); }
    }

    /// <summary>
    /// Get the projection of a expression.
    /// </summary>
    public ReadOnlyCollection<LambdaExpression> ProjectionExpressions
    {
      get { return _projectionExpressions.AsReadOnly (); }
    }

    /// <summary>
    /// Get the modifier of a linq query (e.g. Distinct)
    /// </summary>
    public ReadOnlyCollection<MethodCallExpression> ResultModifierExpression 
    {
      get { return _resultModifierData.AsReadOnly(); }
    }

    /// <summary>
    /// Add <see cref="MethodCallExpression"/> of a modifier.
    /// </summary>
    /// <param name="expression">Expression of the method call which modifies the result of a linq query.</param>
    public void AddResultModifierExpression (MethodCallExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      _resultModifierData.Add (expression);
    }

    /// <summary>
    /// Add <see cref="BodyExpressionDataBase"/>. 
    /// </summary>
    /// <param name="expression"></param>
    public void AddBodyExpression (BodyExpressionDataBase expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      _bodyExpressions.Add (expression);
    }

    /// <summary>
    /// Add <see cref="LambdaExpression"/>
    /// </summary>
    /// <param name="expression"></param>
    public void AddProjectionExpression (LambdaExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      _projectionExpressions.Add (expression);
    }

    /// <summary>
    /// Add <see cref="ParameterExpression"/>
    /// </summary>
    /// <param name="sourceParameterOfPreviousClause"></param>
    public void AddIdentityProjectionExpression (ParameterExpression sourceParameterOfPreviousClause)
    {
      ArgumentUtility.CheckNotNull ("sourceParameterOfPreviousClause", sourceParameterOfPreviousClause);
      AddProjectionExpression (Expression.Lambda (sourceParameterOfPreviousClause, sourceParameterOfPreviousClause));
    }

    /// <summary>
    /// Removes a <see cref="FromExpressionData"/> with represents the main from of a linq query.
    /// </summary>
    /// <returns><see cref="FromExpressionData"/></returns>
    public FromExpressionData ExtractMainFromExpression ()
    {
      if (BodyExpressions.Count == 0)
        throw new InvalidOperationException ("There are no body expressions to be extracted.");

      FromExpressionData fromExpressionData = BodyExpressions[0] as FromExpressionData;
      if (fromExpressionData == null)
        throw new InvalidOperationException ("The first body expression is no FromExpressionData.");

      _bodyExpressions.RemoveAt (0);
      return fromExpressionData;
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
      SubQueryFindingVisitor subQueryFindingVisitor = new SubQueryFindingVisitor (subQueryRegistry);
      T newExpression = (T) subQueryFindingVisitor.ReplaceSubQueries (expression);

      PartialTreeEvaluator partialEvaluator = new PartialTreeEvaluator (newExpression);
      newExpression = (T) partialEvaluator.GetEvaluatedTree();

      return newExpression;
    }

    public void DeleteBodyExpression (BodyExpressionDataBase expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      _bodyExpressions.Remove (expression);
    }
  }
}

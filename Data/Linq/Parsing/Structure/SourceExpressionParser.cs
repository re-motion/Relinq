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
using System.Reflection;
using Remotion.Data.Linq.Parsing.Structure;
using Remotion.Data.Linq.Parsing.TreeEvaluation;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.Structure
{
  public class SourceExpressionParser
  {
    private readonly bool _isTopLevel;
    private readonly CallParserDispatcher _callDispatcher;
    
    public SourceExpressionParser (bool isTopLevel)
    {
      _isTopLevel = isTopLevel;
      _callDispatcher = new CallParserDispatcher ();

      _callDispatcher.RegisterParser ("Select", ParseSelectSource);
      _callDispatcher.RegisterParser ("SelectMany", ParseSelectManySource);
      _callDispatcher.RegisterParser ("Where", ParseWhereSource);
      _callDispatcher.RegisterParser ("OrderBy", ParseOrderBySource);
      _callDispatcher.RegisterParser ("OrderByDescending", ParseOrderBySource);
      _callDispatcher.RegisterParser ("ThenBy", ParseOrderBySource);
      _callDispatcher.RegisterParser ("ThenByDescending", ParseOrderBySource);
      _callDispatcher.RegisterParser ("Distinct", ParseDistinctSource);
    }

    public CallParserDispatcher CallDispatcher
    {
      get { return _callDispatcher; }
    }

    public void Parse (ParseResultCollector resultCollector, Expression sourceExpression, ParameterExpression potentialFromIdentifier, string context)
    {
      ArgumentUtility.CheckNotNull ("resultCollector", resultCollector);
      ArgumentUtility.CheckNotNull ("sourceExpression", sourceExpression);
      ArgumentUtility.CheckNotNull ("context", context);

      var methodCallExpression = sourceExpression as MethodCallExpression;
      if (methodCallExpression != null && _callDispatcher.CanParse (methodCallExpression.Method))
        _callDispatcher.Dispatch (resultCollector, methodCallExpression, potentialFromIdentifier);
      else
        ParseSimpleFromSource (resultCollector, sourceExpression, potentialFromIdentifier, context);
    }

    private void ParseSelectSource (ParseResultCollector resultCollector, Expression sourceExpression)
    {
      MethodCallExpression methodCallExpression = (MethodCallExpression) sourceExpression;
      if (_isTopLevel)
        new SelectExpressionParser ().Parse (resultCollector, methodCallExpression);
      else
        new LetExpressionParser().Parse (resultCollector, methodCallExpression);
    }

    private void ParseSelectManySource (ParseResultCollector resultCollector, Expression sourceExpression)
    {
      MethodCallExpression methodCallExpression = (MethodCallExpression) sourceExpression;
      new SelectManyExpressionParser().Parse (resultCollector, methodCallExpression);
    }

    private void ParseWhereSource (ParseResultCollector resultCollector, Expression sourceExpression)
    {
      MethodCallExpression methodCallExpression = (MethodCallExpression) sourceExpression;
      new WhereExpressionParser (_isTopLevel).Parse (resultCollector, methodCallExpression);
    }

    private void ParseOrderBySource (ParseResultCollector resultCollector, Expression sourceExpression)
    {
      MethodCallExpression methodCallExpression = (MethodCallExpression) sourceExpression;
      new OrderByExpressionParser (_isTopLevel).Parse ( resultCollector, methodCallExpression);
    }

    private void ParseDistinctSource (ParseResultCollector resultCollector, Expression sourceExpression, ParameterExpression potentialFromIdentifier)
    {
      if (!_isTopLevel)
      {
        string message = string.Format ("Distinct is only allowed at the top level of a query, not in the middle: '{0}'.", resultCollector.ExpressionTreeRoot);
        throw new ParserException (message, sourceExpression, resultCollector.ExpressionTreeRoot, null);
      }

      resultCollector.SetDistinct ();
      Expression selectExpression = ((MethodCallExpression) sourceExpression).Arguments[0];
      Parse (resultCollector, selectExpression, potentialFromIdentifier, "first argument of distinct");
    }

    private void ParseSimpleFromSource (ParseResultCollector resultCollector, Expression sourceExpression, ParameterExpression potentialFromIdentifier, string context)
    {
      new SimpleFromSourceExpressionParser ().Parse (resultCollector, sourceExpression, potentialFromIdentifier, context);
    }
  }
}

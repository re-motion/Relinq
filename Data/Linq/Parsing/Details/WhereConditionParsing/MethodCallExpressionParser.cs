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
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.Details.WhereConditionParsing
{
  public class MethodCallExpressionParser : IWhereConditionParser
  {
    private readonly WhereConditionParserRegistry _parserRegistry;

    public MethodCallExpressionParser (WhereConditionParserRegistry parserRegistry)
    {
      ArgumentUtility.CheckNotNull ("parserRegistry", parserRegistry);

      _parserRegistry = parserRegistry;
    }

    public ICriterion Parse (MethodCallExpression methodCallExpression, ParseContext parseContext)
    {
      ArgumentUtility.CheckNotNull ("methodCallExpression", methodCallExpression);
      ArgumentUtility.CheckNotNull ("parseContext", parseContext);

      MethodInfo methodInfo = methodCallExpression.Method;
      ICriterion criterionObject;

      if (methodCallExpression.Object == null)
        criterionObject = null;
      else
        criterionObject = _parserRegistry.GetParser (methodCallExpression.Object).
          Parse (methodCallExpression.Object, parseContext);

      List<IEvaluation> criterionArguments = new List<IEvaluation> ();

      foreach (Expression exp in methodCallExpression.Arguments)
        criterionArguments.Add (_parserRegistry.GetParser (exp).Parse (exp, parseContext));

      return new MethodCall (methodInfo, criterionObject, criterionArguments); 
    }

    public bool CanParse (Expression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      return expression is MethodCallExpression;      
    }

    ICriterion IWhereConditionParser.Parse (Expression expression, ParseContext parseContext)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      ArgumentUtility.CheckNotNull ("parseContext", parseContext);
      
      return Parse ((MethodCallExpression) expression, parseContext);
    }
  }
}
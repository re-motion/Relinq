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
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.Structure
{
  public class SubQueryFindingVisitor : ExpressionTreeVisitor
  {
    private readonly List<QueryModel> _subQueryRegistry;
    private readonly SourceExpressionParser _referenceParser = new SourceExpressionParser (true);

    public SubQueryFindingVisitor (List<QueryModel> subQueryRegistry)
    {
      _subQueryRegistry = subQueryRegistry;
    }

    public Expression ReplaceSubQueries (Expression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      return VisitExpression(expression);
    }

    protected override Expression VisitMethodCallExpression (MethodCallExpression expression)
    {
      if (_referenceParser.CallDispatcher.CanParse (expression.Method))
        return CreateSubQueryNode (expression);
      else
        return base.VisitMethodCallExpression (expression);
    }

    private SubQueryExpression CreateSubQueryNode (MethodCallExpression methodCallExpression)
    {
      QueryParser parser = new QueryParser (methodCallExpression);
      QueryModel queryModel = parser.GetParsedQuery ();
      _subQueryRegistry.Add (queryModel);
      return new SubQueryExpression (queryModel);
    }
  }
}

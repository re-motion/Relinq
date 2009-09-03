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
using System.Linq.Expressions;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Parsing.Structure;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.ExpressionTreeVisitors
{
  /// <summary>
  /// Parses an expression tree, looks for sub-queries in that tree (ie. expressions that themselves are LINQ queries), parses and registers them, and
  /// replaces them with an instance of <see cref="SubQueryExpression"/>.
  /// </summary>
  public class SubQueryFindingVisitor : ExpressionTreeVisitor
  {
    public static Expression ReplaceSubQueries (Expression expressionTree, MethodCallExpressionNodeTypeRegistry nodeTypeRegistry)
    {
      ArgumentUtility.CheckNotNull ("expressionTree", expressionTree);
      ArgumentUtility.CheckNotNull ("nodeTypeRegistry", nodeTypeRegistry);

      var visitor = new SubQueryFindingVisitor (nodeTypeRegistry);
      return visitor.VisitExpression (expressionTree);
    }

    private readonly MethodCallExpressionNodeTypeRegistry _nodeTypeRegistry;
    private readonly QueryParser _innerParser;

    private SubQueryFindingVisitor (MethodCallExpressionNodeTypeRegistry nodeTypeRegistry)
    {
      ArgumentUtility.CheckNotNull ("nodeTypeRegistry", nodeTypeRegistry);

      _nodeTypeRegistry = nodeTypeRegistry;
      _innerParser = new QueryParser (new ExpressionTreeParser (_nodeTypeRegistry));
    }

    protected override Expression VisitExpression (Expression expression)
    {
      var potentialQueryOperatorExpression = _innerParser.ExpressionTreeParser.GetQueryOperatorExpression (expression);
      if (potentialQueryOperatorExpression != null
          && _innerParser.ExpressionTreeParser.NodeTypeRegistry.IsRegistered (potentialQueryOperatorExpression.Method))
        return CreateSubQueryNode (potentialQueryOperatorExpression);
      else
        return base.VisitExpression (expression);
    }

    protected override Expression VisitUnknownExpression (Expression expression)
    {
      //ignore
      return expression;
    }

    private SubQueryExpression CreateSubQueryNode (MethodCallExpression methodCallExpression)
    {
      QueryModel queryModel = _innerParser.GetParsedQuery (methodCallExpression);
      return new SubQueryExpression (queryModel);
    }
  }
}
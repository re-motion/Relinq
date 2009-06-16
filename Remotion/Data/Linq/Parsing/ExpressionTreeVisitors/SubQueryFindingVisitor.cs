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
    public static Expression ReplaceSubQueries (Expression expressionTree, MethodCallExpressionNodeTypeRegistry nodeTypeRegistry, List<QueryModel> subQueryRegistry)
    {
      ArgumentUtility.CheckNotNull ("expressionTree", expressionTree);
      ArgumentUtility.CheckNotNull ("nodeTypeRegistry", nodeTypeRegistry);
      ArgumentUtility.CheckNotNull ("subQueryRegistry", subQueryRegistry);
      
      var visitor = new SubQueryFindingVisitor (nodeTypeRegistry, subQueryRegistry);
      return visitor.VisitExpression (expressionTree);
    }

    private readonly MethodCallExpressionNodeTypeRegistry _nodeTypeRegistry;
    private readonly List<QueryModel> _subQueryRegistry;
    private readonly QueryParser _innerParser;

    // TODO: subQueryRegistry might become obsolete once the new Reolve mechanism is integrated and QueryModel is refactored
    private SubQueryFindingVisitor (MethodCallExpressionNodeTypeRegistry nodeTypeRegistry, List<QueryModel> subQueryRegistry)
    {
      ArgumentUtility.CheckNotNull ("nodeTypeRegistry", nodeTypeRegistry);
      ArgumentUtility.CheckNotNull ("subQueryRegistry", subQueryRegistry);

      _nodeTypeRegistry = nodeTypeRegistry;
      _subQueryRegistry = subQueryRegistry;
      _innerParser = new QueryParser (new ExpressionTreeParser (_nodeTypeRegistry));
    }

    protected override Expression VisitMethodCallExpression (MethodCallExpression expression)
    {
      if (_nodeTypeRegistry.IsRegistered (expression.Method))
        return CreateSubQueryNode (expression);
      else
        return base.VisitMethodCallExpression (expression);
    }

    private SubQueryExpression CreateSubQueryNode (MethodCallExpression methodCallExpression)
    {
      QueryModel queryModel = _innerParser.GetParsedQuery (methodCallExpression);
      _subQueryRegistry.Add (queryModel);
      return new SubQueryExpression (queryModel);
    }
  }
}
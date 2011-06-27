// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// re-linq is free software; you can redistribute it and/or modify it under 
// the terms of the GNU Lesser General Public License as published by the 
// Free Software Foundation; either version 2.1 of the License, 
// or (at your option) any later version.
// 
// re-linq is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-linq; if not, see http://www.gnu.org/licenses.
// 
using System.Linq.Expressions;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Parsing.Structure;
using Remotion.Linq.Parsing.Structure.ExpressionTreeProcessors;
using Remotion.Linq.Utilities;

namespace Remotion.Linq.Parsing.ExpressionTreeVisitors
{
  /// <summary>
  /// Preprocesses an expression tree for parsing. The preprocessing involves detection of sub-queries and VB-specific expressions.
  /// </summary>
  public class SubQueryFindingExpressionTreeVisitor : ExpressionTreeVisitor
  {
    public static Expression Process (Expression expressionTree, INodeTypeProvider nodeTypeProvider)
    {
      ArgumentUtility.CheckNotNull ("expressionTree", expressionTree);
      ArgumentUtility.CheckNotNull ("nodeTypeProvider", nodeTypeProvider);

      var visitor = new SubQueryFindingExpressionTreeVisitor (nodeTypeProvider);
      return visitor.VisitExpression (expressionTree);
    }

    private readonly INodeTypeProvider _nodeTypeProvider;
    private readonly ExpressionTreeParser _expressionTreeParser;
    private readonly QueryParser _queryParser;

    private SubQueryFindingExpressionTreeVisitor (INodeTypeProvider nodeTypeProvider)
    {
      ArgumentUtility.CheckNotNull ("nodeTypeProvider", nodeTypeProvider);

      _nodeTypeProvider = nodeTypeProvider;
      _expressionTreeParser = new ExpressionTreeParser (_nodeTypeProvider, new NullExpressionTreeProcessor());
      _queryParser = new QueryParser (_expressionTreeParser);
    }

    public override Expression VisitExpression (Expression expression)
    {
      var potentialQueryOperatorExpression = _expressionTreeParser.GetQueryOperatorExpression (expression);
      if (potentialQueryOperatorExpression != null && _nodeTypeProvider.IsRegistered (potentialQueryOperatorExpression.Method))
        return CreateSubQueryNode (potentialQueryOperatorExpression);
      else
        return base.VisitExpression (expression);
    }
    
    protected internal override Expression VisitUnknownNonExtensionExpression (Expression expression)
    {
      //ignore
      return expression;
    }

    private SubQueryExpression CreateSubQueryNode (MethodCallExpression methodCallExpression)
    {
      QueryModel queryModel = _queryParser.GetParsedQuery (methodCallExpression);
      return new SubQueryExpression (queryModel);
    }
  }
}
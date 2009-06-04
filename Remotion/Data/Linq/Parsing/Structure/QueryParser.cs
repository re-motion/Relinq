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
using System.Linq.Expressions;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Parsing.Structure.IntermediateModel;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.Structure
{
  public class QueryParser
  {
    private readonly ExpressionTreeParser _expressionTreeParser;

    public QueryParser ()
        : this (new ExpressionTreeParser (MethodCallExpressionNodeTypeRegistry.CreateDefault()))
    {
    }

    public QueryParser (ExpressionTreeParser expressionTreeParser)
    {
      ArgumentUtility.CheckNotNull ("expressionTreeParser", expressionTreeParser);
      _expressionTreeParser = expressionTreeParser;
    }

    public ExpressionTreeParser ExpressionTreeParser
    {
      get { return _expressionTreeParser; }
    }

    public QueryModel GetParsedQuery (Expression expressionTreeRoot)
    {
      ArgumentUtility.CheckNotNull ("expressionTreeRoot", expressionTreeRoot);
      var node = _expressionTreeParser.Parse (expressionTreeRoot);

      IClause lastClause = CreateClauseChain (node);
      SelectClause selectClause = GetOrCreateSelectClause(node, lastClause);

      // TODO: After COMMONS-1178, this code will not be needed any longer.
      var bodyClauses = new List<IBodyClause>();
      var findMainFromClause = FindMainFromClause (selectClause, bodyClauses);
      var queryModel = new QueryModel (expressionTreeRoot.Type, findMainFromClause, selectClause);
      foreach (var bodyClause in bodyClauses)
      {
        queryModel.AddBodyClause (bodyClause);
      }
      return queryModel;
    }

    private IClause CreateClauseChain (IExpressionNode node)
    {
      if (node.Source == null)
        return node.CreateClause (null);
      else
      {
        var previousClause = CreateClauseChain (node.Source);
        return node.CreateClause (previousClause);
      }
    }

    private SelectClause GetOrCreateSelectClause (IExpressionNode lastNode, IClause lastClause)
    {
      if (lastClause is SelectClause)
      {
        return (SelectClause) lastClause;
      }
      else
      {
        var parameterExpression = lastNode.CreateParameterForOutput();
        return new SelectClause (lastClause, Expression.Lambda (parameterExpression, parameterExpression));
      }
    }

    // TODO: After COMMONS-1178, this code will not be needed any longer.
    private MainFromClause FindMainFromClause (IClause clause, List<IBodyClause> bodyClauses)
    {
      if (clause is MainFromClause)
        return (MainFromClause) clause;
      else
      {
        if (clause is IBodyClause)
          bodyClauses.Add ((IBodyClause) clause);

        return FindMainFromClause (clause.PreviousClause, bodyClauses);
      }
    }
  }
}
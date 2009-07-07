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
using System.Linq;
using System.Linq.Expressions;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Parsing.Structure.IntermediateModel;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.Structure
{
  /// <summary>
  /// Takes an <see cref="Expression"/> tree and parses it into a <see cref="QueryModel"/> by use of an <see cref="ExpressionTreeParser"/>.
  /// It first transforms the <see cref="Expression"/> tree into a chain of <see cref="IExpressionNode"/> instances, and then calls 
  /// <see cref="MainSourceExpressionNode.CreateMainFromClause"/> and <see cref="IExpressionNode.Apply"/> in order to instantiate all the 
  /// <see cref="IClause"/>s. With those, a <see cref="QueryModel"/> is created and returned.
  /// </summary>
  public class QueryParser
  {
    private readonly ExpressionTreeParser _expressionTreeParser;

    /// <summary>
    /// Initializes a new instance of the <see cref="QueryParser"/> class, using a default instance of <see cref="ExpressionTreeParser"/> to
    /// convert <see cref="Expression"/> instances into <see cref="IExpressionNode"/>s. The <see cref="MethodCallExpressionNodeTypeRegistry"/> 
    /// used has all relevant methods of the <see cref="Queryable"/> class automatically 
    /// registered.
    /// </summary>
    public QueryParser ()
        : this (new ExpressionTreeParser (MethodCallExpressionNodeTypeRegistry.CreateDefault()))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="QueryParser"/> class, using the given <paramref name="expressionTreeParser"/> to
    /// convert <see cref="Expression"/> instances into <see cref="IExpressionNode"/>s.
    /// </summary>
    /// <param name="expressionTreeParser">The expression tree parser.</param>
    public QueryParser (ExpressionTreeParser expressionTreeParser)
    {
      ArgumentUtility.CheckNotNull ("expressionTreeParser", expressionTreeParser);
      _expressionTreeParser = expressionTreeParser;
    }

    public ExpressionTreeParser ExpressionTreeParser
    {
      get { return _expressionTreeParser; }
    }

    /// <summary>
    /// Gets the <see cref="QueryModel"/> of the given <paramref name="expressionTreeRoot"/>.
    /// </summary>
    /// <param name="expressionTreeRoot">The expression tree to parse.</param>
    /// <returns>A <see cref="QueryModel"/> that represents the query defined in <paramref name="expressionTreeRoot"/>.</returns>
    public QueryModel GetParsedQuery (Expression expressionTreeRoot)
    {
      ArgumentUtility.CheckNotNull ("expressionTreeRoot", expressionTreeRoot);

      var node = _expressionTreeParser.ParseTree (expressionTreeRoot);

      var clauseGenerationContext = new ClauseGenerationContext (new QuerySourceClauseMapping(), _expressionTreeParser.NodeTypeRegistry);

      QueryModel queryModel = ApplyAllNodes (expressionTreeRoot.Type, node, clauseGenerationContext);
      return queryModel;
    }

    /// <summary>
    /// Applies all nodes to a <see cref="QueryModel"/>, which is created by the trailing <see cref="MainSourceExpressionNode"/> in the 
    /// <paramref name="node"/> chain.
    /// </summary>
    /// <param name="resultType">The result type to put into the <see cref="QueryModel"/>.</param>
    /// <param name="node">The entry point to the <see cref="IExpressionNode"/> chain.</param>
    /// <param name="clauseGenerationContext">The clause generation context collecting context information during the parsing process.</param>
    /// <returns>A <see cref="QueryModel"/> created by the training <see cref="MainSourceExpressionNode"/> and transformed by each node in the
    /// <see cref="IExpressionNode"/> chain.</returns>
    private QueryModel ApplyAllNodes (Type resultType, IExpressionNode node, ClauseGenerationContext clauseGenerationContext)
    {
      if (node.Source == null) // this is the last node, create a MainFromClause and a QueryModel
      {
        var mainSourceExpressionNode = (MainSourceExpressionNode) node;
        var mainFromClause = mainSourceExpressionNode.CreateMainFromClause (clauseGenerationContext);
        var defaultSelectClause = new SelectClause (new QuerySourceReferenceExpression (mainFromClause));
        return new QueryModel (resultType, mainFromClause, defaultSelectClause);

        // return node.Apply (null, clauseGenerationContext); // TODO 1310
      }
      else // first go to the next node, then transform the queryModel (created by the last node) by applying the node to i
      {
        var queryModel = ApplyAllNodes (resultType, node.Source, clauseGenerationContext);
        return node.Apply (queryModel, clauseGenerationContext); // TODO 1310: There is only an integration test for this "return", but 1310 will add the unit test.
      }
    }
  }
}
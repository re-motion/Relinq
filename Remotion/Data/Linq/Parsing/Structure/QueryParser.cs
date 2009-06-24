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
using System.Linq;
using System.Linq.Expressions;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Parsing.Structure.IntermediateModel;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.Structure
{
  /// <summary>
  /// Takes an <see cref="Expression"/> tree and parses it into a <see cref="QueryModel"/> by use of an <see cref="ExpressionTreeParser"/>.
  /// It first transforms the <see cref="Expression"/> tree into a chain of <see cref="IExpressionNode"/> instances, and then calls 
  /// <see cref="IExpressionNode.CreateClause"/> to instantiate the <see cref="IClause"/>s. With those, a <see cref="QueryModel"/> is
  /// created and returned.
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

      var clauseGenerationContext = new ClauseGenerationContext (
          new QuerySourceClauseMapping(), _expressionTreeParser.NodeTypeRegistry, new ResultModificationExpressionNodeRegistry());

      var clauses = new List<IClause>();
      CreateClauses (clauses, node, clauseGenerationContext);
      CreateSelectClauseIfNecessary (clauses, node, clauseGenerationContext);

      var queryModelBuilder = new QueryModelBuilder();
      foreach (var clause in clauses)
        queryModelBuilder.AddClause (clause);

      var queryModel = queryModelBuilder.Build (expressionTreeRoot.Type);
      clauseGenerationContext.ResultModificationNodeRegistry.ApplyAll (queryModel, clauseGenerationContext);
      return queryModel;
    }

    /// <summary>
    /// Recursively creates <see cref="IClause"/>s from a chain of <see cref="IExpressionNode"/>s, putting the clauses into the 
    /// <paramref name="clauses"/> list in the same order as the corresponding nodes.
    /// </summary>
    /// <param name="clauses">The list receiving the clauses.</param>
    /// <param name="node">The last node in the chain to process.</param>
    /// <param name="clauseGenerationContext">A container for all the context information needed for creating clauses. This is used to resolve 
    /// predicates and selectors using <see cref="ExpressionResolver"/>s.</param>
    private void CreateClauses (List<IClause> clauses, IExpressionNode node, ClauseGenerationContext clauseGenerationContext)
    {
      if (node.Source == null) // this is the end of the node chain, create a clause symbolizing the end of the clause chain
        clauses.Add (node.CreateClause (null, clauseGenerationContext));
      else
      {
        // this is not the end of the chain, process the rest of the chain before processing this node
        CreateClauses (clauses, node.Source, clauseGenerationContext);

        var previousClause = clauses[clauses.Count - 1];
        if (previousClause is SelectClause && !(node is ResultModificationExpressionNodeBase)) // TODO 1178: Check this check
          clauses.RemoveAt (clauses.Count - 1);

        var newClause = node.CreateClause (previousClause, clauseGenerationContext);
        if (newClause != previousClause)
          clauses.Add (newClause);
      }
    }

    /// <summary>
    /// If the last clause in <paramref name="clauses"/> is not a <see cref="SelectClause"/>, this method will create one and add it to the list
    /// of clauses.
    /// </summary>
    /// <param name="clauses">The clauses produced by the chain defined by <paramref name="lastNode"/>.</param>
    /// <param name="lastNode">The last node in the chain of <see cref="IExpressionNode"/>s.</param>
    /// <param name="clauseGenerationContext">The context info needed to resolve the created selector.</param>
    private void CreateSelectClauseIfNecessary (List<IClause> clauses, IExpressionNode lastNode, ClauseGenerationContext clauseGenerationContext)
    {
      var lastClause = clauses[clauses.Count - 1];
      if (!(lastClause is SelectClause))
      {
        var selectClause = lastNode.CreateSelectClause (lastClause, clauseGenerationContext);
        clauses.Add (selectClause);
      }
    }
  }
}
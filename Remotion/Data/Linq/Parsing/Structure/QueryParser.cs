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
          new QuerySourceClauseMapping (), 
          _expressionTreeParser.NodeTypeRegistry, 
          new SubQueryRegistry());

      IClause lastClause = CreateClauseChain (node, clauseGenerationContext);
      SelectClause selectClause = GetOrCreateSelectClause (node, lastClause, clauseGenerationContext);

      // TODO 1178: After COMMONS-1178, this code will not be needed any longer.
      var bodyClauses = new List<IBodyClause>();
      var findMainFromClause = FindMainFromClause (selectClause, bodyClauses);
      bodyClauses.Reverse (); // need to reverse the list of body clauses to have the last clause (nearest to the MainFromClause) first
      
      var queryModel = new QueryModel (expressionTreeRoot.Type, findMainFromClause, selectClause);
      foreach (var bodyClause in bodyClauses)
        queryModel.AddBodyClause (bodyClause);

      clauseGenerationContext.SubQueryRegistry.UpdateAllParentQueries (queryModel);

      queryModel.SetExpressionTree (expressionTreeRoot);
      return queryModel;
    }

    /// <summary>
    /// Recursively creates <see cref="IClause"/>s from a chain of <see cref="IExpressionNode"/>s, hooking up the clauses in the same
    /// order as the corresponding nodes.
    /// </summary>
    /// <param name="node">The last node in the chain to process.</param>
    /// <param name="clauseGenerationContext">A container for all the context information needed for creating clauses. This is used to resolve 
    /// predicates and selectors using <see cref="ExpressionResolver"/>s.</param>
    /// <returns>An <see cref="IClause"/> that corresponds to <paramref name="node"/>. The chain defined by <see cref="IExpressionNode.Source"/>
    /// is reflected in the chain defined by <see cref="IClause.PreviousClause"/>.</returns>
    private IClause CreateClauseChain (IExpressionNode node, ClauseGenerationContext clauseGenerationContext)
    {
      if (node.Source == null) // this is the end of the node chain, create a clause symbolizing the end of the clause chain
        return node.CreateClause (null, clauseGenerationContext);
      else
      { // this is not the end of the chain, process the rest of the chain before processing this node
        var previousClause = CreateClauseChain (node.Source, clauseGenerationContext);

        // TODO 1180: This is only temporary, we will implement a better model that does not require LetClause later on.
        if (previousClause is SelectClause)
          previousClause = CreateLetClauseFromSelectClause((SelectClause) previousClause);

        return node.CreateClause (previousClause, clauseGenerationContext);
      }
    }

    // TODO 1180: This is only temporary, we will implement a better model that does not require LetClause later on.
    // This creates a LetClause from an existing SelectClause if the SelectClause matches the pattern emitted by the C# compiler for "let" clauses.
    private IClause CreateLetClauseFromSelectClause (SelectClause selectClause)
    {
      // selectClause.Selector is usually something like "i => new { i = i, j = whatever }"
      // we take "j" as the letIdentifier, "whatever" as the letExpression, and the whole expression as the letProjection

      var newExpression = selectClause.Selector as NewExpression;
      if (newExpression != null && newExpression.Members.Count > 0)
      {
        Assertion.IsTrue (newExpression.Members.Count == newExpression.Arguments.Count, "This is ensured by Expression.New.");
        var letMember = newExpression.Members.Last();
        Assertion.IsTrue (letMember.Name.StartsWith ("get_"), 
            "C# emits a MethodInfo to the property getter as the member to access the assignment in a let expression");

        Expression letExpression = newExpression.Arguments.Last();
        ParameterExpression letIdentifier = Expression.Parameter (letExpression.Type, letMember.Name.Substring (4));
        LambdaExpression letProjection = Expression.Lambda (selectClause.Selector); // TODO: The lambda parameters aren't correct, but we'll remove the lambda anyway.
        return new LetClause (selectClause.PreviousClause, letIdentifier, letExpression, letProjection);
      }
      return selectClause;
    }

    /// <summary>
    /// Gets or create the <see cref="SelectClause"/> marking the end of the clause chain.
    /// </summary>
    /// <param name="lastNode">The last node in the chain of <see cref="IExpressionNode"/>s.</param>
    /// <param name="lastClause">The last clause produced by the chain defined by <paramref name="lastNode"/>.</param>
    /// <param name="clauseGenerationContext">The context info needed to resolve the created selector.</param>
    /// <returns><paramref name="lastClause"/> if it is a <see cref="SelectClause"/>, or a new <see cref="SelectClause"/> that references
    /// <paramref name="lastClause"/> as its <see cref="SelectClause.PreviousClause"/>. If a new <see cref="SelectClause"/> is created, its selector
    /// will take the data streamed out by <paramref name="lastClause"/> and return it unchanged.</returns>
    private SelectClause GetOrCreateSelectClause (IExpressionNode lastNode, IClause lastClause, ClauseGenerationContext clauseGenerationContext)
    {
      if (lastClause is SelectClause)
      {
        return (SelectClause) lastClause;
      }
      else
      {
        var parameterExpression = lastNode.CreateParameterForOutput();
        var resolvedParameterExpression = lastNode.Resolve (parameterExpression, parameterExpression, clauseGenerationContext);
        return new SelectClause (lastClause, resolvedParameterExpression);
      }
    }

    // TODO 1178: After COMMONS-1178, this code will not be needed any longer.
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
// Copyright (c) rubicon IT GmbH, www.rubicon.eu
//
// See the NOTICE file distributed with this work for additional information
// regarding copyright ownership.  rubicon licenses this file to you under 
// the Apache License, Version 2.0 (the "License"); you may not use this 
// file except in compliance with the License.  You may obtain a copy of the 
// License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, WITHOUT 
// WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.  See the 
// License for the specific language governing permissions and limitations
// under the License.
// 
using System;
using System.Linq;
using System.Linq.Expressions;
using Remotion.Linq.Clauses;
using Remotion.Linq.Parsing.ExpressionVisitors.Transformation;
using Remotion.Linq.Parsing.ExpressionVisitors.TreeEvaluation;
using Remotion.Linq.Parsing.Structure.IntermediateModel;
using Remotion.Utilities;

namespace Remotion.Linq.Parsing.Structure
{
  /// <summary>
  /// Takes an <see cref="Expression"/> tree and parses it into a <see cref="QueryModel"/> by use of an <see cref="ExpressionTreeParser"/>.
  /// It first transforms the <see cref="Expression"/> tree into a chain of <see cref="IExpressionNode"/> instances, and then calls 
  /// <see cref="MainSourceExpressionNode.CreateMainFromClause"/> and <see cref="IExpressionNode.Apply"/> in order to instantiate all the 
  /// <see cref="IClause"/>s. With those, a <see cref="QueryModel"/> is created and returned.
  /// </summary>
  public sealed class QueryParser : IQueryParser
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="QueryParser"/> class, using default parameters for parsing. 
    /// The <see cref="Structure.ExpressionTreeParser.NodeTypeProvider"/> used has all relevant methods of the <see cref="Queryable"/> class 
    /// automatically registered, and the <see cref="Structure.ExpressionTreeParser.Processor"/> comprises partial evaluation, and default 
    /// expression transformations. See <see cref="Structure.ExpressionTreeParser.CreateDefaultNodeTypeProvider"/>, 
    /// <see cref="Structure.ExpressionTreeParser.CreateDefaultProcessor"/>, and <see cref="ExpressionTransformerRegistry.CreateDefault"/>
    /// for details.
    /// </summary>
    /// <param name="evaluatableExpressionFilter">
    ///   Filter allowing to disable evaluation of independent subtrees even if they can potentially be evaluated; optional,
    ///   <see cref="NullEvaluatableExpressionFilter"/> is used by default.
    /// </param>
    public static QueryParser CreateDefault (IEvaluatableExpressionFilter evaluatableExpressionFilter = null)
    {
      var transformerRegistry = ExpressionTransformerRegistry.CreateDefault();
      var ensuredFilter = evaluatableExpressionFilter ?? new NullEvaluatableExpressionFilter();
      var expressionTreeParser = new ExpressionTreeParser (
          ExpressionTreeParser.CreateDefaultNodeTypeProvider (), 
          ExpressionTreeParser.CreateDefaultProcessor (transformerRegistry, ensuredFilter));
      return new QueryParser(expressionTreeParser);
    }

    private readonly ExpressionTreeParser _expressionTreeParser;

    /// <summary>
    /// Initializes a new instance of the <see cref="QueryParser"/> class, using the given <paramref name="expressionTreeParser"/> to
    /// convert <see cref="Expression"/> instances into <see cref="IExpressionNode"/>s. Use this constructor if you wish to customize the
    /// parser. To use a default parser (with the possibility to register custom node types), use the <see cref="CreateDefault"/> method.
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
    /// Gets the <see cref="INodeTypeProvider"/> used by <see cref="GetParsedQuery"/> to parse <see cref="MethodCallExpression"/> instances.
    /// </summary>
    /// <value>The node type registry.</value>
    public INodeTypeProvider NodeTypeProvider
    {
      get { return _expressionTreeParser.NodeTypeProvider; }
    }

    /// <summary>
    /// Gets the <see cref="IExpressionTreeProcessor"/> used by <see cref="GetParsedQuery"/> to process the <see cref="Expression"/> tree 
    /// before analyzing its structure.
    /// </summary>
    /// <value>The processor.</value>
    public IExpressionTreeProcessor Processor
    {
      get { return _expressionTreeParser.Processor; }
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
      var clauseGenerationContext = new ClauseGenerationContext (_expressionTreeParser.NodeTypeProvider);

      QueryModel queryModel = ApplyAllNodes (node, clauseGenerationContext);
      return queryModel;
    }

    /// <summary>
    /// Applies all nodes to a <see cref="QueryModel"/>, which is created by the trailing <see cref="MainSourceExpressionNode"/> in the 
    /// <paramref name="node"/> chain.
    /// </summary>
    /// <param name="node">The entry point to the <see cref="IExpressionNode"/> chain.</param>
    /// <param name="clauseGenerationContext">The clause generation context collecting context information during the parsing process.</param>
    /// <returns>A <see cref="QueryModel"/> created by the training <see cref="MainSourceExpressionNode"/> and transformed by each node in the
    /// <see cref="IExpressionNode"/> chain.</returns>
    private QueryModel ApplyAllNodes (IExpressionNode node, ClauseGenerationContext clauseGenerationContext)
    {
      QueryModel queryModel = null;
      if (node.Source != null)
        queryModel = ApplyAllNodes (node.Source, clauseGenerationContext);

      return node.Apply (queryModel, clauseGenerationContext);
    }
  }
}

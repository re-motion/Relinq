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
using System.Linq.Expressions;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Utilities;

namespace Remotion.Linq.Parsing.Structure.IntermediateModel
{
  /// <summary>
  /// Base class for <see cref="IExpressionNode"/> implementations that represent instantiations of <see cref="MethodCallExpression"/>.
  /// </summary>
  public abstract class MethodCallExpressionNodeBase : IExpressionNode
  {
    private IExpressionNode _source;
    private readonly Type _nodeResultType;
    private readonly string _associatedIdentifier;

    protected MethodCallExpressionNodeBase (MethodCallExpressionParseInfo parseInfo)
    {
      if (parseInfo.AssociatedIdentifier == null)
        throw new ArgumentException ("Unitialized struct.", "parseInfo");

      _associatedIdentifier = parseInfo.AssociatedIdentifier;
      _source = parseInfo.Source;
      _nodeResultType = parseInfo.ParsedExpression.Type;
    }

    public string AssociatedIdentifier
    {
      get { return _associatedIdentifier; }
    }

    public IExpressionNode Source
    {
      get { return _source; }
    }

    public Type NodeResultType
    {
      get { return _nodeResultType; }
    }

    public abstract Expression Resolve (
        ParameterExpression inputParameter, Expression expressionToBeResolved, ClauseGenerationContext clauseGenerationContext);

    protected abstract void ApplyNodeSpecificSemantics (QueryModel queryModel, ClauseGenerationContext clauseGenerationContext);

    public QueryModel Apply (QueryModel queryModel, ClauseGenerationContext clauseGenerationContext)
    {
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);

      queryModel = WrapQueryModelAfterEndOfQuery (queryModel, clauseGenerationContext);
      ApplyNodeSpecificSemantics (queryModel, clauseGenerationContext);
      SetResultTypeOverride (queryModel);
      return queryModel;
    }

    /// <summary>
    /// Wraps the <paramref name="queryModel"/> into a subquery after a node that indicates the end of the query (
    /// <see cref="ResultOperatorExpressionNodeBase"/> or <see cref="GroupByExpressionNode"/>). Override this method
    /// when implementing a <see cref="IExpressionNode"/> that does not need a subquery to be created if it occurs after the query end.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When an ordinary node follows a result operator or group node, it cannot simply append its clauses to the <paramref name="queryModel"/> 
    /// because semantically, the result operator (or grouping) must be executed _before_ the clause. Therefore, in such scenarios, we wrap 
    /// the current query model into a <see cref="SubQueryExpression"/> that we put into the <see cref="MainFromClause"/> of a new 
    /// <see cref="QueryModel"/>.
    /// </para>
    /// <para>
    /// This method also changes the <see cref="Source"/> of this node because logically, all <see cref="Resolve"/> operations must be handled
    /// by the new <see cref="MainFromClause"/> holding the <see cref="SubQueryExpression"/>. For example, consider the following call chain:
    /// <code>
    /// MainSource (...)
    ///   .Select (x => x)
    ///   .Distinct ()
    ///   .Select (x => x)
    /// </code>
    /// 
    /// Naively, the last Select node would resolve (via Distinct and Select) to the <see cref="MainFromClause"/> created by the initial MainSource.
    /// After this method is executed, however, that <see cref="MainFromClause"/> is part of the sub query, and a new <see cref="MainFromClause"/> 
    /// has been created to hold it. Therefore, we replace the chain as follows:
    /// <code>
    /// MainSource (MainSource (...).Select (x => x).Distinct ())
    ///   .Select (x => x)
    /// </code>
    /// 
    /// Now, the last Select node resolves to the new <see cref="MainFromClause"/>.
    /// </para>
    /// </remarks>
    protected virtual QueryModel WrapQueryModelAfterEndOfQuery (QueryModel queryModel, ClauseGenerationContext clauseGenerationContext)
    {
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);

      var sourceAsResultOperatorNode = _source as ResultOperatorExpressionNodeBase;
      if (sourceAsResultOperatorNode != null)
        return WrapQueryModel(queryModel, sourceAsResultOperatorNode.AssociatedIdentifier, clauseGenerationContext);
      else
        return queryModel;
    }

    /// <summary>
    /// Sets the result type override of the given <see cref="QueryModel"/>.
    /// </summary>
    /// <param name="queryModel">The query model to set the <see cref="QueryModel.ResultTypeOverride"/> of.</param>
    /// <remarks>
    /// By default, the result type override is set to <see cref="NodeResultType"/> in the <see cref="Apply"/> method. This ensures that the query
    /// model represents the type of the query correctly. Specific node parsers can override this method to set the 
    /// <see cref="QueryModel.ResultTypeOverride"/> to another value, or to clear it (set it to <see langword="null" />). Do not leave the
    /// <see cref="QueryModel.ResultTypeOverride"/> unchanged when overriding this method, as a source node might have set it to a value that doesn't 
    /// fit this node.
    /// </remarks>
    protected virtual void SetResultTypeOverride (QueryModel queryModel)
    {
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);

      queryModel.ResultTypeOverride = NodeResultType;
    }

    private QueryModel WrapQueryModel (QueryModel queryModel, string associatedIdentifier, ClauseGenerationContext clauseGenerationContext)
    {
      var subQueryExpression = new SubQueryExpression (queryModel);

      // change the Source of this node so that Resolve will later correctly go to the new main from clause we create for the sub query
      var newMainSourceNode = new MainSourceExpressionNode (associatedIdentifier, subQueryExpression);
      _source = newMainSourceNode;

      return newMainSourceNode.Apply (null, clauseGenerationContext);
    }

    protected NotSupportedException CreateResolveNotSupportedException ()
    {
      return new NotSupportedException (
          GetType().Name + " does not support resolving of expressions, because it does not stream any data to the following node.");
    }

    protected NotSupportedException CreateOutputParameterNotSupportedException ()
    {
      return new NotSupportedException (
          GetType().Name + " does not support creating a parameter for its output because it does not stream any data to the following node.");
    }
  }
}

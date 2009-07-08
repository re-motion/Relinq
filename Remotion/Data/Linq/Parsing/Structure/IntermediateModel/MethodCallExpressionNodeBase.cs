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
using System.Reflection;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.Structure.IntermediateModel
{
  /// <summary>
  /// Base class for <see cref="IExpressionNode"/> implementations that represent instantiations of <see cref="MethodCallExpression"/>.
  /// </summary>
  public abstract class MethodCallExpressionNodeBase : IExpressionNode
  {
    /// <summary>
    /// Gets the <see cref="MethodInfo"/> from a given <see cref="LambdaExpression"/> that has to wrap a <see cref="MethodCallExpression"/>.
    /// If the method is a generic method, its open generic method definition is returned.
    /// This method can be used for registration of the node type with an <see cref="MethodCallExpressionNodeTypeRegistry"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="methodCall">The method call.</param>
    /// <returns></returns>
    protected static MethodInfo GetSupportedMethod<T> (Expression<Func<T>> methodCall)
    {
      ArgumentUtility.CheckNotNull ("methodCall", methodCall);

      var method = ParserUtility.GetMethod (methodCall);
      return method.IsGenericMethod ? method.GetGenericMethodDefinition() : method;
    }

    protected MethodCallExpressionNodeBase (MethodCallExpressionParseInfo parseInfo)
    {
      AssociatedIdentifier = parseInfo.AssociatedIdentifier;
      Source = parseInfo.Source;
      ParsedExpression = parseInfo.ParsedExpression;
    }

    public string AssociatedIdentifier { get; private set; }
    public IExpressionNode Source { get; private set; }
    public MethodCallExpression ParsedExpression { get; private set; }

    Expression IExpressionNode.ParsedExpression
    {
      get { return ParsedExpression; }
    }

    public abstract Expression Resolve (
        ParameterExpression inputParameter, Expression expressionToBeResolved, ClauseGenerationContext clauseGenerationContext);

    protected abstract QueryModel ApplyNodeSpecificSemantics (QueryModel queryModel, ClauseGenerationContext clauseGenerationContext);

    public QueryModel Apply (QueryModel queryModel, ClauseGenerationContext clauseGenerationContext)
    {
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);

      queryModel = WrapQueryModelAfterResultOperator (queryModel, clauseGenerationContext);
      queryModel = ApplyNodeSpecificSemantics (queryModel, clauseGenerationContext);
      return queryModel;
    }

    /// <summary>
    /// Wraps the <paramref name="queryModel"/> into a subquery after a <see cref="ResultOperatorBase"/> has been created. Override this method
    /// when implementing a <see cref="IExpressionNode"/> that does not need a subquery to be created if it occurs after a 
    /// <see cref="ResultOperatorBase"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When an ordinary node 
    /// follows a result operator node,  it cannot simply append its clauses to the <paramref name="queryModel"/> because semantically, the 
    /// result operator must be executed _before_ the clause. Therefore, in such scenarios, we wrap the current query model into a 
    /// <see cref="SubQueryExpression"/> that we put into the <see cref="MainFromClause"/> of a new <see cref="QueryModel"/>.
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
    protected virtual QueryModel WrapQueryModelAfterResultOperator (QueryModel queryModel, ClauseGenerationContext clauseGenerationContext)
    {
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);

      if (queryModel.ResultOperators.Count > 0)
      {
        var oldResultType = queryModel.ResultType;
        queryModel.ResultType = Source.ParsedExpression.Type;
            // the result type of the old query model is what the last node's expression says it should be

        var subQueryExpression = new SubQueryExpression (queryModel);

        // change the Source of this node so that Resolve will later correctly go to the new main from clause we create for the sub query
        var newMainSourceNode = new MainSourceExpressionNode (Source.AssociatedIdentifier, subQueryExpression);
        Source = newMainSourceNode;

        var newMainFromClause = newMainSourceNode.CreateMainFromClause (clauseGenerationContext);
        var newSelectClause = new SelectClause (new QuerySourceReferenceExpression (newMainFromClause));
        return new QueryModel (oldResultType, newMainFromClause, newSelectClause);
            // the new query model gets the result type the old query model would have gotten, hadn't it been wrapped
      }
      else
        return queryModel;
    }

    protected InvalidOperationException CreateResolveNotSupportedException ()
    {
      return
          new InvalidOperationException (
              GetType().Name + " does not support resolving of expressions, because it does not stream any data to the following node.");
    }

    protected InvalidOperationException CreateOutputParameterNotSupportedException ()
    {
      return
          new InvalidOperationException (
              GetType().Name + " does not support creating a parameter for its output because it does not stream any data to the following node.");
    }
  }
}
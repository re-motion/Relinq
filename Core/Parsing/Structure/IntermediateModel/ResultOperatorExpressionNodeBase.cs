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
using Remotion.Utilities;

namespace Remotion.Linq.Parsing.Structure.IntermediateModel
{
  /// <summary>
  /// Acts as a base class for <see cref="IExpressionNode"/>s standing for <see cref="MethodCallExpression"/>s that operate on the result of the query
  /// rather than representing actual clauses, such as <see cref="CountExpressionNode"/> or <see cref="DistinctExpressionNode"/>.
  /// </summary>
  public abstract class ResultOperatorExpressionNodeBase : MethodCallExpressionNodeBase
  {
    private readonly MethodCallExpression _parsedExpression;

    protected ResultOperatorExpressionNodeBase (
        MethodCallExpressionParseInfo parseInfo, LambdaExpression optionalPredicate, LambdaExpression optionalSelector)
        : base (TransformParseInfo (parseInfo, optionalPredicate, optionalSelector))
    {
      if (optionalPredicate != null && optionalPredicate.Parameters.Count != 1)
        throw new ArgumentException ("OptionalPredicate must have exactly one parameter.", "optionalPredicate");

      if (optionalSelector != null && optionalSelector.Parameters.Count != 1)
        throw new ArgumentException ("OptionalSelector must have exactly one parameter.", "optionalSelector");

      _parsedExpression = parseInfo.ParsedExpression;
    }

    protected abstract ResultOperatorBase CreateResultOperator (ClauseGenerationContext clauseGenerationContext);

    public MethodCallExpression ParsedExpression
    {
      get { return _parsedExpression; }
    }

    protected override void ApplyNodeSpecificSemantics (QueryModel queryModel, ClauseGenerationContext clauseGenerationContext)
    {
      //NOTE: Do not seal ApplyNodeSpecificSemantics() in ResultOperatorExpressionNodeBase. It is overridden by e.g. Fetch-operators.
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);

      var resultOperator = CreateResultOperator (clauseGenerationContext);
      queryModel.ResultOperators.Add (resultOperator);
    }

    protected sealed override QueryModel WrapQueryModelAfterEndOfQuery (QueryModel queryModel, ClauseGenerationContext clauseGenerationContext)
    {
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);

      // Result operators can safely be appended to the previous query model even after another result operator, so do not wrap the previous
      // query model.
      return queryModel;
    }

    private static MethodCallExpressionParseInfo TransformParseInfo (
        MethodCallExpressionParseInfo parseInfo,
        LambdaExpression optionalPredicate,
        LambdaExpression optionalSelector)
    {
      var source = parseInfo.Source;

      if (optionalPredicate != null)
        source = new WhereExpressionNode (parseInfo, optionalPredicate);

      if (optionalSelector != null)
      {
        var newParseInfo = new MethodCallExpressionParseInfo (parseInfo.AssociatedIdentifier, source, parseInfo.ParsedExpression);
        source = new SelectExpressionNode (newParseInfo, optionalSelector);
      }

      return new MethodCallExpressionParseInfo (parseInfo.AssociatedIdentifier, source, parseInfo.ParsedExpression);
    }
  }
}

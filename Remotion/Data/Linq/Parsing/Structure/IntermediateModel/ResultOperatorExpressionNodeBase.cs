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
using Remotion.Data.Linq.Clauses;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.Structure.IntermediateModel
{
  /// <summary>
  /// Acts as a base class for <see cref="IExpressionNode"/>s standing for <see cref="MethodCallExpression"/>s that operate on the result of the query
  /// rather than representing actual clauses, such as <see cref="CountExpressionNode"/> or <see cref="DistinctExpressionNode"/>.
  /// </summary>
  public abstract class ResultOperatorExpressionNodeBase : MethodCallExpressionNodeBase
  {
    protected ResultOperatorExpressionNodeBase (
        MethodCallExpressionParseInfo parseInfo, LambdaExpression optionalPredicate, LambdaExpression optionalSelector)
        : base (parseInfo)
    {
      if (optionalPredicate != null && optionalPredicate.Parameters.Count != 1)
        throw new ArgumentException ("OptionalPredicate must have exactly one parameter.", "optionalPredicate");

      if (optionalSelector != null && optionalSelector.Parameters.Count != 1)
        throw new ArgumentException ("OptionalSelector must have exactly one parameter.", "optionalSelector");

      if (optionalPredicate != null)
        Source = new WhereExpressionNode (parseInfo, optionalPredicate);

      if (optionalSelector != null)
      {
        var newParseInfo = new MethodCallExpressionParseInfo (parseInfo.AssociatedIdentifier, Source, parseInfo.ParsedExpression);
        Source = new SelectExpressionNode (newParseInfo, optionalSelector);
      }

      ParsedExpression = parseInfo.ParsedExpression;
    }

    protected abstract ResultOperatorBase CreateResultOperator (ClauseGenerationContext clauseGenerationContext);

    public MethodCallExpression ParsedExpression { get; private set; }

    protected override QueryModel ApplyNodeSpecificSemantics (QueryModel queryModel, ClauseGenerationContext clauseGenerationContext)
    {
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);

      var resultOperator = CreateResultOperator (clauseGenerationContext);
      queryModel.ResultOperators.Add (resultOperator);
      return queryModel;
    }

    protected override QueryModel WrapQueryModelAfterEndOfQuery (QueryModel queryModel, ClauseGenerationContext clauseGenerationContext)
    {
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);

      // Result operators can safely be appended to the previous query model even after another result operator, so do not wrap the previous
      // query model.
      return queryModel;
    }
  }
}
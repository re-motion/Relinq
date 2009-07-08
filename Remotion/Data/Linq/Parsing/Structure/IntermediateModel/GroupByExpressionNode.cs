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
using System.Reflection;
using Remotion.Utilities;
using Remotion.Data.Linq.Clauses;

namespace Remotion.Data.Linq.Parsing.Structure.IntermediateModel
{
  public class GroupByExpressionNode : MethodCallExpressionNodeBase
  {
    public static readonly MethodInfo[] SupportedMethods = new[]
                                                           {
                                                               GetSupportedMethod (() => Queryable.GroupBy<object, object> (null, o => null)),
                                                               GetSupportedMethod (() => Enumerable.GroupBy<object, object> (null, o => null)),
                                                               GetSupportedMethod (() => Queryable.GroupBy<object, object, object> (null, o => null, o => null)),
                                                               GetSupportedMethod (() => Enumerable.GroupBy<object, object, object> (null, o => null, o => null)),
                                                           };

    private readonly ResolvedExpressionCache _cachedKeySelector;
    private readonly ResolvedExpressionCache _cachedElementSelector;

    public GroupByExpressionNode (MethodCallExpressionParseInfo parseInfo, LambdaExpression keySelector, LambdaExpression elementSelector)
        : base (parseInfo)
    {
      ArgumentUtility.CheckNotNull ("keySelector", keySelector);
      ArgumentUtility.CheckNotNull ("elementSelector", elementSelector);

      if (keySelector.Parameters.Count != 1)
        throw new ArgumentException ("KeySelector must have exactly one parameter.", "keySelector");

      if (elementSelector.Parameters.Count != 1)
        throw new ArgumentException ("ElementSelector must have exactly one parameter.", "elementSelector");

      KeySelector = keySelector;
      ElementSelector = elementSelector;

      _cachedKeySelector = new ResolvedExpressionCache (this);
      _cachedElementSelector = new ResolvedExpressionCache (this);
    }

    public LambdaExpression KeySelector { get; private set; }
    public LambdaExpression ElementSelector { get; private set; }

    public Expression GetResolvedKeySelector (ClauseGenerationContext clauseGenerationContext)
    {
      return _cachedKeySelector.GetOrCreate (r => r.GetResolvedExpression (KeySelector.Body, KeySelector.Parameters[0], clauseGenerationContext));
    }

    public Expression GetResolvedElementSelector (ClauseGenerationContext clauseGenerationContext)
    {
      return _cachedElementSelector.GetOrCreate (r => r.GetResolvedExpression (ElementSelector.Body, ElementSelector.Parameters[0], clauseGenerationContext));
    }

    public override Expression Resolve (ParameterExpression inputParameter, Expression expressionToBeResolved, ClauseGenerationContext clauseGenerationContext)
    {
      throw new NotImplementedException();
    }

    protected override QueryModel ApplyNodeSpecificSemantics (QueryModel queryModel, ClauseGenerationContext clauseGenerationContext)
    {
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);
      queryModel.SelectOrGroupClause = 
          new GroupClause (GetResolvedElementSelector (clauseGenerationContext), GetResolvedKeySelector (clauseGenerationContext));
      return queryModel;
    }
  }
}
// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// re-linq is free software; you can redistribute it and/or modify it under 
// the terms of the GNU Lesser General Public License as published by the 
// Free Software Foundation; either version 2.1 of the License, 
// or (at your option) any later version.
// 
// re-linq is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-linq; if not, see http://www.gnu.org/licenses.
// 
using System;
using System.Linq.Expressions;
using Remotion.Linq.Clauses;
using Remotion.Linq.Parsing.Structure.IntermediateModel;

namespace Remotion.Linq.UnitTests.Linq.Core.Parsing.Structure.IntermediateModel.TestDomain
{
  public class TestMethodCallExpressionNode : MethodCallExpressionNodeBase
  {
    private readonly IBodyClause _bodyClauseToAddInApply;

    public TestMethodCallExpressionNode (MethodCallExpressionParseInfo parseInfo, IBodyClause bodyClauseToAddInApply)
        : base (parseInfo)
    {
      _bodyClauseToAddInApply = bodyClauseToAddInApply;
    }

    public override Expression Resolve (
        ParameterExpression inputParameter, Expression expressionToBeResolved, ClauseGenerationContext clauseGenerationContext)
    {
      return Source.Resolve (inputParameter, expressionToBeResolved, clauseGenerationContext);
    }

    protected override QueryModel ApplyNodeSpecificSemantics (QueryModel queryModel, ClauseGenerationContext clauseGenerationContext)
    {
      queryModel.BodyClauses.Add (_bodyClauseToAddInApply);
      return queryModel;
    }
  }
}

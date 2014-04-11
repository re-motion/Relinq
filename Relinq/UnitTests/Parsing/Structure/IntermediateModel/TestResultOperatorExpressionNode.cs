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
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Parsing.Structure.IntermediateModel;

namespace Remotion.Linq.UnitTests.Parsing.Structure.IntermediateModel
{
  public class TestResultOperatorExpressionNode : ResultOperatorExpressionNodeBase
  {
    public TestResultOperatorExpressionNode (
        MethodCallExpressionParseInfo parseInfo, LambdaExpression optionalPredicate, LambdaExpression optionalSelector)
        : base (parseInfo, optionalPredicate, optionalSelector)
    {
    }

    public override Expression Resolve (
        ParameterExpression inputParameter, Expression expressionToBeResolved, ClauseGenerationContext clauseGenerationContext)
    {
      throw new NotImplementedException();
    }

    protected override ResultOperatorBase CreateResultOperator (ClauseGenerationContext clauseGenerationContext)
    {
      return new TakeResultOperator (Expression.Constant (100));
    }
  }
}

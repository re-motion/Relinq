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
using Remotion.Linq.Parsing.Structure.IntermediateModel;

namespace Remotion.Linq.UnitTests.Parsing.Structure.IntermediateModel.TestDomain
{
  public class ExpressionNodeWithTooManyCtors : IExpressionNode
  {
    public ExpressionNodeWithTooManyCtors ()
    {
    }

// ReSharper disable UnusedParameter.Local
    public ExpressionNodeWithTooManyCtors (int i)
// ReSharper restore UnusedParameter.Local
    {
    }

    public IExpressionNode Source
    {
      get { throw new NotImplementedException(); }
    }

    public Expression ParsedExpression
    {
      get { throw new NotImplementedException(); }
    }

    public string AssociatedIdentifier
    {
      get { throw new NotImplementedException(); }
    }

    public Expression Resolve (ParameterExpression inputParameter, Expression expressionToBeResolved, ClauseGenerationContext clauseGenerationContext)
    {
      throw new NotImplementedException();
    }

    public IClause CreateClause (IClause previousClause, ClauseGenerationContext clauseGenerationContext)
    {
      throw new NotImplementedException();
    }

    public SelectClause CreateSelectClause (IClause previousClause, ClauseGenerationContext clauseGenerationContext)
    {
      throw new NotImplementedException();
    }

    public QueryModel Apply (QueryModel queryModel, ClauseGenerationContext clauseGenerationContext)
    {
      throw new NotImplementedException();
    }
  }
}
